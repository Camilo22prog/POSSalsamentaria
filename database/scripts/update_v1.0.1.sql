-- =============================================================
-- update_v1.0.1.sql
-- Actualización acumulativa desde v1.0.0 → v1.0.1
-- Cubre todas las migraciones EF Core aplicadas en este sistema.
-- Es IDEMPOTENTE: puede ejecutarse aunque algunos cambios ya
-- existan; los omitirá sin error.
-- =============================================================

USE POSSalsamentaria;
GO

-- Verificar si ya fue aplicada
IF EXISTS (SELECT 1 FROM __DatabaseVersion WHERE Version = '1.0.1')
BEGIN
    PRINT 'La versión 1.0.1 ya fue aplicada. No se requieren cambios.';
    RETURN;
END
GO

BEGIN TRANSACTION;

BEGIN TRY

    PRINT '========================================';
    PRINT 'Iniciando actualización a v1.0.1';
    PRINT '========================================';

    -- ============================================================
    -- 1. TABLA Productos
    -- ============================================================

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Productos') AND name = 'TipoIVA')
    BEGIN
        ALTER TABLE Productos ADD TipoIVA INT NOT NULL DEFAULT 0;
        PRINT '✓ Productos.TipoIVA agregado';
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Productos') AND name = 'VentaPorPeso')
    BEGIN
        ALTER TABLE Productos ADD VentaPorPeso BIT NOT NULL DEFAULT 0;
        PRINT '✓ Productos.VentaPorPeso agregado';
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Productos') AND name = 'PrecioPorKilo')
    BEGIN
        ALTER TABLE Productos ADD PrecioPorKilo DECIMAL(18,2) NOT NULL DEFAULT 0;
        PRINT '✓ Productos.PrecioPorKilo agregado';
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Productos') AND name = 'CategoriaPeso')
    BEGIN
        ALTER TABLE Productos ADD CategoriaPeso INT NULL;
        PRINT '✓ Productos.CategoriaPeso agregado';
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Productos') AND name = 'ProveedorId')
    BEGIN
        ALTER TABLE Productos ADD ProveedorId INT NULL;
        PRINT '✓ Productos.ProveedorId agregado';
    END

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('Productos') AND name = 'IX_Productos_ProveedorId')
    BEGIN
        CREATE INDEX IX_Productos_ProveedorId ON Productos(ProveedorId);
        PRINT '✓ Índice IX_Productos_ProveedorId creado';
    END

    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Productos_Proveedores_ProveedorId')
    BEGIN
        ALTER TABLE Productos
            ADD CONSTRAINT FK_Productos_Proveedores_ProveedorId
            FOREIGN KEY (ProveedorId) REFERENCES Proveedores(Id);
        PRINT '✓ FK Productos → Proveedores agregado';
    END

    -- ============================================================
    -- 2. TABLA MovimientosCaja
    -- ============================================================

    -- Cambiar Tipo de nvarchar a INT (solo si sigue siendo nvarchar)
    IF EXISTS (
        SELECT 1 FROM sys.columns c
        JOIN sys.types t ON c.user_type_id = t.user_type_id
        WHERE c.object_id = OBJECT_ID('MovimientosCaja')
          AND c.name = 'Tipo'
          AND t.name IN ('nvarchar','varchar')
    )
    BEGIN
        -- Limpiar datos no convertibles antes de cambiar el tipo
        UPDATE MovimientosCaja SET Tipo = '0' WHERE TRY_CAST(Tipo AS INT) IS NULL;
        ALTER TABLE MovimientosCaja ALTER COLUMN Tipo INT NOT NULL;
        PRINT '✓ MovimientosCaja.Tipo convertido a INT';
    END

    -- Ampliar Concepto a nvarchar(500)
    IF EXISTS (
        SELECT 1 FROM sys.columns c
        JOIN sys.types t ON c.user_type_id = t.user_type_id
        WHERE c.object_id = OBJECT_ID('MovimientosCaja')
          AND c.name = 'Concepto'
          AND t.name = 'nvarchar'
          AND c.max_length < 1000   -- 500 chars × 2 bytes = 1000
    )
    BEGIN
        ALTER TABLE MovimientosCaja ALTER COLUMN Concepto NVARCHAR(500) NOT NULL;
        PRINT '✓ MovimientosCaja.Concepto ampliado a nvarchar(500)';
    END

    -- Eliminar columna Observaciones si existe
    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('MovimientosCaja') AND name = 'Observaciones')
    BEGIN
        ALTER TABLE MovimientosCaja DROP COLUMN Observaciones;
        PRINT '✓ MovimientosCaja.Observaciones eliminado';
    END

    -- Agregar columna Referencia
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('MovimientosCaja') AND name = 'Referencia')
    BEGIN
        ALTER TABLE MovimientosCaja ADD Referencia NVARCHAR(100) NULL;
        PRINT '✓ MovimientosCaja.Referencia agregado';
    END

    -- Recrear FK con Restrict (si existe como Cascade, se elimina y recrea)
    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_MovimientosCaja_Cajas_CajaId')
    BEGIN
        DECLARE @DeleteRule_MC_Caja INT;
        SELECT @DeleteRule_MC_Caja = delete_referential_action
        FROM sys.foreign_keys WHERE name = 'FK_MovimientosCaja_Cajas_CajaId';
        IF @DeleteRule_MC_Caja <> 1  -- 1 = NO ACTION/RESTRICT
        BEGIN
            ALTER TABLE MovimientosCaja DROP CONSTRAINT FK_MovimientosCaja_Cajas_CajaId;
            ALTER TABLE MovimientosCaja
                ADD CONSTRAINT FK_MovimientosCaja_Cajas_CajaId
                FOREIGN KEY (CajaId) REFERENCES Cajas(Id) ON DELETE NO ACTION;
            PRINT '✓ FK_MovimientosCaja_Cajas_CajaId cambiado a RESTRICT';
        END
    END

    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_MovimientosCaja_Usuarios_UsuarioId')
    BEGIN
        DECLARE @DeleteRule_MC_User INT;
        SELECT @DeleteRule_MC_User = delete_referential_action
        FROM sys.foreign_keys WHERE name = 'FK_MovimientosCaja_Usuarios_UsuarioId';
        IF @DeleteRule_MC_User <> 1
        BEGIN
            ALTER TABLE MovimientosCaja DROP CONSTRAINT FK_MovimientosCaja_Usuarios_UsuarioId;
            ALTER TABLE MovimientosCaja
                ADD CONSTRAINT FK_MovimientosCaja_Usuarios_UsuarioId
                FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id) ON DELETE NO ACTION;
            PRINT '✓ FK_MovimientosCaja_Usuarios_UsuarioId cambiado a RESTRICT';
        END
    END

    -- ============================================================
    -- 3. TABLA Ventas — renombrar columnas
    -- ============================================================

    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Ventas') AND name = 'Impuesto')
    BEGIN
        EXEC sp_rename 'Ventas.Impuesto', 'IVATotal', 'COLUMN';
        PRINT '✓ Ventas.Impuesto renombrado a IVATotal';
    END

    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Ventas') AND name = 'Descuento')
    BEGIN
        EXEC sp_rename 'Ventas.Descuento', 'DescuentoTotal', 'COLUMN';
        PRINT '✓ Ventas.Descuento renombrado a DescuentoTotal';
    END

    -- Eliminar campo antiguo AnuladaPorId (fue reemplazado por AnuladaPorUsuarioId)
    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Ventas') AND name = 'AnuladaPorId')
    BEGIN
        ALTER TABLE Ventas DROP COLUMN AnuladaPorId;
        PRINT '✓ Ventas.AnuladaPorId eliminado';
    END

    -- FechaAnulacion: eliminar si existía (será re-creado limpio)
    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Ventas') AND name = 'FechaAnulacion')
    BEGIN
        ALTER TABLE Ventas DROP COLUMN FechaAnulacion;
        PRINT '✓ Ventas.FechaAnulacion eliminado (se recreará)';
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Ventas') AND name = 'AnuladaPorUsuarioId')
    BEGIN
        ALTER TABLE Ventas ADD AnuladaPorUsuarioId INT NULL;
        PRINT '✓ Ventas.AnuladaPorUsuarioId agregado';
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Ventas') AND name = 'FechaAnulacion')
    BEGIN
        ALTER TABLE Ventas ADD FechaAnulacion DATETIME2 NULL;
        PRINT '✓ Ventas.FechaAnulacion agregado';
    END

    -- ============================================================
    -- 4. TABLA Pagos — renombrar columna
    -- ============================================================

    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Pagos') AND name = 'MetodoPago')
    BEGIN
        EXEC sp_rename 'Pagos.MetodoPago', 'Metodo', 'COLUMN';
        PRINT '✓ Pagos.MetodoPago renombrado a Metodo';
    END

    -- ============================================================
    -- 5. TABLA DetallesVenta
    -- ============================================================

    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('DetallesVenta') AND name = 'Observaciones')
    BEGIN
        ALTER TABLE DetallesVenta DROP COLUMN Observaciones;
        PRINT '✓ DetallesVenta.Observaciones eliminado';
    END

    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('DetallesVenta') AND name = 'PesoVendido')
    BEGIN
        ALTER TABLE DetallesVenta DROP COLUMN PesoVendido;
        PRINT '✓ DetallesVenta.PesoVendido eliminado';
    END

    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('DetallesVenta') AND name = 'PrecioFinal')
    BEGIN
        ALTER TABLE DetallesVenta DROP COLUMN PrecioFinal;
        PRINT '✓ DetallesVenta.PrecioFinal eliminado';
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('DetallesVenta') AND name = 'MontoIVA')
    BEGIN
        ALTER TABLE DetallesVenta ADD MontoIVA DECIMAL(18,2) NOT NULL DEFAULT 0;
        PRINT '✓ DetallesVenta.MontoIVA agregado';
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('DetallesVenta') AND name = 'PorcentajeIVA')
    BEGIN
        ALTER TABLE DetallesVenta ADD PorcentajeIVA DECIMAL(5,2) NOT NULL DEFAULT 0;
        PRINT '✓ DetallesVenta.PorcentajeIVA agregado';
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('DetallesVenta') AND name = 'Total')
    BEGIN
        ALTER TABLE DetallesVenta ADD Total DECIMAL(18,2) NOT NULL DEFAULT 0;
        PRINT '✓ DetallesVenta.Total agregado';
    END

    -- ============================================================
    -- 6. TABLA Cajas
    -- ============================================================

    -- Eliminar índices que ya no aplican
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('Cajas') AND name = 'IX_Cajas_NumeroCaja')
    BEGIN
        DROP INDEX IX_Cajas_NumeroCaja ON Cajas;
        PRINT '✓ Índice IX_Cajas_NumeroCaja eliminado';
    END

    IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('Cajas') AND name = 'IX_Cajas_FechaApertura')
    BEGIN
        DROP INDEX IX_Cajas_FechaApertura ON Cajas;
        PRINT '✓ Índice IX_Cajas_FechaApertura eliminado';
    END

    -- Eliminar columna NumeroCaja (requiere eliminar índice único primero si existe)
    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Cajas') AND name = 'NumeroCaja')
    BEGIN
        ALTER TABLE Cajas DROP COLUMN NumeroCaja;
        PRINT '✓ Cajas.NumeroCaja eliminado';
    END

    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Cajas') AND name = 'ObservacionesCierre')
    BEGIN
        ALTER TABLE Cajas DROP COLUMN ObservacionesCierre;
        PRINT '✓ Cajas.ObservacionesCierre eliminado';
    END

    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Cajas') AND name = 'TotalOtros')
    BEGIN
        ALTER TABLE Cajas DROP COLUMN TotalOtros;
        PRINT '✓ Cajas.TotalOtros eliminado';
    END

    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Cajas') AND name = 'TotalVentas')
    BEGIN
        ALTER TABLE Cajas DROP COLUMN TotalVentas;
        PRINT '✓ Cajas.TotalVentas eliminado';
    END

    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Cajas') AND name = 'Estado')
    BEGIN
        ALTER TABLE Cajas DROP COLUMN Estado;
        PRINT '✓ Cajas.Estado eliminado';
    END

    -- Columna UsuarioId1 temporal (fue agregada y luego eliminada en migraciones posteriores)
    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Cajas_Usuarios_UsuarioId1')
        ALTER TABLE Cajas DROP CONSTRAINT FK_Cajas_Usuarios_UsuarioId1;
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('Cajas') AND name = 'IX_Cajas_UsuarioId1')
        DROP INDEX IX_Cajas_UsuarioId1 ON Cajas;
    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Cajas') AND name = 'UsuarioId1')
    BEGIN
        ALTER TABLE Cajas DROP COLUMN UsuarioId1;
        PRINT '✓ Cajas.UsuarioId1 eliminado';
    END

    -- Hacer MontoFinal y Diferencia NOT NULL
    IF EXISTS (
        SELECT 1 FROM sys.columns
        WHERE object_id = OBJECT_ID('Cajas') AND name = 'MontoFinal' AND is_nullable = 1
    )
    BEGIN
        UPDATE Cajas SET MontoFinal = 0 WHERE MontoFinal IS NULL;
        ALTER TABLE Cajas ALTER COLUMN MontoFinal DECIMAL(18,2) NOT NULL;
        PRINT '✓ Cajas.MontoFinal cambiado a NOT NULL';
    END

    IF EXISTS (
        SELECT 1 FROM sys.columns
        WHERE object_id = OBJECT_ID('Cajas') AND name = 'Diferencia' AND is_nullable = 1
    )
    BEGIN
        UPDATE Cajas SET Diferencia = 0 WHERE Diferencia IS NULL;
        ALTER TABLE Cajas ALTER COLUMN Diferencia DECIMAL(18,2) NOT NULL;
        PRINT '✓ Cajas.Diferencia cambiado a NOT NULL';
    END

    -- Nuevas columnas de Cajas
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Cajas') AND name = 'Abierta')
    BEGIN
        ALTER TABLE Cajas ADD Abierta BIT NOT NULL DEFAULT 0;
        PRINT '✓ Cajas.Abierta agregado';
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Cajas') AND name = 'TotalNequi')
    BEGIN
        ALTER TABLE Cajas ADD TotalNequi DECIMAL(18,2) NOT NULL DEFAULT 0;
        PRINT '✓ Cajas.TotalNequi agregado';
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Cajas') AND name = 'TotalDaviplata')
    BEGIN
        ALTER TABLE Cajas ADD TotalDaviplata DECIMAL(18,2) NOT NULL DEFAULT 0;
        PRINT '✓ Cajas.TotalDaviplata agregado';
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Cajas') AND name = 'TotalQR')
    BEGIN
        ALTER TABLE Cajas ADD TotalQR DECIMAL(18,2) NOT NULL DEFAULT 0;
        PRINT '✓ Cajas.TotalQR agregado';
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Cajas') AND name = 'EfectivoContado')
    BEGIN
        ALTER TABLE Cajas ADD EfectivoContado DECIMAL(18,2) NOT NULL DEFAULT 0;
        PRINT '✓ Cajas.EfectivoContado agregado';
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Cajas') AND name = 'EfectivoEsperado')
    BEGIN
        ALTER TABLE Cajas ADD EfectivoEsperado DECIMAL(18,2) NOT NULL DEFAULT 0;
        PRINT '✓ Cajas.EfectivoEsperado agregado';
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Cajas') AND name = 'DiferenciaEfectivo')
    BEGIN
        ALTER TABLE Cajas ADD DiferenciaEfectivo DECIMAL(18,2) NOT NULL DEFAULT 0;
        PRINT '✓ Cajas.DiferenciaEfectivo agregado';
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Cajas') AND name = 'TotalRetiros')
    BEGIN
        ALTER TABLE Cajas ADD TotalRetiros DECIMAL(18,2) NOT NULL DEFAULT 0;
        PRINT '✓ Cajas.TotalRetiros agregado';
    END

    -- ============================================================
    -- 7. TABLA DetallesDenominaciones (nueva)
    -- ============================================================

    IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DetallesDenominaciones')
    BEGIN
        CREATE TABLE DetallesDenominaciones (
            Id       INT IDENTITY(1,1) PRIMARY KEY,
            CajaId   INT NOT NULL,
            Tipo     INT NOT NULL,
            Valor    INT NOT NULL,
            Cantidad INT NOT NULL,
            Total    DECIMAL(18,2) NOT NULL,
            TipoArqueo INT NOT NULL,
            Fecha    DATETIME2 NOT NULL,
            CONSTRAINT FK_DetallesDenominaciones_Cajas_CajaId
                FOREIGN KEY (CajaId) REFERENCES Cajas(Id) ON DELETE NO ACTION
        );
        CREATE INDEX IX_DetallesDenominaciones_CajaId ON DetallesDenominaciones(CajaId);
        PRINT '✓ Tabla DetallesDenominaciones creada';
    END

    -- ============================================================
    -- 8. TABLA RetirosCaja (nueva)
    -- ============================================================

    IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RetirosCaja')
    BEGIN
        CREATE TABLE RetirosCaja (
            Id           INT IDENTITY(1,1) PRIMARY KEY,
            CajaId       INT NOT NULL,
            Monto        DECIMAL(18,2) NOT NULL,
            Motivo       NVARCHAR(200) NOT NULL,
            Observaciones NVARCHAR(500) NULL,
            UsuarioId    INT NOT NULL,
            Fecha        DATETIME2 NOT NULL,
            CONSTRAINT FK_RetirosCaja_Cajas_CajaId
                FOREIGN KEY (CajaId) REFERENCES Cajas(Id) ON DELETE NO ACTION,
            CONSTRAINT FK_RetirosCaja_Usuarios_UsuarioId
                FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id) ON DELETE NO ACTION
        );
        CREATE INDEX IX_RetirosCaja_CajaId  ON RetirosCaja(CajaId);
        CREATE INDEX IX_RetirosCaja_UsuarioId ON RetirosCaja(UsuarioId);
        PRINT '✓ Tabla RetirosCaja creada';
    END

    -- ============================================================
    -- 9. TABLA Clientes — reestructuración
    -- ============================================================

    -- Eliminar índice antiguo
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('Clientes') AND name = 'IX_Clientes_Documento')
    BEGIN
        DROP INDEX IX_Clientes_Documento ON Clientes;
        PRINT '✓ Índice IX_Clientes_Documento eliminado';
    END

    -- Renombrar columnas (solo si tienen el nombre viejo)
    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Clientes') AND name = 'Nombre')
    BEGIN
        EXEC sp_rename 'Clientes.Nombre', 'NombreCompleto', 'COLUMN';
        PRINT '✓ Clientes.Nombre renombrado a NombreCompleto';
    END

    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Clientes') AND name = 'Estado')
        AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Clientes') AND name = 'TipoDocumento')
    BEGIN
        EXEC sp_rename 'Clientes.Estado', 'TipoDocumento', 'COLUMN';
        PRINT '✓ Clientes.Estado renombrado a TipoDocumento';
    END

    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Clientes') AND name = 'Documento')
        AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Clientes') AND name = 'TelefonoSecundario')
    BEGIN
        EXEC sp_rename 'Clientes.Documento', 'TelefonoSecundario', 'COLUMN';
        PRINT '✓ Clientes.Documento renombrado a TelefonoSecundario';
    END

    -- Eliminar columnas obsoletas
    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Clientes') AND name = 'LimiteCredito')
    BEGIN
        ALTER TABLE Clientes DROP COLUMN LimiteCredito;
        PRINT '✓ Clientes.LimiteCredito eliminado';
    END

    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Clientes') AND name = 'SaldoPendiente')
    BEGIN
        ALTER TABLE Clientes DROP COLUMN SaldoPendiente;
        PRINT '✓ Clientes.SaldoPendiente eliminado';
    END

    -- Agregar nuevas columnas
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Clientes') AND name = 'NumeroDocumento')
    BEGIN
        ALTER TABLE Clientes ADD NumeroDocumento NVARCHAR(50) NOT NULL DEFAULT '';
        PRINT '✓ Clientes.NumeroDocumento agregado';
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Clientes') AND name = 'TipoCliente')
    BEGIN
        ALTER TABLE Clientes ADD TipoCliente INT NOT NULL DEFAULT 0;
        PRINT '✓ Clientes.TipoCliente agregado';
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Clientes') AND name = 'RazonSocial')
    BEGIN
        ALTER TABLE Clientes ADD RazonSocial NVARCHAR(200) NULL;
        PRINT '✓ Clientes.RazonSocial agregado';
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Clientes') AND name = 'NombreComercial')
    BEGIN
        ALTER TABLE Clientes ADD NombreComercial NVARCHAR(200) NULL;
        PRINT '✓ Clientes.NombreComercial agregado';
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Clientes') AND name = 'Regimen')
    BEGIN
        ALTER TABLE Clientes ADD Regimen INT NOT NULL DEFAULT 0;
        PRINT '✓ Clientes.Regimen agregado';
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Clientes') AND name = 'ResponsabilidadFiscal')
    BEGIN
        ALTER TABLE Clientes ADD ResponsabilidadFiscal INT NOT NULL DEFAULT 0;
        PRINT '✓ Clientes.ResponsabilidadFiscal agregado';
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Clientes') AND name = 'ActividadEconomica')
    BEGIN
        ALTER TABLE Clientes ADD ActividadEconomica NVARCHAR(10) NULL;
        PRINT '✓ Clientes.ActividadEconomica agregado';
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Clientes') AND name = 'Pais')
    BEGIN
        ALTER TABLE Clientes ADD Pais NVARCHAR(100) NOT NULL DEFAULT '';
        PRINT '✓ Clientes.Pais agregado';
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Clientes') AND name = 'Departamento')
    BEGIN
        ALTER TABLE Clientes ADD Departamento NVARCHAR(100) NOT NULL DEFAULT '';
        PRINT '✓ Clientes.Departamento agregado';
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Clientes') AND name = 'Ciudad')
    BEGIN
        ALTER TABLE Clientes ADD Ciudad NVARCHAR(100) NOT NULL DEFAULT '';
        PRINT '✓ Clientes.Ciudad agregado';
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Clientes') AND name = 'EmailSecundario')
    BEGIN
        ALTER TABLE Clientes ADD EmailSecundario NVARCHAR(200) NULL;
        PRINT '✓ Clientes.EmailSecundario agregado';
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Clientes') AND name = 'Activo')
    BEGIN
        ALTER TABLE Clientes ADD Activo BIT NOT NULL DEFAULT 0;
        PRINT '✓ Clientes.Activo agregado';
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Clientes') AND name = 'Observaciones')
    BEGIN
        ALTER TABLE Clientes ADD Observaciones NVARCHAR(500) NULL;
        PRINT '✓ Clientes.Observaciones agregado';
    END

    -- Índice único por NumeroDocumento
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('Clientes') AND name = 'IX_Clientes_NumeroDocumento')
    BEGIN
        CREATE UNIQUE INDEX IX_Clientes_NumeroDocumento ON Clientes(NumeroDocumento)
            WHERE NumeroDocumento <> '';   -- filtrado para evitar conflicto con filas existentes vacías
        PRINT '✓ Índice único IX_Clientes_NumeroDocumento creado';
    END

    -- ============================================================
    -- 10. TABLA Usuarios / TABLA Roles
    --     El sistema migró de tabla Roles separada con FK
    --     a columna Rol directa en Usuarios (int enum).
    -- ============================================================

    -- Ampliar PasswordHash a 256
    IF EXISTS (
        SELECT 1 FROM sys.columns c
        JOIN sys.types t ON c.user_type_id = t.user_type_id
        WHERE c.object_id = OBJECT_ID('Usuarios')
          AND c.name = 'PasswordHash'
          AND t.name = 'nvarchar'
          AND c.max_length = 510   -- 255 × 2
    )
    BEGIN
        ALTER TABLE Usuarios ALTER COLUMN PasswordHash NVARCHAR(256) NOT NULL;
        PRINT '✓ Usuarios.PasswordHash ampliado a nvarchar(256)';
    END

    -- Renombrar RolId → Rol (solo si existe RolId y no existe Rol)
    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Usuarios') AND name = 'RolId')
       AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Usuarios') AND name = 'Rol')
    BEGIN
        -- Primero eliminar FK y su índice para poder renombrar
        IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Usuarios_Roles_RolId')
            ALTER TABLE Usuarios DROP CONSTRAINT FK_Usuarios_Roles_RolId;
        IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('Usuarios') AND name = 'IX_Usuarios_RolId')
            DROP INDEX IX_Usuarios_RolId ON Usuarios;

        EXEC sp_rename 'Usuarios.RolId', 'Rol', 'COLUMN';
        PRINT '✓ Usuarios.RolId renombrado a Rol';
    END

    -- Eliminar tabla Roles si ya no se usa (FK fue eliminada arriba)
    IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Roles')
       AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE referenced_object_id = OBJECT_ID('Roles'))
    BEGIN
        DROP TABLE Roles;
        PRINT '✓ Tabla Roles eliminada (ahora Usuarios.Rol es enum directo)';
    END

    -- ============================================================
    -- Registrar versión
    -- ============================================================

    INSERT INTO __DatabaseVersion (Version, Description)
    VALUES ('1.0.1', 'Actualización acumulativa: IVA, métodos de pago (Nequi/Daviplata/QR), '
                   + 'arqueo de caja, retiros, clientes ampliados, venta por peso, '
                   + 'proveedor en producto, anulación de ventas.');

    COMMIT TRANSACTION;

    PRINT '========================================';
    PRINT '✓ Actualización a v1.0.1 completada';
    PRINT '========================================';

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT '========================================';
    PRINT '❌ ERROR durante la actualización:';
    PRINT ERROR_MESSAGE();
    PRINT '========================================';
    THROW;
END CATCH
GO
