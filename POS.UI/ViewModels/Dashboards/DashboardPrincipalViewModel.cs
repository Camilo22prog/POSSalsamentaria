using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using POS.Application.DTOs.Dashboard;
using POS.Application.Interfaces;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace POS.UI.ViewModels.Dashboards
{
    public partial class DashboardPrincipalViewModel : ObservableObject
    {
        private readonly IReporteService _reporteService;

        [ObservableProperty]
        private DashboardPrincipalDto? _datos;

        [ObservableProperty]
        private bool _isLoading;

        // Gráfico de ventas semanales
        [ObservableProperty]
        private ISeries[] _seriesVentasSemanales = Array.Empty<ISeries>();

        [ObservableProperty]
        private Axis[] _axisXVentasSemanales = Array.Empty<Axis>();

        public DashboardPrincipalViewModel(IReporteService reporteService)
        {
            _reporteService = reporteService;
        }

        [RelayCommand]
        private async Task CargarDashboardAsync()
        {
            IsLoading = true;

            try
            {
                Datos = await _reporteService.ObtenerDashboardPrincipalAsync();
                
                // Configurar gráfico de ventas semanales
                ConfigurarGraficoVentasSemanales();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al cargar dashboard:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ConfigurarGraficoVentasSemanales()
        {
            if (Datos?.VentasSemanales == null || !Datos.VentasSemanales.Any())
                return;

            SeriesVentasSemanales = new ISeries[]
            {
                new LineSeries<decimal>
                {
                    Values = Datos.VentasSemanales.Select(v => v.Total).ToArray(),
                    Name = "Ventas",
                    Fill = new SolidColorPaint(SKColors.LightBlue.WithAlpha(50)),
                    Stroke = new SolidColorPaint(SKColors.Blue) { StrokeThickness = 3 },
                    GeometrySize = 10,
                    GeometryFill = new SolidColorPaint(SKColors.Blue),
                    GeometryStroke = new SolidColorPaint(SKColors.White) { StrokeThickness = 3 }
                }
            };

            AxisXVentasSemanales = new Axis[]
            {
                new Axis
                {
                    Labels = Datos.VentasSemanales.Select(v => v.Fecha.ToString("dd/MM")).ToArray(),
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