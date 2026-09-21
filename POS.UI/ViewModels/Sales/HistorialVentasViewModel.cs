using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Application.DTOs.Sales;
using POS.Application.Interfaces;
using POS.UI.Helpers;
using POS.UI.Services;
using POS.UI.Views.Sales;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows;

namespace POS.UI.ViewModels.Sales
{
    public partial class HistorialVentasViewModel : ObservableObject
    {
        private readonly IVentaService _ventaService;
        private List<VentaDto> _todasLasVentas = new();

        [ObservableProperty]
        private ObservableCollection<VentaDto> _ventas = new();

        [ObservableProperty]
        private VentaDto? _ventaSeleccionada;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private DateTime _fechaInicio = DateTime.Now.Date;

        [ObservableProperty]
        private DateTime _fechaFin = DateTime.Now.Date;

        [ObservableProperty]
        private string _textoBusqueda = string.Empty;

        // Filtro estado: "Todas", "Vigentes", "Anuladas"
        [ObservableProperty]
        private string _filtroEstado = "Todas";

        // Resumen
        [ObservableProperty]
        private int _totalVentas;

        [ObservableProperty]
        private decimal _montoTotal;

        [ObservableProperty]
        private int _ventasAnuladas;

        [ObservableProperty]
        private decimal _promedioVenta;

        public HistorialVentasViewModel(IVentaService ventaService)
        {
            _ventaService = ventaService;
            _ = CargarVentasAsync();
        }

        partial void OnTextoBusquedaChanged(string value) => AplicarFiltros();
        partial void OnFiltroEstadoChanged(string value) => AplicarFiltros();

