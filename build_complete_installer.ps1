# =============================================
# Script completo para crear instalador
# POS Salsamentaria v1.1.0
# =============================================

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  CONSTRUCCION COMPLETA DEL INSTALADOR" -ForegroundColor Cyan
Write-Host "  POS SALSAMENTARIA v1.1.0" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# PASO 1: Limpiar
Write-Host "[1/6] Limpiando proyecto..." -ForegroundColor Yellow
dotnet clean | Out-Null
Remove-Item -Path ".\publish" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path ".\installer_output" -Recurse -Force -ErrorAction SilentlyContinue
Write-Host "OK - Limpieza completada" -ForegroundColor Green
Write-Host ""

# PASO 2: Compilar
Write-Host "[2/6] Compilando proyecto..." -ForegroundColor Yellow
dotnet build --configuration Release | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR - Error en compilacion" -ForegroundColor Red
    exit 1
}
Write-Host "OK - Compilacion exitosa" -ForegroundColor Green
Write-Host ""

# PASO 3: Crear backup de BD
Write-Host "[3/6] Creando backup de base de datos..." -ForegroundColor Yellow
New-Item -ItemType Directory -Force -Path ".\database\backups" | Out-Null

$backupQuery = "USE master; BACKUP DATABASE POSSalsamentaria TO DISK = 'C:\Temp\POSSalsamentaria_v1.1.0_Completo.bak' WITH FORMAT, INIT;"

try {
    sqlcmd -S "localhost\SQLEXPRESS" -E -Q $backupQuery 2>&1 | Out-Null
    Copy-Item -Path "C:\Temp\POSSalsamentaria_v1.1.0_Completo.bak" -Destination ".\database\backups\" -Force
    Write-Host "OK - Backup creado" -ForegroundColor Green
} catch {
    Write-Host "AVISO - No se pudo crear backup automatico (hazlo manualmente en SSMS)" -ForegroundColor Yellow
}
Write-Host ""

# PASO 4: Publicar aplicacion
Write-Host "[4/6] Publicando aplicacion..." -ForegroundColor Yellow

dotnet publish .\POS.UI\POS.UI.csproj `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=false `
    -p:PublishTrimmed=false `
    -o .\publish | Out-Null

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR - Error al publicar aplicacion" -ForegroundColor Red
    exit 1
}

dotnet publish .\POS.Updater\POS.Updater.csproj `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=false `
    -o .\publish | Out-Null

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR - Error al publicar updater" -ForegroundColor Red
    exit 1
}

Write-Host "OK - Publicacion completada" -ForegroundColor Green
Write-Host ""

# PASO 5: Verificar archivos
Write-Host "[5/6] Verificando archivos..." -ForegroundColor Yellow
$archivos = @(".\publish\POS.UI.exe", ".\publish\POS.Updater.exe")
$ok = $true
foreach ($archivo in $archivos) {
    if (-not (Test-Path $archivo)) {
        Write-Host "ERROR - Falta: $archivo" -ForegroundColor Red
        $ok = $false
    }
}
if (-not $ok) { exit 1 }
Write-Host "OK - Archivos verificados" -ForegroundColor Green
Write-Host ""

# PASO 6: Compilar instalador
Write-Host "[6/6] Compilando instalador con Inno Setup..." -ForegroundColor Yellow
$innoSetup = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"

if (-not (Test-Path $innoSetup)) {
    Write-Host "ERROR - Inno Setup no encontrado" -ForegroundColor Red
    Write-Host "Instala desde: https://jrsoftware.org/isdl.php" -ForegroundColor Yellow
    exit 1
}

& $innoSetup ".\installer.iss" | Out-Null

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR - Error al compilar instalador" -ForegroundColor Red
    exit 1
}

Write-Host "OK - Instalador compilado" -ForegroundColor Green
Write-Host ""

# RESULTADO FINAL
$instalador = ".\installer_output\POS_Salsamentaria_Setup_v1.1.0.exe"
if (Test-Path $instalador) {
    $tamano = [math]::Round((Get-Item $instalador).Length / 1MB, 2)

    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "  INSTALADOR CREADO EXITOSAMENTE" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Archivo: POS_Salsamentaria_Setup_v1.1.0.exe" -ForegroundColor Cyan
    Write-Host "Ubicacion: .\installer_output\" -ForegroundColor Cyan
    Write-Host "Tamano: $tamano MB" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Pasos siguientes:" -ForegroundColor Yellow
    Write-Host "  1. Probar en VM o PC limpio" -ForegroundColor White
    Write-Host "  2. Copiar a USB" -ForegroundColor White
    Write-Host "  3. Agregar SQL Server Express al USB" -ForegroundColor White
    Write-Host "  4. Llevar a la salsamentaria" -ForegroundColor White
    Write-Host ""
} else {
    Write-Host "ERROR - No se genero el instalador" -ForegroundColor Red
    exit 1
}