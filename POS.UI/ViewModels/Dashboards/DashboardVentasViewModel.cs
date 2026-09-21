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
    public partial class DashboardVentasViewModel : ObservableObject
    {
        private readonly IReporteService _reporteService;

        [ObservableProperty]
        private DashboardVentasDto? _datos;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _periodoSeleccionado = "dia";

        [ObservableProperty]
        private ISeries[] _seriesTendencia = Array.Empty<ISeries>();

        [ObservableProperty]
        private Axis[] _axisXTendencia = Array.Empty<Axis>();

        [ObservableProperty]
        private ISeries[] _seriesCategoria = Array.Empty<ISeries>();

        [ObservableProperty]
        private Axis[] _axisXCategoria = Array.Empty<Axis>();

        public bool IsDiaActivo => PeriodoSeleccionado == "dia";
        public bool IsSemanaActivo => PeriodoSeleccionado == "semana";
        public bool IsMesActivo => PeriodoSeleccionado == "mes";
        public bool IsAnioActivo => PeriodoSeleccionado == "año";

        public string TituloComparativa => PeriodoSeleccionado switch
        {
            "dia" => "vs ayer",
            "semana" => "vs semana anterior",
            "mes" => "vs mes anterior",
            "año" => "vs año anterior",
            _ => "vs período anterior"
        };

        public DashboardVentasViewModel(IReporteService reporteService)
        {
            _reporteService = reporteService;
        }

        partial void OnPeriodoSeleccionadoChanged(string value)
        {
            OnPropertyChanged(nameof(IsDiaActivo));
            OnPropertyChanged(nameof(IsSemanaActivo));
            OnPropertyChanged(nameof(IsMesActivo));
            OnPropertyChanged(nameof(IsAnioActivo));
            OnPropertyChanged(nameof(TituloComparativa));
            _ = CargarDashboardAsync();
        }

        [RelayCommand]
        private void SetPeriodo(string periodo)
        {
            PeriodoSeleccionado = periodo;
        }

        [RelayCommand]
        private async Task CargarDashboardAsync()
        {
            IsLoading = true;
            try
            {
                Datos = await _reporteService.ObtenerDashboardVentasAsync(PeriodoSeleccionado);
                ConfigurarGraficos();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar dashboard de ventas:\n\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ConfigurarGraficos()
        {
            if (Datos == null) return;
            ConfigurarGraficoTendencia();
            ConfigurarGraficoCategoria();
        }

        private void ConfigurarGraficoTendencia()
        {
            if (!Datos!.VentasTendencia.Any()) return;

            SeriesTendencia = new ISeries[]
            {
                new LineSeries<decimal>
                {
                    Values = Datos.VentasTendencia.Select(v => v.Total).ToArray(),
                    Name = "Ventas",
                    Fill = new SolidColorPaint(new SKColor(52, 152, 219, 40)),
                    Stroke = new SolidColorPaint(new SKColor(52, 152, 219)) { StrokeThickness = 3 },
                    GeometrySize = 10,
                    GeometryFill = new SolidColorPaint(new SKColor(52, 152, 219)),
                    GeometryStroke = new SolidColorPaint(SKColors.White) { StrokeThickness = 3 }
                }
            };

            var labels = PeriodoSeleccionado == "año"
                ? Datos.VentasTendencia.Select(v => v.Fecha.ToString("MMM")).ToArray()
                : Datos.VentasTendencia.Select(v => v.Fecha.ToString("dd/MM")).ToArray();

            AxisXTendencia = new Axis[] { new Axis { Labels = labels, LabelsRotation = 0 } };
        }

        private void ConfigurarGraficoCategoria()
        {
            if (!Datos!.VentasPorCategoria.Any()) return;

            var colores = new SKColor[]
            {
                new(52, 152, 219), new(39, 174, 96), new(155, 89, 182),
                new(230, 126, 34), new(26, 188, 156), new(231, 76, 60),
                new(52, 73, 94), new(241, 196, 15)
            };

            SeriesCategoria = new ISeries[]
            {
                new ColumnSeries<decimal>
                {
                    Values = Datos.VentasPorCategoria.Select(c => c.Total).ToArray(),
                    Name = "Ventas",
                    Fill = new SolidColorPaint(new SKColor(52, 152, 219)),
                    MaxBarWidth = 60
                }
            };

            AxisXCategoria = new Axis[]
            {
                new Axis
                {
                    Labels = Datos.VentasPorCategoria.Select(c => c.NombreCategoria).ToArray(),
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
