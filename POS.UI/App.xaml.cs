using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using POS.Infrastructure.Data;
using POS.Domain.Interfaces.Repositories;
using POS.Infrastructure.Repositories;
using POS.Application.Interfaces;
using POS.Application.Services;
using POS.UI.Services;
using POS.UI.ViewModels;
using POS.UI.ViewModels.Catalog;
using POS.UI.ViewModels.Inventory;
using POS.UI.ViewModels.Customers;
using POS.UI.ViewModels.Sales;   
using POS.UI.ViewModels.Reports;
using POS.UI.ViewModels.Expenses;
using POS.UI.ViewModels.Settings;
using POS.UI.Views;
using POS.UI.Views.Customers;
using POS.UI.Views.Catalog;
using POS.UI.Views.Expenses;
using POS.UI.Views.Settings;
using POS.UI.Views.Inventory;
using POS.UI.Views.Sales;
using POS.UI.Views.Reports;
using POS.UI.Views.Dashboards;
using POS.UI.ViewModels.Dashboards;
using POS.UI.ViewModels.Security;
using POS.UI.Views.Security;
using POS.Application.Services;
using POS.UI.ViewModels.Catalog;
using POS.UI.Views.Catalog;
using POS.UI.ViewModels.Purchases;
using POS.UI.Views.Purchases;


namespace POS.UI
{
    public partial class App : System.Windows.Application
    {
        private readonly IHost _host;

        public static IServiceProvider Services => ((App)Current)._host.Services;

        public App()
        {
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // DbContext: Transient para que cada servicio tenga su propia instancia
                    // y evitar el error de operaciones concurrentes en el mismo DbContext
                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        var connectionString = context.Configuration.GetConnectionString("DefaultConnection") 
                            ?? "Server=localhost\\SQLEXPRESS;Database=POSSalsamentaria;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";
                        options.UseSqlServer(
                            connectionString,
                            o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
                        );
                    },
                    ServiceLifetime.Transient,
                    ServiceLifetime.Transient);

                    // Repositories
                    services.AddTransient<IUnitOfWork, UnitOfWork>();

                    // Services
                    services.AddTransient<IAuthService, AuthService>();
                    services.AddTransient<IProductoService, ProductoService>();
                    services.AddTransient<ICategoriaService, CategoriaService>();
                    services.AddTransient<IProveedorService, ProveedorService>();
                    services.AddTransient<IInventarioService, InventarioService>();
                    services.AddTransient<IVentaService, VentaService>();
                    services.AddTransient<ICajaService, CajaService>();
                    services.AddTransient<IClienteService, ClienteService>();
                    services.AddTransient<IReporteService, ReporteService>();
                    services.AddTransient<IGastoService, GastoService>();
                    services.AddTransient<IConfiguracionService, ConfiguracionService>();
                    services.AddTransient<ICompraService, CompraService>();
                    services.AddTransient<IUsuarioService, POS.Application.Services.UsuarioService>();
                    services.AddSingleton<IUpdateService, UpdateService>();
                    // Balanza
                    services.AddSingleton<IBalanzaService, BalanzaService>(); 

