using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using POS.Application.DTOs.Dashboard;
using POS.Application.Interfaces;
using SkiaSharp;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace POS.UI.ViewModels.Dashboards
{
    public partial class DashboardFinancieroViewModel : ObservableObject
    {
        private readonly IReporteService _reporteService;

        [ObservableProperty]
        private DashboardFinancieroDto? _datos;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private ISeries[] _seriesVentas = Array.Empty<ISeries>();

        [ObservableProperty]
        private Axis[] _axisXVentas = Array.Empty<Axis>();

        public DashboardFinancieroViewModel(IReporteService reporteService)
        {
            _reporteService = reporteService;
        }

        [RelayCommand]
        private async Task CargarDashboardAsync()
        {
            IsLoading = true;
            try
            {
                Datos = await _reporteService.ObtenerDashboardFinancieroAsync();
                ConfigurarGraficoVentas();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar dashboard financiero:\n\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ConfigurarGraficoVentas()
        {
            if (Datos?.VentasMensuales == null || !Datos.VentasMensuales.Any()) return;

            SeriesVentas = new ISeries[]
            {
                new LineSeries<decimal>
                {
                    Values = Datos.VentasMensuales.Select(v => v.Total).ToArray(),
                    Name = "Ingresos",
                    Fill = new SolidColorPaint(new SKColor(39, 174, 96, 40)),
                    Stroke = new SolidColorPaint(new SKColor(39, 174, 96)) { StrokeThickness = 3 },
                    GeometrySize = 8,
                    GeometryFill = new SolidColorPaint(new SKColor(39, 174, 96)),
                    GeometryStroke = new SolidColorPaint(SKColors.White) { StrokeThickness = 2 }
                }
            };

            AxisXVentas = new Axis[]
            {
                new Axis
                {
                    Labels = Datos.VentasMensuales.Select(v => v.Fecha.ToString("dd/MM")).ToArray(),
                    LabelsRotation = 0
                }
            };
        }

        public async Task InicializarAsync()
        {
            await CargarDashboardAsync();
        }
    }
}
