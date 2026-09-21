@echo off
echo ========================================
echo   CONSTRUCCIÓN COMPLETA DEL INSTALADOR
echo   POS SALSAMENTARIA v1.0.0
echo ========================================
echo.

REM 1. LIMPIAR
echo [1/4] Limpiando archivos anteriores...
if exist ".\publish" rmdir /s /q ".\publish"
if exist ".\installer_output" rmdir /s /q ".\installer_output"
echo OK
echo.

REM 2. PUBLICAR APLICACIÓN
echo [2/4] Publicando aplicación...
dotnet publish .\POS.UI\POS.UI.csproj ^
    -c Release ^
    -r win-x64 ^
    --self-contained true ^
    -p:PublishSingleFile=false ^
    -p:PublishTrimmed=false ^
    -p:Version=1.0.0 ^
    -o .\publish

if errorlevel 1 (
    echo ERROR: Falló la publicación
    pause
    exit /b 1
)
echo OK
echo.

REM 3. VERIFICAR ARCHIVOS
echo [3/4] Verificando archivos necesarios...
if not exist ".\publish\POS.UI.exe" (
    echo ERROR: No se encontró POS.UI.exe
    pause
    exit /b 1
)
if not exist ".\database\scripts\install_database.sql" (
    echo ERROR: No se encontró install_database.sql
    pause
    exit /b 1
)
echo OK
echo.

REM 4. COMPILAR INSTALADOR CON INNO SETUP
echo [4/4] Compilando instalador...
"C:\Program Files (x86)\Inno Setup 6\ISCC.exe" installer.iss

if errorlevel 1 (
    echo ERROR: Falló la compilación del instalador
    echo Verifica que Inno Setup esté instalado en la ruta por defecto
    pause
    exit /b 1
)

echo.
echo ========================================
echo   INSTALADOR CREADO EXITOSAMENTE
echo ========================================
echo.
echo Ubicación: .\installer_output\POS_Salsamentaria_Setup_v1.0.0.exe
echo.
echo Pasos siguientes:
echo 1. Probar el instalador en una VM o PC limpio
echo 2. Llevar el instalador a la salsamentaria
echo 3. Ejecutar como Administrador
echo.
pause