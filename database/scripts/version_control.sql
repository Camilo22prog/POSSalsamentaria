-- version_control.sql
-- Funciones de control de versiones de base de datos

USE POSSalsamentaria;
GO

-- Función para obtener versión actual
IF OBJECT_ID('dbo.GetCurrentVersion', 'FN') IS NOT NULL
    DROP FUNCTION dbo.GetCurrentVersion;
GO

CREATE FUNCTION dbo.GetCurrentVersion()
RETURNS NVARCHAR(20)
AS
BEGIN
    DECLARE @Version NVARCHAR(20);
    
    SELECT TOP 1 @Version = Version
    FROM __DatabaseVersion
    ORDER BY AppliedDate DESC;
    
    RETURN ISNULL(@Version, '0.0.0');
END
GO

-- Procedimiento para registrar versión
IF OBJECT_ID('dbo.RegisterVersion', 'P') IS NOT NULL
    DROP PROCEDURE dbo.RegisterVersion;
GO

CREATE PROCEDURE dbo.RegisterVersion
    @Version NVARCHAR(20),
    @Description NVARCHAR(500)
AS
BEGIN
    INSERT INTO __DatabaseVersion (Version, Description, AppliedDate)
    VALUES (@Version, @Description, GETDATE());
    
    PRINT 'Versión ' + @Version + ' registrada';
END
GO

-- Función para verificar si versión está aplicada
IF OBJECT_ID('dbo.IsVersionApplied', 'FN') IS NOT NULL
    DROP FUNCTION dbo.IsVersionApplied;
GO

CREATE FUNCTION dbo.IsVersionApplied(@Version NVARCHAR(20))
RETURNS BIT
AS
BEGIN
    DECLARE @Result BIT = 0;
    
    IF EXISTS (SELECT 1 FROM __DatabaseVersion WHERE Version = @Version)
        SET @Result = 1;
    
    RETURN @Result;
END
GO

PRINT 'Funciones de control de versiones creadas exitosamente';
GO