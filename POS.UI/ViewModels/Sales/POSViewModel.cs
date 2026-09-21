using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Application.DTOs.Sales;
using POS.Application.Interfaces;
using POS.UI.Helpers;
using POS.UI.Services;
using POS.UI.Views.Sales;
using System.Collections.ObjectModel;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;


namespace POS.UI.ViewModels.Sales
{
    public partial class POSViewModel : ObservableObject
    {
        private readonly IProductoService _productoService;
        private readonly IVentaService _ventaService;
        private readonly ICajaService _cajaService;
        private readonly IClienteService _clienteService;
        private readonly IAuthService _authService;
        private readonly IServiceProvider _serviceProvider;
        private int _contadorPestanas = 0;
        private const int MAX_PESTANAS = 7;

        /// <summary>Última venta completada — usada por F7 para reimprimir.</summary>
        public static VentaDto? UltimaVenta { get; set; }

        [ObservableProperty]
        private ObservableCollection<VentaPestanaViewModel> _pestanas = new();

        [ObservableProperty]
        private VentaPestanaViewModel? _pestanaActiva;

        [ObservableProperty]
        private int? _cajaId;

        [ObservableProperty]
        private string _nombreUsuario = string.Empty;

        [ObservableProperty]
        private bool _tieneCajaAbierta = false;

        public POSViewModel(
            IProductoService productoService,
            IVentaService ventaService,
            ICajaService cajaService,
            IClienteService clienteService,
            IAuthService authService,
            IServiceProvider serviceProvider)
        {
            _productoService = productoService;
            _ventaService = ventaService;
            _cajaService = cajaService;
            _clienteService = clienteService;
            _authService = authService;
            _serviceProvider = serviceProvider;

            NombreUsuario = SessionService.Instance.UsuarioActual?.NombreCompleto ?? "";
        }

        public async Task InicializarAsync()
        {
            try
            {
                // Verificar si hay caja abierta
                var usuarioId = SessionService.Instance.UsuarioActual?.Id ?? 0;
                
                var caja = await _cajaService.ObtenerCajaAbiertaPorUsuarioAsync(usuarioId);

                if (caja != null)
                {
                    CajaId = caja.Id;
                    TieneCajaAbierta = true;

                    // Crear primera pestaña
                    NuevaPestana();
                }
                else
                {
                    TieneCajaAbierta = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al inicializar: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void AbrirCaja()
        {
            try
            {
                var abrirCajaViewModel = _serviceProvider.GetRequiredService<AbrirCajaViewModel>();
                var abrirCajaView = new AbrirCajaView(abrirCajaViewModel);
                abrirCajaView.ShowDialog();

                // Recargar estado después de abrir caja
                _ = InicializarAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al abrir ventana: {ex.Message}\n\n{ex.StackTrace}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void RegistrarRetiro()
        {
            if (!CajaId.HasValue)
            {
                MessageBox.Show("No hay caja abierta", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var retiroViewModel = new RetiroCajaViewModel(_cajaService, _authService, CajaId.Value);
                var retiroView = new RetiroCajaView(retiroViewModel);
                retiroView.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al abrir retiro: {ex.Message}\n\n{ex.StackTrace}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void CerrarCaja()
        {
            if (!CajaId.HasValue)
            {
                MessageBox.Show("No hay caja abierta", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Confirmar cierre
            var result = MessageBox.Show(
                "⚠️ ¿Está seguro que desea CERRAR la caja?\n\n" +
                "Se realizará el arqueo final y no podrá realizar más ventas hasta abrir una nueva caja.",
                "Confirmar Cierre de Caja",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                var cerrarCajaViewModel = new CerrarCajaViewModel(_cajaService, CajaId.Value);
                var cerrarCajaView = new CerrarCajaView(cerrarCajaViewModel);
                
                var dialogResult = cerrarCajaView.ShowDialog();

                // Solo actualizar si efectivamente se cerró la caja
                if (dialogResult == true)
                {
                    CajaId = null;
                    TieneCajaAbierta = false;
                    Pestanas.Clear();

                    MessageBox.Show(
                        "Caja cerrada exitosamente.\n\nPara realizar ventas debe abrir una nueva caja.",
                        "Caja Cerrada",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al cerrar caja: {ex.Message}\n\n{ex.StackTrace}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void AbrirProductosPorPeso()
        {
            if (!CajaId.HasValue)
            {
                MessageBox.Show("No hay caja abierta", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (PestanaActiva == null)
            {
                MessageBox.Show("No hay ninguna venta activa", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var vista = _serviceProvider.GetRequiredService<ProductosPorPesoView>();

                // ── Crear la ventana ANTES de configurar callbacks ──────────────
                // Es necesario crearla primero para poder pasarle ventana.Close()
                // al callback de cierre.
                var ventana = new Window
                {
                    Content = vista,
                    Title = "Productos por Peso",
                    Width = 550,
                    Height = 650,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    ResizeMode = ResizeMode.NoResize,
                    Owner = System.Windows.Application.Current.Windows
                            .OfType<Window>()
                            .FirstOrDefault(w => w.IsActive)
                };

                if (vista.DataContext is ProductosPorPesoViewModel viewModel)
                {
                    // Al agregar producto → lo mete en la pestaña activa
                    viewModel.SetCallbackAgregarProducto(PestanaActiva.AgregarProductoPorPeso);
                    // Al agregar producto → cierra la ventana automáticamente
                    viewModel.SetCallbackCerrarModulo(() => ventana.Close());
                }

                ventana.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}\n\n{ex.StackTrace}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void NuevaPestana()
        {
            if (Pestanas.Count >= MAX_PESTANAS)
            {
                MessageBox.Show(
                    $"Solo se permiten {MAX_PESTANAS} ventas simultáneas",
                    "Límite alcanzado",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (!CajaId.HasValue)
            {
                MessageBox.Show("No hay caja abierta", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _contadorPestanas++;
            var nuevaPestana = new VentaPestanaViewModel(
                _productoService,
                _clienteService,  // ✅ AGREGAR
                _contadorPestanas,
                CajaId.Value,
                CerrarPestana);

            Pestanas.Add(nuevaPestana);
            PestanaActiva = nuevaPestana;
        }

        private void CerrarPestana(VentaPestanaViewModel pestana)
        {
            Pestanas.Remove(pestana);

            if (Pestanas.Any())
            {
                PestanaActiva = Pestanas.Last();
            }
            else
            {
                // Si no hay pestañas, crear una nueva
                NuevaPestana();
            }
        }

        [RelayCommand]
        private void ImprimirUltimaVenta()
        {
            if (UltimaVenta == null)
            {
                MessageBox.Show("No hay venta reciente para imprimir.", "Información",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            try
            {
                var printer = new TicketPrinter();
                printer.ImprimirTicket(UltimaVenta, abrirCajon: false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al imprimir: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void CerrarPestanaActual()
        {
            if (PestanaActiva != null)
            {
                PestanaActiva.CancelarVentaCommand.Execute(null);
            }
        }
    }
}