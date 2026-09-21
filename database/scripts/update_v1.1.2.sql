-- =============================================================
-- update_v1.1.2.sql
-- Actualización para la versión 1.1.2
-- Añade los campos financieros y de liquidación a la tabla DetallesCompra
-- =============================================================

USE POSSalsamentaria;
GO

BEGIN TRANSACTION;

BEGIN TRY

    PRINT '========================================';
    PRINT 'Iniciando actualización de base de datos a v1.1.2';
    PRINT '========================================';

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('DetallesCompra') AND name = 'PrecioSinIva')
    BEGIN
        ALTER TABLE DetallesCompra ADD PrecioSinIva DECIMAL(18,2) NOT NULL DEFAULT 0;
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('DetallesCompra') AND name = 'PrecioConIva')
    BEGIN
        ALTER TABLE DetallesCompra ADD PrecioConIva DECIMAL(18,2) NOT NULL DEFAULT 0;
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('DetallesCompra') AND name = 'IvaPorcentaje')
    BEGIN
        ALTER TABLE DetallesCompra ADD IvaPorcentaje DECIMAL(18,2) NOT NULL DEFAULT 0;
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('DetallesCompra') AND name = 'IvaValor')
    BEGIN
        ALTER TABLE DetallesCompra ADD IvaValor DECIMAL(18,2) NOT NULL DEFAULT 0;
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('DetallesCompra') AND name = 'DescuentoPorcentaje')
    BEGIN
        ALTER TABLE DetallesCompra ADD DescuentoPorcentaje DECIMAL(18,2) NOT NULL DEFAULT 0;
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('DetallesCompra') AND name = 'DescuentoValor')
    BEGIN
        ALTER TABLE DetallesCompra ADD DescuentoValor DECIMAL(18,2) NOT NULL DEFAULT 0;
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('DetallesCompra') AND name = 'IbuaPorcentaje')
    BEGIN
        ALTER TABLE DetallesCompra ADD IbuaPorcentaje DECIMAL(18,2) NOT NULL DEFAULT 0;
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('DetallesCompra') AND name = 'IbuaValor')
    BEGIN
        ALTER TABLE DetallesCompra ADD IbuaValor DECIMAL(18,2) NOT NULL DEFAULT 0;
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('DetallesCompra') AND name = 'IcuiPorcentaje')
    BEGIN
        ALTER TABLE DetallesCompra ADD IcuiPorcentaje DECIMAL(18,2) NOT NULL DEFAULT 0;
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('DetallesCompra') AND name = 'IcuiValor')
    BEGIN
        ALTER TABLE DetallesCompra ADD IcuiValor DECIMAL(18,2) NOT NULL DEFAULT 0;
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('DetallesCompra') AND name = 'CostoUnitarioLiquidado')
    BEGIN
        ALTER TABLE DetallesCompra ADD CostoUnitarioLiquidado DECIMAL(18,2) NOT NULL DEFAULT 0;
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('DetallesCompra') AND name = 'PorcentajeGanancia')
    BEGIN
        ALTER TABLE DetallesCompra ADD PorcentajeGanancia DECIMAL(18,2) NOT NULL DEFAULT 0;
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('DetallesCompra') AND name = 'PrecioVentaCalculado')
    BEGIN
        ALTER TABLE DetallesCompra ADD PrecioVentaCalculado DECIMAL(18,2) NOT NULL DEFAULT 0;
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('DetallesCompra') AND name = 'LoteCodigo')
    BEGIN
        ALTER TABLE DetallesCompra ADD LoteCodigo NVARCHAR(MAX) NULL;
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('DetallesCompra') AND name = 'ActualizarCatalogo')
    BEGIN
        ALTER TABLE DetallesCompra ADD ActualizarCatalogo BIT NOT NULL DEFAULT 0;
    END

    IF NOT EXISTS (SELECT 1 FROM __DatabaseVersion WHERE Version = '1.1.2')
    BEGIN
        INSERT INTO __DatabaseVersion (Version, Description, AppliedDate)
        VALUES ('1.1.2', 'Actualizacion v1.1.2: Calculadora financiera de compras.', GETDATE());
    END

    COMMIT TRANSACTION;
    PRINT '========================================';
    PRINT '✓ Actualización a v1.1.2 completada con éxito';
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
