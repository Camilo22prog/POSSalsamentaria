using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Application.DTOs.Security;
using POS.UI.Services;
using POS.UI.Views;
using POS.UI.Views.Catalog;
using POS.UI.Views.Customers;
using POS.UI.Views.Inventory;
using POS.UI.Views.Sales;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using POS.UI.Views.Reports;
using POS.UI.Views.Dashboards;
using POS.UI.Views.Expenses;
using POS.UI.Views.Settings;
using POS.UI.Views.Security;
using POS.UI.Views.Purchases;
using POS.UI.Views.Inventory;
using POS.Application.Interfaces;
using System.Reflection;
using System.Windows.Threading;

namespace POS.UI.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private UsuarioDto? _usuarioActual;

        [ObservableProperty]
        private string _titulo = "POS Salsamentaria";

        [ObservableProperty]
        private UserControl? _contenidoActual;

        [ObservableProperty]
        private bool _puedeGestionarUsuarios;

        [ObservableProperty]
        private bool _puedeGestionarProductos;

        [ObservableProperty]
        private bool _puedeGestionarInventario;

        [ObservableProperty]
        private bool _puedeVerReportes;

        [ObservableProperty]
        private bool _puedeAnularVentas;

        [ObservableProperty]
        private string _versionActual = string.Empty;

        [ObservableProperty]
        private string _horaStatusBar = string.Empty;

        [ObservableProperty]
        private int _contadorAlertasStock;

        [ObservableProperty]
        private bool _hayAlertasStock;

        public MainViewModel()
        {
            UsuarioActual = SessionService.Instance.UsuarioActual;
            PuedeGestionarUsuarios = SessionService.Instance.PuedeGestionarUsuarios();
            PuedeGestionarProductos = SessionService.Instance.PuedeGestionarProductos();
            PuedeGestionarInventario = SessionService.Instance.PuedeGestionarInventario();
            PuedeVerReportes = SessionService.Instance.PuedeVerReportes();
            PuedeAnularVentas = SessionService.Instance.PuedeAnularVentas();

            VersionActual = Assembly.GetExecutingAssembly()
                .GetName()
                .Version?
                .ToString(3) ?? "1.0.0";

            HoraStatusBar = DateTime.Now.ToString("dd/MM/yyyy  HH:mm:ss");
            var reloj = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            reloj.Tick += (_, _) => HoraStatusBar = DateTime.Now.ToString("dd/MM/yyyy  HH:mm:ss");
            reloj.Start();

            // Verificar alertas de stock al arrancar y cada 5 minutos
            _ = ActualizarAlertasStockAsync();
            var timerAlertas = new DispatcherTimer { Interval = TimeSpan.FromMinutes(5) };
            timerAlertas.Tick += async (_, _) => await ActualizarAlertasStockAsync();
            timerAlertas.Start();

            // Navegar a la pantalla de inicio al arrancar
            Dispatcher.CurrentDispatcher.BeginInvoke(() =>
            {
                ContenidoActual = App.Services.GetRequiredService<HomeView>();
            }, System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private async Task ActualizarAlertasStockAsync()
        {
            try
            {
                var inventarioService = App.Services.GetRequiredService<IInventarioService>();
                var count = await inventarioService.ContarProductosConStockBajoAsync();
                ContadorAlertasStock = count;
                HayAlertasStock = count > 0;
            }
            catch { }
        }

        [RelayCommand]
        private void AbrirAlertasStock()
        {
            var ventana = App.Services.GetRequiredService<AlertasStockView>();
            var owner = System.Windows.Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsVisible && w != ventana);
            if (owner != null) ventana.Owner = owner;
            ventana.ShowDialog();
        }

        [RelayCommand]
        private void CerrarSesion()
        {
            var result = MessageBox.Show(
                "¿Está seguro que desea cerrar sesión?",
                "Confirmar",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                SessionService.Instance.CerrarSesion();

                var loginWindow = App.Services.GetRequiredService<LoginWindow>();
                loginWindow.Show();

                foreach (Window window in System.Windows.Application.Current.Windows)
                {
                    if (window is MainWindow)
                    {
                        window.Close();
                        break;
                    }
                }
            }
        }

        [RelayCommand]
        private void AbrirUsuarios()
        {
            if (!PuedeGestionarUsuarios)
            {
                MessageBox.Show("No tiene permisos para gestionar usuarios", "Acceso denegado", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ContenidoActual = App.Services.GetRequiredService<UsuariosView>();
        }

        // ✅ NUEVO - Abrir ventana de actualizaciones
        [RelayCommand]
        private void AbrirActualizaciones()
        {
            var updateView = App.Services.GetRequiredService<UpdateView>();
            updateView.ShowDialog();
        }

        [RelayCommand]
        private void NavegarProductos()
        {
            if (!PuedeGestionarProductos) { MostrarAccesoDenegado(); return; }
            ContenidoActual = App.Services.GetRequiredService<ProductosView>();
        }

        [RelayCommand]
        private void NavegarInventario()
        {
            if (!PuedeGestionarInventario) { MostrarAccesoDenegado(); return; }
            ContenidoActual = App.Services.GetRequiredService<InventarioView>();
        }

        [RelayCommand]
        private void NavegarPOS()
        {
            var posView = App.Services.GetRequiredService<POSView>();
            ContenidoActual = posView;
        }

        [RelayCommand]
        private void NavegarClientes()
        {
            System.Diagnostics.Debug.WriteLine("🔍 NavegarClientes ejecutado");
            
            try
            {
                System.Diagnostics.Debug.WriteLine("🔍 Obteniendo ClientesView...");
                var clientesView = App.Services.GetRequiredService<ClientesView>();
                
                System.Diagnostics.Debug.WriteLine("🔍 Asignando ContenidoActual...");
                ContenidoActual = clientesView;
                
                System.Diagnostics.Debug.WriteLine("✅ ClientesView cargado exitosamente");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error: {ex.Message}");
                MessageBox.Show($"Error al navegar a Clientes:\n\n{ex.Message}\n\n{ex.InnerException?.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void Navegar(string modulo)
        {
            try
            {
                switch (modulo)
                {
                    case "Productos":
                        if (!PuedeGestionarProductos) { MostrarAccesoDenegado(); return; }
                        ContenidoActual = App.Services.GetRequiredService<ProductosView>();
                        break;
                    case "Inventario":
                        if (!PuedeGestionarInventario) { MostrarAccesoDenegado(); return; }
                        ContenidoActual = App.Services.GetRequiredService<InventarioView>();
                        break;
                    case "Clientes":
                        ContenidoActual = App.Services.GetRequiredService<ClientesView>();
                        break;
                    case "HistorialVentas":
                        ContenidoActual = App.Services.GetRequiredService<HistorialVentasView>();
                        break;
                    case "Reportes":
                        if (!PuedeVerReportes) { MostrarAccesoDenegado(); return; }
                        ContenidoActual = App.Services.GetRequiredService<ReportesMenuView>();
                        break;
                    case "Categorias":
                        if (!PuedeGestionarProductos) { MostrarAccesoDenegado(); return; }
                        ContenidoActual = App.Services.GetRequiredService<CategoriasView>();
                        break;
                    case "Proveedores":
                        if (!PuedeGestionarProductos) { MostrarAccesoDenegado(); return; }
                        ContenidoActual = App.Services.GetRequiredService<ProveedoresView>();
                        break;
                    case "Gastos":
                        if (!PuedeGestionarProductos) { MostrarAccesoDenegado(); return; }
                        ContenidoActual = App.Services.GetRequiredService<GastosView>();
                        break;
                    case "Compras":
                        if (!PuedeGestionarProductos) { MostrarAccesoDenegado(); return; }
                        ContenidoActual = App.Services.GetRequiredService<ComprasView>();
                        break;
                    case "Configuracion":
                        if (!PuedeGestionarUsuarios) { MostrarAccesoDenegado(); return; }
                        ContenidoActual = App.Services.GetRequiredService<ConfiguracionView>();
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al navegar a {modulo}:\n\n{ex.Message}\n\n{ex.InnerException?.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static void MostrarAccesoDenegado() =>
            MessageBox.Show("No tiene permisos para acceder a esta sección.",
                "Acceso denegado", MessageBoxButton.OK, MessageBoxImage.Warning);

        [RelayCommand]
        private void NavegarDashboard()
        {
            var dashboardView = App.Services.GetRequiredService<DashboardHubView>();
            ContenidoActual = dashboardView;
        }
    }
}