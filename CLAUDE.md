# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

**Build the solution:**
```bash
dotnet build POSSalsamentaria.sln
```

**Run the application:**
```bash
dotnet run --project POS.UI/POS.UI.csproj
```

**Publish (self-contained single-file for Windows x64):**
```bash
dotnet publish POS.UI/POS.UI.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:PublishTrimmed=false -o ./publish
```
Or use `publish.bat` on Windows.

**EF Core migrations (run from solution root, targeting POS.Infrastructure):**
```bash
dotnet ef migrations add <NombreMigracion> --project POS.Infrastructure --startup-project POS.UI
dotnet ef database update --project POS.Infrastructure --startup-project POS.UI
```

## Architecture

Clean Architecture with four projects:

- **POS.Domain** — entities, enums, repository interfaces. No external dependencies. Entity groups: Security (`Usuario`, `Auditoria`), Catalog (`Producto`, `Categoria`), Inventory (`Lote`, `MovimientoInventario`, `Merma`), Sales (`Venta`, `DetalleVenta`, `Pago`, `Caja`, `MovimientoCaja`, `RetiroCaja`), Purchases (`Proveedor`, `Compra`, `DetalleCompra`).
- **POS.Application** — service interfaces and implementations, DTOs. Services receive `IUnitOfWork` via DI and use transactions for multi-step operations (`BeginTransactionAsync` / `CommitTransactionAsync`).
- **POS.Infrastructure** — EF Core `ApplicationDbContext` (SQL Server), repository implementations, `UnitOfWork`, and EF migrations. `DependencyInjection.cs` provides the extension method `AddInfrastructure`.
- **POS.UI** — WPF app (.NET 8, `net8.0-windows`). Uses MVVM with `CommunityToolkit.Mvvm` (`[ObservableProperty]`, `[RelayCommand]`). Material Design (MaterialDesignThemes 4.9). Charts via LiveChartsCore.

### Key patterns in POS.UI

- **DI container** is wired manually in `App.xaml.cs` using `Microsoft.Extensions.Hosting`. All ViewModels and Views are registered as `Transient`; `IBalanzaService` and `IUpdateService` are `Singleton`.
- **Navigation** is handled by `MainViewModel` which swaps a `UserControl` (`ContenidoActual`) to navigate between views. Each view/viewmodel pair is resolved from `App.Services`.
- **Session/permissions** are managed by the singleton `SessionService` (in `POS.UI/Services/`). It holds the logged-in `UsuarioDto` and exposes role-based permission checks used by `MainViewModel` to show/hide menu items.
- **Ticket printing** lives in `POS.UI/Helpers/TicketPrinter.cs` and `CierreCajaPrinter.cs`.

### Database

SQL Server Express, local connection string hardcoded in `App.xaml.cs`:
```
Server=localhost\SQLEXPRESS;Database=POSSalsamentaria;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true
```
Also present in `POS.UI/appsettings.json` (copied to output).

### Scale (balanza) integration

`BalanzaService` communicates with a serial-port scale in continuous stream mode. It is registered as a `Singleton` and auto-connects 3 seconds after startup.

### Update service

`UpdateService` checks a remote URL for a newer version. The URL placeholder (`https://tu-servidor.com/updates`) must be updated before deploying.
