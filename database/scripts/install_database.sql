-- install_database.sql
-- Script de instalación inicial de la base de datos

USE master;
GO

-- Crear base de datos si no existe
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'POSSalsamentaria')
BEGIN
    CREATE DATABASE POSSalsamentaria;
    PRINT 'Base de datos POSSalsamentaria creada exitosamente';
END
ELSE
BEGIN
    PRINT 'La base de datos POSSalsamentaria ya existe';
END
GO

USE POSSalsamentaria;
GO

-- Crear tabla de control de versiones si no existe
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = '__DatabaseVersion')
BEGIN
    CREATE TABLE __DatabaseVersion (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Version NVARCHAR(20) NOT NULL,
        Description NVARCHAR(500),
        AppliedDate DATETIME NOT NULL DEFAULT GETDATE()
    );
    
    INSERT INTO __DatabaseVersion (Version, Description)
    VALUES ('1.0.0', 'Instalación inicial');
    
    PRINT 'Tabla de control de versiones creada';
END
GO

-- Crear usuario administrador por defecto
USE POSSalsamentaria;
GO

IF NOT EXISTS (SELECT * FROM Usuarios WHERE NombreUsuario = 'admin')
BEGIN
    INSERT INTO Usuarios (NombreUsuario, NombreCompleto, Email, PasswordHash, Rol, Estado, Bloqueado, FechaCreacion)
    VALUES (
        'admin',
        'Administrador',
        'admin@possalsamentaria.com',
        'jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=', -- Password: admin
        1, -- Administrador
        1, -- Activo
        0, -- No bloqueado
        GETDATE()
    );
    
    PRINT 'Usuario administrador creado (usuario: admin, password: admin)';
    PRINT '*** IMPORTANTE: Cambie la contraseña del administrador después del primer inicio ***';
END
GO

PRINT 'Instalación de base de datos completada exitosamente';
PRINT 'Version: 1.0.0';
GO