                    // ViewModels
                    services.AddTransient<LoginViewModel>();
                    services.AddTransient<MainViewModel>();
                    services.AddTransient<ProductosViewModel>();
                    services.AddTransient<ProductoFormViewModel>();
                    services.AddTransient<InventarioViewModel>();
                    services.AddTransient<AjustarStockViewModel>();
                    services.AddTransient<KardexViewModel>();
                    services.AddTransient<POSViewModel>();
                    services.AddTransient<VentaPestanaViewModel>();
                    services.AddTransient<ProcesarPagoViewModel>();
                    services.AddTransient<AbrirCajaViewModel>();
                    services.AddTransient<AbrirCajonManualViewModel>();
                    services.AddTransient<ClientesViewModel>();
                    services.AddTransient<ClienteFormViewModel>();
                    services.AddTransient<HistorialVentasViewModel>();
                    services.AddTransient<AnularVentaViewModel>();
                    services.AddTransient<ReportesMenuViewModel>();
                    services.AddTransient<VentasPorPeriodoViewModel>();
                    services.AddTransient<VentasPorProductoViewModel>();
                    services.AddTransient<VentasPorClienteViewModel>();
                    services.AddTransient<VentasPorMetodoPagoViewModel>();
                    services.AddTransient<EstadoResultadosViewModel>();
                    services.AddTransient<ReporteInventarioViewModel>();
                    services.AddTransient<AnalisisABCViewModel>();
                    services.AddTransient<DesempenoUsuarioViewModel>();
                    services.AddTransient<ReporteArqueosViewModel>();
                    services.AddTransient<VentasAnuladasViewModel>();
                    services.AddTransient<HomeViewModel>();
                    services.AddTransient<DashboardHubViewModel>();
                    services.AddTransient<DashboardPrincipalViewModel>();
                    services.AddTransient<DashboardVentasViewModel>();
                    services.AddTransient<DashboardInventarioViewModel>();
                    services.AddTransient<DashboardFinancieroViewModel>();
                    services.AddTransient<ProductosPorPesoViewModel>();
                    services.AddTransient<UsuariosViewModel>();
                    services.AddTransient<UpdateViewModel>();
                    services.AddTransient<CategoriasViewModel>();
                    services.AddTransient<ProveedoresViewModel>();
                    services.AddTransient<GastosViewModel>();
                    services.AddTransient<ConfiguracionViewModel>();
                    services.AddTransient<ComprasViewModel>();
                    services.AddTransient<NuevaCompraViewModel>();
                    services.AddTransient<AlertasStockViewModel>();
                    // Views
                    services.AddTransient<LoginWindow>();
                    services.AddTransient<MainWindow>();
                    services.AddTransient<ProductosView>();
                    services.AddTransient<ProductoFormView>();
                    services.AddTransient<InventarioView>();
                    services.AddTransient<AjustarStockView>();
                    services.AddTransient<KardexView>();  
                    services.AddTransient<POSView>();
                    services.AddTransient<ProcesarPagoView>(); 
                    services.AddTransient<AbrirCajaView>();   
                    services.AddTransient<CerrarCajaView>();  
                    services.AddTransient<RetiroCajaView>();
                    services.AddTransient<ClientesView>();
                    services.AddTransient<ClienteFormView>();
                    services.AddTransient<HistorialVentasView>();       
                    services.AddTransient<AnularVentaView>();
                    services.AddTransient<ReportesMenuView>();
                    services.AddTransient<VentasPorPeriodoView>();
                    services.AddTransient<VentasPorProductoView>();   
                    services.AddTransient<VentasPorClienteView>();     
                    services.AddTransient<VentasPorMetodoPagoView>();   
                    services.AddTransient<EstadoResultadosView>();
                    services.AddTransient<ReporteInventarioView>();    
                    services.AddTransient<AnalisisABCView>();  
                    services.AddTransient<DesempenoUsuarioView>();  
                    services.AddTransient<ReporteArqueosView>();   
                    services.AddTransient<VentasAnuladasView>();
                    services.AddTransient<HomeView>();
                    services.AddTransient<DashboardHubView>();
                    services.AddTransient<DashboardPrincipalView>();
                    services.AddTransient<DashboardVentasView>();
                    services.AddTransient<DashboardInventarioView>();
                    services.AddTransient<DashboardFinancieroView>();
                    services.AddTransient<ProductosPorPesoView>();
                    services.AddTransient<UsuariosView>();
                    services.AddTransient<UpdateView>();
                    services.AddTransient<CategoriasView>();
                    services.AddTransient<ProveedoresView>();
                    services.AddTransient<GastosView>();
                    services.AddTransient<ConfiguracionView>();
                    services.AddTransient<ComprasView>();
                    services.AddTransient<NuevaCompraView>();
                    services.AddTransient<AlertasStockView>();
                })
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await _host.StartAsync();

            // Cargar configuración del negocio en memoria antes de mostrar UI
            await CargarConfiguracionAsync();

            var loginWindow = Services.GetRequiredService<LoginWindow>();
            loginWindow.Show();

            base.OnStartup(e);

            // Conectar balanza al iniciar (en segundo plano)
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(3000);

                    var balanza = Services.GetRequiredService<IBalanzaService>();
                    System.Diagnostics.Debug.WriteLine("🔌 Iniciando conexión automática de balanza...");

                    var puertoCfg = AppConfig.Current.PuertoBalanza;
                    var baud      = AppConfig.Current.BaudRateBalanza;

                    string? puerto;
                    if (!string.IsNullOrEmpty(puertoCfg))
                    {
                        // Usar puerto configurado directamente
                        var ok = await balanza.ConectarAsync(puertoCfg, baud);
                        puerto = ok ? puertoCfg : null;
                    }
                    else
                    {
                        puerto = await balanza.DetectarPuertoAsync();
                    }

                    if (puerto != null)
                        System.Diagnostics.Debug.WriteLine($"✅ Balanza conectada en {puerto}");
                    else
                        System.Diagnostics.Debug.WriteLine("⚠️ No se encontró balanza al iniciar");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ Error al conectar balanza: {ex.Message}");
                }
            });
        }

        private static async Task CargarConfiguracionAsync()
        {
            try
            {
                var cfgService = Services.GetRequiredService<IConfiguracionService>();
                var dto = await cfgService.ObtenerAsync();
                AppConfig.Cargar(dto);

                // Pasar URL de actualizaciones al servicio singleton
                var updateService = Services.GetRequiredService<IUpdateService>();
                updateService.ConfigurarUrl(dto.UrlActualizaciones);

                System.Diagnostics.Debug.WriteLine($"✅ Configuración cargada: {dto.NombreNegocio}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Error al cargar configuración: {ex.Message}");
            }
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            // ✅ Desconectar balanza al salir
            try
            {
                var balanza = Services.GetRequiredService<IBalanzaService>();
                balanza.Desconectar();
            }
            catch { }

            await _host.StopAsync();
            _host.Dispose();

            base.OnExit(e);
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"Error no manejado:\n\n{e.Exception.Message}\n\n{e.Exception.StackTrace}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            MessageBox.Show($"Error crítico:\n\n{ex?.Message}\n\n{ex?.StackTrace}",
                "Error Fatal",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}