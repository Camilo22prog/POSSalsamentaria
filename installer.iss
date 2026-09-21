; Script de instalación para POS Salsamentaria
; Creado con Inno Setup

#define MyAppName "POS Salsamentaria"
#define MyAppVersion "1.1.0"
#define MyAppPublisher "Tu Nombre/Empresa"
#define MyAppURL "https://www.tuempresa.com"
#define MyAppExeName "POS.UI.exe"
#define MyAppAssocName MyAppName + " File"
#define MyAppAssocExt ".posdb"
#define MyAppAssocKey StringChange(MyAppAssocName, " ", "") + MyAppAssocExt

[Setup]
; INFORMACIÓN DE LA APLICACIÓN
AppId={{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}

; CONFIGURACIÓN DE INSTALACIÓN
DefaultDirName={autopf}\POSSalsamentaria
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
DisableProgramGroupPage=yes

; CONFIGURACIÓN DE SALIDA
OutputDir=.\installer_output
OutputBaseFilename=POS_Salsamentaria_Setup_v{#MyAppVersion}
SetupIconFile=installer_resources\icon.ico
;WizardImageFile=installer_resources\logo.bmp

; COMPRESIÓN
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern

; PRIVILEGIOS
PrivilegesRequired=admin
PrivilegesRequiredOverridesAllowed=dialog

; DESINSTALADOR
UninstallDisplayIcon={app}\{#MyAppExeName}
UninstallDisplayName={#MyAppName}

; ARQUITECTURA
ArchitecturesInstallIn64BitMode=x64

[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"

[Tasks]
Name: "desktopicon"; Description: "Crear un icono en el &escritorio"; GroupDescription: "Iconos adicionales:"
Name: "quicklaunchicon"; Description: "Crear un icono en &Inicio rápido"; GroupDescription: "Iconos adicionales:"; Flags: unchecked

[Files]
; Archivos de la aplicación
Source: ".\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; Scripts de base de datos
Source: ".\database\scripts\*"; DestDir: "{app}\database\scripts"; Flags: ignoreversion recursesubdirs
; Recursos del instalador
Source: ".\installer_resources\sql_server_check.bat"; DestDir: "{tmp}"; Flags: deleteafterinstall

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
; Verificar e instalar SQL Server Express si es necesario
Filename: "{tmp}\sql_server_check.bat"; StatusMsg: "Verificando SQL Server Express..."; Flags: waituntilterminated runhidden; Check: NeedsSQLServer
Filename: "https://go.microsoft.com/fwlink/?linkid=2216019"; Description: "Descargar SQL Server Express (necesario)"; Flags: shellexec nowait postinstall skipifsilent; Check: NeedsSQLServer

; Crear base de datos
Filename: "sqlcmd"; Parameters: "-S localhost\SQLEXPRESS -E -i ""{app}\database\scripts\install_database.sql"""; StatusMsg: "Creando base de datos..."; Flags: waituntilterminated runhidden; Check: not DatabaseExists

; Crear funciones de control de versiones
Filename: "sqlcmd"; Parameters: "-S localhost\SQLEXPRESS -E -d POSSalsamentaria -i ""{app}\database\scripts\version_control.sql"""; StatusMsg: "Configurando sistema de versiones..."; Flags: waituntilterminated runhidden; Check: not DatabaseExists

; Ejecutar aplicación después de instalar
Filename: "{app}\{#MyAppExeName}"; Description: "Ejecutar {#MyAppName}"; Flags: nowait postinstall skipifsilent

[Code]
function IsSQLServerInstalled: Boolean;
var
  ResultCode: Integer;
begin
  // Ejecutar script de verificación
  Exec(ExpandConstant('{tmp}\sql_server_check.bat'), '', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  Result := (ResultCode = 0);
end;

function NeedsSQLServer: Boolean;
begin
  Result := not IsSQLServerInstalled;
end;

function DatabaseExists: Boolean;
var
  ResultCode: Integer;
begin
  Result := False;
  
  // Verificar si existe la base de datos
  Exec('sqlcmd', '-S localhost\SQLEXPRESS -E -Q "SELECT name FROM sys.databases WHERE name = ''POSSalsamentaria''"', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  
  Result := (ResultCode = 0);
end;

function InitializeSetup(): Boolean;
begin
  Result := True;
  
  // Verificar SQL Server al inicio
  if not IsSQLServerInstalled then
  begin
    if MsgBox('SQL Server Express no está instalado.' + #13#10 + #13#10 +
              'Es necesario para que el sistema funcione.' + #13#10 + #13#10 +
              '¿Desea continuar? Se abrirá la página de descarga al finalizar.', 
              mbConfirmation, MB_YESNO) = IDNO then
    begin
      Result := False;
    end;
  end;
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    // Verificar que todo se instaló correctamente
    if not FileExists(ExpandConstant('{app}\{#MyAppExeName}')) then
    begin
      MsgBox('Error: No se pudo instalar la aplicación correctamente.', mbError, MB_OK);
    end
    else if IsSQLServerInstalled and not DatabaseExists then
    begin
      MsgBox('La aplicación se instaló correctamente.' + #13#10 + #13#10 +
             'La base de datos se creará automáticamente al ejecutar la aplicación por primera vez.', 
             mbInformation, MB_OK);
    end;
  end;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  DialogResult: Integer;
begin
  if CurUninstallStep = usUninstall then
  begin
    DialogResult := MsgBox(
      '¿Desea eliminar también la base de datos?' + #13#10 + #13#10 +
      'ADVERTENCIA: Se perderán todos los datos de ventas, productos e inventario.' + #13#10 + #13#10 +
      'Seleccione:' + #13#10 +
      '- SÍ para eliminar TODO (aplicación y base de datos)' + #13#10 +
      '- NO para conservar la base de datos', 
      mbConfirmation, 
      MB_YESNO);
      
    if DialogResult = IDYES then
    begin
      // Eliminar base de datos
      Exec('sqlcmd', '-S localhost\SQLEXPRESS -E -Q "DROP DATABASE IF EXISTS POSSalsamentaria"', '', SW_HIDE, ewWaitUntilTerminated, DialogResult);
      
      if DialogResult = 0 then
        MsgBox('Base de datos eliminada correctamente.', mbInformation, MB_OK)
      else
        MsgBox('No se pudo eliminar la base de datos. Es posible que no exista o esté en uso.', mbError, MB_OK);
    end
    else
    begin
      MsgBox('La base de datos se conservó. Puede seguir usando sus datos si reinstala la aplicación.', mbInformation, MB_OK);
    end;
  end;
end;