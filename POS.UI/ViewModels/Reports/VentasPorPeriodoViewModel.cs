using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using POS.Application.DTOs.Reports;
using POS.Application.Interfaces;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace POS.UI.ViewModels.Reports
{
    public partial class VentasPorPeriodoViewModel : ObservableObject
    {
        private readonly IReporteService _reporteService;

        [ObservableProperty]
        private DateTime _fechaInicio = DateTime.Now.Date;

        [ObservableProperty]
        private DateTime _fechaFin = DateTime.Now.Date;

        [ObservableProperty]
        private ResumenVentasPorPeriodoDto? _resumen;

        [ObservableProperty]
        private ObservableCollection<VentasPorPeriodoDto> _detalles = new();

        [ObservableProperty]
        private bool _isLoading;

        public VentasPorPeriodoViewModel(IReporteService reporteService)
        {
            _reporteService = reporteService;
            
            System.Diagnostics.Debug.WriteLine("✅ VentasPorPeriodoViewModel creado");
            
            // Establecer fechas del mes actual
            FechaInicio = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            FechaFin = DateTime.Now.Date;
            
            System.Diagnostics.Debug.WriteLine($"📅 Fechas establecidas: {FechaInicio:dd/MM/yyyy} - {FechaFin:dd/MM/yyyy}");
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

                Resumen = await _reporteService.ObtenerVentasPorPeriodoAsync(filtro);
                Detalles = new ObservableCollection<VentasPorPeriodoDto>(Resumen.DetallesPorDia);
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
        private async Task ExportarExcelAsync()
        {
            if (Resumen == null)
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
                    FileName = $"Ventas_Por_Periodo_{FechaInicio:yyyyMMdd}_{FechaFin:yyyyMMdd}.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    IsLoading = true;

                    var filtro = new FiltroReporteDto
                    {
                        FechaInicio = FechaInicio,
                        FechaFin = FechaFin
                    };

                    var excelData = await _reporteService.ExportarVentasPorPeriodoAsync(filtro);
                    await File.WriteAllBytesAsync(saveFileDialog.FileName, excelData);

                    MessageBox.Show(
                        "Reporte exportado exitosamente",
                        "Éxito",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    // Abrir el archivo
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
                case "Ayer":
                    FechaInicio = hoy.AddDays(-1);
                    FechaFin = hoy.AddDays(-1);
                    break;
                case "Semana":
                    FechaInicio = hoy.AddDays(-(int)hoy.DayOfWeek);
                    FechaFin = hoy;
                    break;
                case "Mes":
                    FechaInicio = new DateTime(hoy.Year, hoy.Month, 1);
                    FechaFin = hoy;
                    break;
                case "MesAnterior":
                    var primerDiaMesAnterior = new DateTime(hoy.Year, hoy.Month, 1).AddMonths(-1);
                    FechaInicio = primerDiaMesAnterior;
                    FechaFin = primerDiaMesAnterior.AddMonths(1).AddDays(-1);
                    break;
                case "Año":
                    FechaInicio = new DateTime(hoy.Year, 1, 1);
                    FechaFin = hoy;
                    break;
            }

            _ = GenerarReporteAsync();
        }
    }
}