using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using POS.Application.DTOs.Reports;
using POS.Application.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace POS.UI.ViewModels.Reports
{
    public partial class VentasPorMetodoPagoViewModel : ObservableObject
    {
        private readonly IReporteService _reporteService;

        [ObservableProperty]
        private DateTime _fechaInicio = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

        [ObservableProperty]
        private DateTime _fechaFin = DateTime.Now.Date;

        [ObservableProperty]
        private ObservableCollection<MetodoPagoConDetalles> _metodosPago = new();

        [ObservableProperty]
        private bool _isLoading;

        public VentasPorMetodoPagoViewModel(IReporteService reporteService)
        {
            _reporteService = reporteService;
        }

        [RelayCommand]
        private async Task GenerarReporteAsync()
        {
            if (FechaInicio > FechaFin)
            {
                MessageBox.Show(
                    "La fecha de inicio no puede ser mayor a la fecha fin",
                    "Validación",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            IsLoading = true;

            try
            {
                var filtro = new FiltroReporteDto
                {
                    FechaInicio = FechaInicio,
                    FechaFin = FechaFin
                };

                var resultado = await _reporteService.ObtenerVentasPorMetodoPagoAsync(filtro);
                
                MetodosPago = new ObservableCollection<MetodoPagoConDetalles>(
                    resultado.Select(m =>
                    {
                        var item = new MetodoPagoConDetalles
                        {
                            Metodo = m.Metodo,
                            MetodoTexto = m.MetodoTexto,
                            CantidadTransacciones = m.CantidadTransacciones,
                            MontoTotal = m.MontoTotal,
                            PorcentajeDelTotal = m.PorcentajeDelTotal,
                            Detalles = new ObservableCollection<DetallePagoDto>(m.Detalles),
                            IsExpanded = false
                        };
                        // Pasar servicio y filtro para permitir exportación
                        item.SetServicioYFiltro(_reporteService, filtro);
                        return item;
                    })
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al generar reporte:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task ExportarGeneralAsync()
        {
            if (MetodosPago == null || MetodosPago.Count == 0)
            {
                MessageBox.Show(
                    "Primero debe generar el reporte",
                    "Validación",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel Files|*.xlsx",
                    FileName = $"Ventas_Por_Metodo_Pago_{FechaInicio:yyyyMMdd}_{FechaFin:yyyyMMdd}.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    IsLoading = true;

                    var filtro = new FiltroReporteDto
                    {
                        FechaInicio = FechaInicio,
                        FechaFin = FechaFin
                    };

                    var excelData = await _reporteService.ExportarVentasPorMetodoPagoGeneralAsync(filtro);
                    await File.WriteAllBytesAsync(saveFileDialog.FileName, excelData);

                    MessageBox.Show(
                        "Reporte exportado exitosamente",
                        "Éxito",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = saveFileDialog.FileName,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al exportar:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void EstablecerRangoRapido(string rango)
        {
            var hoy = DateTime.Now.Date;

            switch (rango)
            {
                case "Hoy":
                    FechaInicio = hoy;
                    FechaFin = hoy;
                    break;
                case "Semana":
                    FechaInicio = hoy.AddDays(-(int)hoy.DayOfWeek);
                    FechaFin = hoy;
                    break;
                case "Mes":
                    FechaInicio = new DateTime(hoy.Year, hoy.Month, 1);
                    FechaFin = hoy;
                    break;
                case "Año":
                    FechaInicio = new DateTime(hoy.Year, 1, 1);
                    FechaFin = hoy;
                    break;
            }

            _ = GenerarReporteAsync();
        }
    }

    // ✅ Clase separada fuera de VentasPorMetodoPagoViewModel
    public partial class MetodoPagoConDetalles : ObservableObject
    {
        private IReporteService? _reporteService;
        private FiltroReporteDto? _filtro;

        public POS.Domain.Enums.MetodoPago Metodo { get; set; }
        public string MetodoTexto { get; set; } = string.Empty;
        public int CantidadTransacciones { get; set; }
        public decimal MontoTotal { get; set; }
        public decimal PorcentajeDelTotal { get; set; }
        public ObservableCollection<DetallePagoDto> Detalles { get; set; } = new();
        
        [ObservableProperty]
        private bool _isExpanded;

        public void SetServicioYFiltro(IReporteService servicio, FiltroReporteDto filtro)
        {
            _reporteService = servicio;
            _filtro = filtro;
        }

        [RelayCommand]
        private async Task ExportarDetalleAsync()
        {
            if (_reporteService == null || _filtro == null)
            {
                MessageBox.Show("Error al exportar", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel Files|*.xlsx",
                    FileName = $"Detalle_{MetodoTexto.Replace(" ", "_")}_{_filtro.FechaInicio:yyyyMMdd}_{_filtro.FechaFin:yyyyMMdd}.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var excelData = await _reporteService.ExportarVentasPorMetodoPagoDetalladoAsync(_filtro, Metodo);
                    await File.WriteAllBytesAsync(saveFileDialog.FileName, excelData);

                    MessageBox.Show(
                        "Detalle exportado exitosamente",
                        "Éxito",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = saveFileDialog.FileName,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al exportar:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}