@echo off
REM Verificar si SQL Server Express está instalado

sc query "MSSQL$SQLEXPRESS" >nul 2>&1
if %errorlevel% equ 0 (
    echo SQL Server Express detectado
    exit /b 0
) else (
    echo SQL Server Express NO detectado
    exit /b 1
)