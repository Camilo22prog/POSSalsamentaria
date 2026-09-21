@echo off
echo ========================================
echo   PUBLICANDO POS SALSAMENTARIA (ARCHIVOS SUELTOS)
echo ========================================
echo.

REM Limpiar publicaciones anteriores
if exist ".\publish" rmdir /s /q ".\publish"

REM Publicar aplicación
dotnet publish .\POS.UI\POS.UI.csproj ^
    -c Release ^
    -r win-x64 ^
    --self-contained true ^
    -p:PublishSingleFile=false ^
    -p:PublishTrimmed=false ^
    -o .\publish

REM Publicar Updater
dotnet publish .\POS.Updater\POS.Updater.csproj ^
    -c Release ^
    -r win-x64 ^
    --self-contained false ^
    -o .\publish

echo.
echo ========================================
echo   PUBLICACION COMPLETADA
echo ========================================
echo.
echo Archivos en: .\publish
pause