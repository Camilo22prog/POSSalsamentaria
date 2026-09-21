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
    public partial class DashboardInventarioViewModel : ObservableObject
    {
        private readonly IReporteService _reporteService;

        [ObservableProperty]
        private DashboardInventarioDto? _datos;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private ISeries[] _seriesStock = Array.Empty<ISeries>();

        [ObservableProperty]
        private Axis[] _axisXStock = Array.Empty<Axis>();

        public DashboardInventarioViewModel(IReporteService reporteService)
        {
            _reporteService = reporteService;
        }

        [RelayCommand]
        private async Task CargarDashboardAsync()
        {
            IsLoading = true;
            try
            {
                Datos = await _reporteService.ObtenerDashboardInventarioAsync();
                ConfigurarGraficoStock();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar dashboard de inventario:\n\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ConfigurarGraficoStock()
        {
            if (Datos?.StockPorCategoria == null || !Datos.StockPorCategoria.Any()) return;

            SeriesStock = new ISeries[]
            {
                new ColumnSeries<decimal>
                {
                    Values = Datos.StockPorCategoria.Select(c => c.ValorTotal).ToArray(),
                    Name = "Valor en stock",
                    Fill = new SolidColorPaint(new SKColor(39, 174, 96)),
                    MaxBarWidth = 60
                }
            };

            AxisXStock = new Axis[]
            {
                new Axis
                {
                    Labels = Datos.StockPorCategoria.Select(c => c.NombreCategoria).ToArray(),
                    LabelsRotation = -30
                }
            };
        }

        public async Task InicializarAsync()
        {
            await CargarDashboardAsync();
        }
    }
}
