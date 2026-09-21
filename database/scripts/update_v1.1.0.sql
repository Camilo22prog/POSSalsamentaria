-- =============================================================
-- update_v1.1.0.sql
-- Actualización acumulativa desde v1.0.1 → v1.1.0
-- Cubre todas las migraciones EF Core aplicadas desde v1.0.1.
-- Es IDEMPOTENTE.
-- =============================================================

USE POSSalsamentaria;
GO

-- Verificar si ya fue aplicada
IF EXISTS (SELECT 1 FROM __DatabaseVersion WHERE Version = '1.1.0')
BEGIN
    PRINT 'La versión 1.1.0 ya fue aplicada. No se requieren cambios.';
    RETURN;
END
GO

BEGIN TRANSACTION;

BEGIN TRY

    PRINT '========================================';
    PRINT 'Iniciando actualización a v1.1.0';
    PRINT '========================================';

    -- ============================================================
    -- 1. TABLA GastosOperativos
    -- ============================================================
    IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'GastosOperativos')
    BEGIN
        CREATE TABLE GastosOperativos (
            Id INT IDENTITY(1,1) PRIMARY KEY,
            Tipo INT NOT NULL,
            Descripcion NVARCHAR(MAX) NOT NULL,
            Monto DECIMAL(18,2) NOT NULL,
            Fecha DATETIME2 NOT NULL,
            Comprobante NVARCHAR(MAX) NULL,
            Observaciones NVARCHAR(MAX) NULL,
            UsuarioId INT NOT NULL,
            FechaCreacion DATETIME2 NOT NULL,
            CONSTRAINT FK_GastosOperativos_Usuarios_UsuarioId
                FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id) ON DELETE CASCADE
        );
        CREATE INDEX IX_GastosOperativos_UsuarioId ON GastosOperativos(UsuarioId);
        PRINT '✓ Tabla GastosOperativos creada';
    END

    -- ============================================================
    -- 2. TABLA Configuraciones
    -- ============================================================
    IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Configuraciones')
    BEGIN
        CREATE TABLE Configuraciones (
            Id INT PRIMARY KEY,
            NombreNegocio NVARCHAR(MAX) NOT NULL,
            Nit NVARCHAR(MAX) NULL,
            Direccion NVARCHAR(MAX) NULL,
            Telefono NVARCHAR(MAX) NULL,
            MensajePieTicket NVARCHAR(MAX) NOT NULL,
            NombreImpresora NVARCHAR(MAX) NULL,
            AbrirCajonAutomatico BIT NOT NULL,
            PuertoBalanza NVARCHAR(MAX) NULL,
            BaudRateBalanza INT NOT NULL,
            UrlActualizaciones NVARCHAR(MAX) NULL,
            FechaModificacion DATETIME2 NOT NULL
        );
        PRINT '✓ Tabla Configuraciones creada';
    END

    -- Insertar fila de configuración por defecto si no existe
    IF NOT EXISTS (SELECT 1 FROM Configuraciones WHERE Id = 1)
    BEGIN
        INSERT INTO Configuraciones (Id, NombreNegocio, MensajePieTicket, AbrirCajonAutomatico, BaudRateBalanza, FechaModificacion)
        VALUES (1, 'Mi Negocio', '¡Gracias por su compra!', 1, 9600, GETDATE());
        PRINT '✓ Fila de configuración inicial insertada';
    END

    -- ============================================================
    -- Registrar versión 1.1.0
    -- ============================================================
    INSERT INTO __DatabaseVersion (Version, Description, AppliedDate)
    VALUES ('1.1.0', 'Actualización v1.1.0: Módulo de Gastos Operativos y Configuración de Sistema.', GETDATE());

    COMMIT TRANSACTION;

    PRINT '========================================';
    PRINT '✓ Actualización a v1.1.0 completada';
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
