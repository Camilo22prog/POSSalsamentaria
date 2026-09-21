using CommunityToolkit.Mvvm.ComponentModel;
using POS.Application.Interfaces;
using POS.UI.Services;
using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace POS.UI.ViewModels
{
    public partial class HomeViewModel : ObservableObject
    {
        private readonly IReporteService _reporteService;
        private readonly DispatcherTimer _timer;

        [ObservableProperty] private string _horaActual = string.Empty;
        [ObservableProperty] private string _fechaActual = string.Empty;
        [ObservableProperty] private string _saludoHora = string.Empty;
        [ObservableProperty] private string _nombreUsuario = string.Empty;
        [ObservableProperty] private string _rolUsuario = string.Empty;

        // Estadísticas rápidas del día
        [ObservableProperty] private decimal _ventasHoy;
        [ObservableProperty] private int _transaccionesHoy;
        [ObservableProperty] private bool _cajaAbierta;
        [ObservableProperty] private int _productosStockBajo;
        [ObservableProperty] private bool _statsLoaded;

        public HomeViewModel(IReporteService reporteService)
        {
            _reporteService = reporteService;

            var usuario = SessionService.Instance.UsuarioActual;
            NombreUsuario = usuario?.NombreCompleto ?? "Usuario";
            RolUsuario = usuario?.RolNombre ?? string.Empty;

            ActualizarTiempo();

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += (_, _) => ActualizarTiempo();
            _timer.Start();
        }

        private void ActualizarTiempo()
        {
            var now = DateTime.Now;
            HoraActual = now.ToString("HH:mm:ss");

            var cultura = new CultureInfo("es-CO");
            FechaActual = char.ToUpper(now.ToString("dddd, dd 'de' MMMM 'de' yyyy", cultura)[0])
                          + now.ToString("dddd, dd 'de' MMMM 'de' yyyy", cultura)[1..];

            SaludoHora = now.Hour < 12 ? "Buenos días" : now.Hour < 18 ? "Buenas tardes" : "Buenas noches";
        }

        public async Task InicializarAsync()
        {
            try
            {
                var datos = await _reporteService.ObtenerDashboardPrincipalAsync();
                VentasHoy = datos.VentasHoy;
                TransaccionesHoy = datos.TransaccionesHoy;
                CajaAbierta = datos.CajaAbierta;
                ProductosStockBajo = datos.ProductosStockBajo;
                StatsLoaded = true;
            }
            catch
            {
                StatsLoaded = false;
            }
        }
    }
}