        [RelayCommand]
        private async Task CargarVentasAsync()
        {
            IsLoading = true;
            try
            {
                var ventas = await _ventaService.ObtenerVentasPorRangoAsync(FechaInicio, FechaFin);
                _todasLasVentas = ventas.OrderByDescending(v => v.Fecha).ToList();
                AplicarFiltros();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar ventas:\n\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void AplicarFiltros()
        {
            var resultado = _todasLasVentas.AsEnumerable();

            if (FiltroEstado == "Vigentes")
                resultado = resultado.Where(v => !v.Anulada);
            else if (FiltroEstado == "Anuladas")
                resultado = resultado.Where(v => v.Anulada);

            if (!string.IsNullOrWhiteSpace(TextoBusqueda))
            {
                var texto = TextoBusqueda.Trim().ToLower();
                resultado = resultado.Where(v =>
                    v.NumeroVenta.ToLower().Contains(texto) ||
                    (v.ClienteNombre ?? "").ToLower().Contains(texto) ||
                    (v.UsuarioNombre ?? "").ToLower().Contains(texto));
            }

            var lista = resultado.ToList();
            Ventas = new ObservableCollection<VentaDto>(lista);

            var vigentes = lista.Where(v => !v.Anulada).ToList();
            TotalVentas = vigentes.Count;
            MontoTotal = vigentes.Sum(v => v.Total);
            VentasAnuladas = lista.Count(v => v.Anulada);
            PromedioVenta = vigentes.Count > 0 ? vigentes.Average(v => v.Total) : 0;
        }

        [RelayCommand]
        private void FiltroHoy()
        {
            FechaInicio = DateTime.Now.Date;
            FechaFin = DateTime.Now.Date;
            _ = CargarVentasAsync();
        }

        [RelayCommand]
        private void FiltroAyer()
        {
            FechaInicio = DateTime.Now.Date.AddDays(-1);
            FechaFin = DateTime.Now.Date.AddDays(-1);
            _ = CargarVentasAsync();
        }

        [RelayCommand]
        private void FiltroSemana()
        {
            FechaInicio = DateTime.Now.Date.AddDays(-6);
            FechaFin = DateTime.Now.Date;
            _ = CargarVentasAsync();
        }

        [RelayCommand]
        private void FiltroMes()
        {
            FechaInicio = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            FechaFin = DateTime.Now.Date;
            _ = CargarVentasAsync();
        }

        [RelayCommand]
        private void ReimprimirTicket()
        {
            if (VentaSeleccionada == null)
            {
                MessageBox.Show("Seleccione una venta para reimprimir.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (VentaSeleccionada.Anulada)
            {
                var confirmar = MessageBox.Show(
                    $"La venta {VentaSeleccionada.NumeroVenta} está ANULADA.\n¿Desea imprimir el ticket de todas formas?",
                    "Venta Anulada",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);
                if (confirmar != MessageBoxResult.Yes) return;
            }

            try
            {
                var printer = new TicketPrinter();
                printer.ImprimirTicket(VentaSeleccionada, abrirCajon: false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al reimprimir:\n\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void ImprimirVentasDelDia()
        {
            var ventasActivas = Ventas.Where(v => !v.Anulada).ToList();
            if (!ventasActivas.Any())
            {
                MessageBox.Show("No hay ventas vigentes para imprimir.", "Información",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var doc = new PrintDocument();
                doc.DefaultPageSettings.PaperSize = new PaperSize("A4", 827, 1169);
                doc.DefaultPageSettings.Margins = new Margins(40, 40, 40, 40);

                doc.PrintPage += (s, e) =>
                {
                    if (e.Graphics == null) return;
                    var fontTitulo = new System.Drawing.Font("Arial", 14, System.Drawing.FontStyle.Bold);
                    var fontEncabezado = new System.Drawing.Font("Arial", 9, System.Drawing.FontStyle.Bold);
                    var fontNormal = new System.Drawing.Font("Arial", 9, System.Drawing.FontStyle.Regular);

                    float x = e.MarginBounds.Left;
                    float y = e.MarginBounds.Top;

                    var rango = FechaInicio == FechaFin
                        ? FechaInicio.ToString("dd/MM/yyyy")
                        : $"{FechaInicio:dd/MM/yyyy} – {FechaFin:dd/MM/yyyy}";

                    e.Graphics.DrawString($"VENTAS — {rango}", fontTitulo, Brushes.Black, x, y);
                    y += 24;
                    e.Graphics.DrawString(new string('-', 80), fontNormal, Brushes.Black, x, y);
                    y += 14;

                    e.Graphics.DrawString("# Venta", fontEncabezado, Brushes.Black, x, y);
                    e.Graphics.DrawString("Fecha/Hora", fontEncabezado, Brushes.Black, x + 130, y);
                    e.Graphics.DrawString("Cliente", fontEncabezado, Brushes.Black, x + 240, y);
                    e.Graphics.DrawString("Usuario", fontEncabezado, Brushes.Black, x + 420, y);
                    e.Graphics.DrawString("Total", fontEncabezado, Brushes.Black, x + 560, y);
                    y += 16;
                    e.Graphics.DrawString(new string('-', 80), fontNormal, Brushes.Black, x, y);
                    y += 12;

                    decimal totalGeneral = 0;
                    foreach (var v in ventasActivas)
                    {
                        e.Graphics.DrawString(v.NumeroVenta, fontNormal, Brushes.Black, x, y);
                        e.Graphics.DrawString(v.Fecha.ToString("dd/MM HH:mm"), fontNormal, Brushes.Black, x + 130, y);
                        var cliente = v.ClienteNombre?.Length > 22 ? v.ClienteNombre[..22] : (v.ClienteNombre ?? "-");
                        e.Graphics.DrawString(cliente, fontNormal, Brushes.Black, x + 240, y);
                        e.Graphics.DrawString(v.UsuarioNombre ?? "", fontNormal, Brushes.Black, x + 420, y);
                        e.Graphics.DrawString(v.Total.ToString("C0"), fontNormal, Brushes.Black, x + 560, y);
                        totalGeneral += v.Total;
                        y += 14;
                    }

                    y += 4;
                    e.Graphics.DrawString(new string('=', 80), fontNormal, Brushes.Black, x, y);
                    y += 12;
                    e.Graphics.DrawString(
                        $"TOTAL: {totalGeneral:C0}  ({ventasActivas.Count} ventas)",
                        new System.Drawing.Font("Arial", 11, System.Drawing.FontStyle.Bold),
                        Brushes.Black, x, y);
                };

                doc.Print();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al imprimir: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void AnularVenta()
        {
            if (VentaSeleccionada == null)
            {
                MessageBox.Show("Seleccione una venta para anular.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (VentaSeleccionada.Anulada)
            {
                MessageBox.Show("Esta venta ya fue anulada previamente.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!SessionService.Instance.PuedeAnularVentas())
            {
                MessageBox.Show("No tiene permisos para anular ventas.", "Acceso Denegado",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var anularViewModel = new AnularVentaViewModel(
                    _ventaService,
                    VentaSeleccionada.Id,
                    VentaSeleccionada.NumeroVenta,
                    async () => await CargarVentasAsync());

                var anularView = new AnularVentaView(anularViewModel);
                anularView.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir ventana:\n\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
