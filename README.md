# POS Salsamentaria

Sistema de punto de venta (POS) de escritorio para salsamentarías, desarrollado en WPF/.NET 8 con arquitectura por capas.

## 📋 Tabla de contenido

- [Stack tecnológico](#-stack-tecnológico)
- [Arquitectura del proyecto](#-arquitectura-del-proyecto)
- [Requisitos previos](#-requisitos-previos)
- [Instalación y configuración](#-instalación-y-configuración)
- [Ejecutar el proyecto](#-ejecutar-el-proyecto)
- [Estructura de la base de datos](#-estructura-de-la-base-de-datos)
- [Flujo de trabajo con Git](#-flujo-de-trabajo-con-git)
- [Roles y permisos](#-roles-y-permisos)
- [Compilar instalador](#-compilar-instalador-opcional)

---

## 🛠️ Stack tecnológico

**Frontend / Aplicación:**
- WPF (.NET 8) con patrón MVVM
- CommunityToolkit.Mvvm (ObservableObject, RelayCommand)
- LiveCharts2 (SkiaSharpView.WPF) — gráficas de dashboards

**Backend / Datos:**
- Entity Framework Core (ORM + migraciones)
- SQL Server Express

**Despliegue:**
- Inno Setup (instalador .exe)
- `dotnet publish` (self-contained, single-file)
- POS.Updater (proyecto console para actualizaciones automáticas)

---

## 🏗️ Arquitectura del proyecto

El proyecto sigue una arquitectura por capas (Clean Architecture simplificada):

```
POSSalsamentaria/
├── POS.Domain/            # Entidades y enums (Usuario, TipoRol, Producto, etc.)
├── POS.Application/       # DTOs, interfaces y servicios (lógica de negocio)
├── POS.Infrastructure/    # Repositorios, ApplicationDbContext, configuraciones EF
├── POS.UI/                # WPF: Views, ViewModels, Services
├── POS.Updater/           # Proyecto console para aplicar actualizaciones
├── database/
│   └── scripts/           # Scripts SQL de instalación y actualización
├── installer_resources/   # Recursos para el instalador (icono, scripts)
└── installer.iss          # Script de Inno Setup
```

**Regla de dependencias:** `UI → Application → Domain` y `Infrastructure → Application → Domain`. La UI nunca accede directamente a Infrastructure; todo pasa por interfaces definidas en Application.

---

## ✅ Requisitos previos

Antes de clonar el proyecto, instala:

1. **Visual Studio 2022** (o superior) con la carga de trabajo ".NET desktop development"
   - Descarga: https://visualstudio.microsoft.com/
2. **.NET 8 SDK**
   - Descarga: https://dotnet.microsoft.com/download/dotnet/8.0
3. **SQL Server Express** (o SQL Server Developer si ya tienes uno)
   - Descarga: https://go.microsoft.com/fwlink/?linkid=2216019
4. **SQL Server Management Studio (SSMS)**
   - Descarga: https://aka.ms/ssmsfullsetup
5. **Git**
   - Descarga: https://git-scm.com/downloads

---

## ⚙️ Instalación y configuración

### 1. Clonar el repositorio

```powershell
git clone https://github.com/TU-USUARIO/POSSalsamentaria.git
cd POSSalsamentaria
```

### 2. Restaurar paquetes NuGet

```powershell
dotnet restore
```

O simplemente abre `POSSalsamentaria.sln` en Visual Studio, que restaura automáticamente.

### 3. Configurar el connection string

Abre `POS.UI/appsettings.json` y verifica que apunte a tu instancia local de SQL Server:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=POSSalsamentaria;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

> ⚠️ Si tu instancia de SQL Server tiene otro nombre (ej. `TU-PC\SQLEXPRESS`), ajústalo aquí.

### 4. Crear y migrar la base de datos

Desde PowerShell, en la raíz del proyecto:

```powershell
cd POS.Infrastructure
dotnet ef database update --startup-project ../POS.UI
cd ..
```

Esto crea la base de datos `POSSalsamentaria` con todas las tablas mediante las migraciones de Entity Framework.

> 💡 Si no tienes la herramienta `dotnet-ef` instalada:
> ```powershell
> dotnet tool install --global dotnet-ef
> ```

### 5. Crear el usuario administrador inicial

Abre SSMS, conecta a `localhost\SQLEXPRESS` y ejecuta:

```sql
USE POSSalsamentaria;
GO

INSERT INTO Usuarios (
    NombreUsuario, NombreCompleto, Email, PasswordHash,
    Rol, Estado, Bloqueado, IntentosFailidos, FechaCreacion
)
VALUES (
    'admin', 'Administrador', 'admin@possalsamentaria.com',
    'jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=', -- hash de "admin"
    1, 1, 0, 0, GETDATE()
);
GO
```

---

## ▶️ Ejecutar el proyecto

### Desde Visual Studio
1. Abre `POSSalsamentaria.sln`
2. Click derecho en `POS.UI` → **"Establecer como proyecto de inicio"**
3. Presiona `F5`

### Desde línea de comandos

```powershell
dotnet run --project POS.UI
```

**Login inicial:**
- Usuario: `admin`
- Contraseña: `admin`

> ⚠️ Cambiar la contraseña inmediatamente después del primer inicio de sesión.

---

## 🗄️ Estructura de la base de datos

Tablas principales:

| Tabla | Descripción |
|---|---|
| `Usuarios` | Usuarios del sistema, con `Rol` (1=Admin, 2=Supervisor, 3=Cajero) |
| `Productos` | Catálogo de productos, incluye soporte para venta por peso |
| `Categorias` | Categorías de productos |
| `Clientes` | Clientes registrados |
| `Cajas` | Aperturas/cierres de caja |
| `Ventas` / `DetallesVenta` / `PagosVenta` | Transacciones de venta |
| `MovimientosCaja` | Ingresos/egresos de caja |
| `MovimientosInventario` | Historial de entradas/salidas de stock |
| `ArqueosCaja` | Conteo de denominaciones al cerrar caja |
| `__DatabaseVersion` | Control de versiones aplicadas a la BD |

Si haces cambios de esquema, **usa migraciones de EF Core**, no scripts SQL manuales:

```powershell
cd POS.Infrastructure
dotnet ef migrations add NombreDeLaMigracion --startup-project ../POS.UI
dotnet ef database update --startup-project ../POS.UI
```

---

## 🌿 Flujo de trabajo con Git

Para evitar conflictos trabajando en equipo:

1. **Nunca trabajen directamente sobre `main`**. Crea una rama por funcionalidad:
   ```powershell
   git checkout -b feature/nombre-de-la-funcionalidad
   ```

2. Haz commits pequeños y descriptivos:
   ```powershell
   git add .
   git commit -m "feat: agrega módulo de proveedores"
   ```

3. Sube tu rama y crea un Pull Request:
   ```powershell
   git push origin feature/nombre-de-la-funcionalidad
   ```

4. Revisión entre el equipo antes de mergear a `main`.

5. Antes de empezar a trabajar cada día, actualiza tu rama:
   ```powershell
   git checkout main
   git pull origin main
   git checkout tu-rama
   git merge main
   ```

**Convención de commits sugerida:**
- `feat:` nueva funcionalidad
- `fix:` corrección de bug
- `refactor:` cambios internos sin alterar comportamiento
- `docs:` documentación
- `db:` cambios de base de datos/migraciones

---

## 👥 Roles y permisos

| Rol | Permisos |
|---|---|
| **Administrador** | Acceso total: usuarios, productos, inventario, reportes, anular ventas |
| **Supervisor** | Anular ventas, ver reportes, cerrar caja (sin gestión de usuarios/productos) |
| **Cajero** | Solo ventas y clientes |

---

## 📦 Compilar instalador (opcional)

Solo necesario para generar el `.exe` de distribución final (no para desarrollo diario).

Requiere [Inno Setup 6](https://jrsoftware.org/isdl.php) instalado.

```powershell
dotnet publish .\POS.UI\POS.UI.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o .\publish
dotnet publish .\POS.Updater\POS.Updater.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o .\publish
& "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" .\installer.iss
```

El instalador queda en `.\installer_output\`.

---

## 🐛 Problemas comunes

| Problema | Solución |
|---|---|
| `A second operation was started on this context instance...` | Concurrencia en DbContext; usar `SemaphoreSlim` en el ViewModel |
| `Invalid object name 'Usuarios'` | Faltan migraciones; correr `dotnet ef database update` |
| Login falla con admin/admin | Verificar que el `PasswordHash` en BD coincida con el hash SHA256 esperado |
| `sqlcmd no reconocido` | SQL Server no instalado o no está en el PATH |

---

## 📞 Contacto

Dudas sobre el proyecto: contactar a Camilix (mantenedor principal).
