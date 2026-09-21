using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using POS.Application.DTOs.Reports;
using POS.Application.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace POS.UI.ViewModels.Reports
{
    public partial class EstadoResultadosViewModel : ObservableObject
    {
        private readonly IReporteService _reporteService;

        [ObservableProperty]
        private DateTime _fechaInicio = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

        [ObservableProperty]
        private DateTime _fechaFin = DateTime.Now.Date;

        [ObservableProperty]
        private EstadoResultadosDto? _estadoResultados;

        [ObservableProperty]
        private bool _isLoading;

        public EstadoResultadosViewModel(IReporteService reporteService)
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

                EstadoResultados = await _reporteService.ObtenerEstadoResultadosAsync(filtro);
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
            if (EstadoResultados == null)
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
                    FileName = $"Estado_Resultados_{FechaInicio:yyyyMMdd}_{FechaFin:yyyyMMdd}.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    IsLoading = true;

                    var filtro = new FiltroReporteDto
                    {
                        FechaInicio = FechaInicio,
                        FechaFin = FechaFin
                    };

                    var excelData = await _reporteService.ExportarEstadoResultadosAsync(filtro);
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
                case "Mes":
                    FechaInicio = new DateTime(hoy.Year, hoy.Month, 1);
                    FechaFin = hoy;
                    break;
                case "MesAnterior":
                    var primerDiaMesAnterior = new DateTime(hoy.Year, hoy.Month, 1).AddMonths(-1);
                    FechaInicio = primerDiaMesAnterior;
                    FechaFin = primerDiaMesAnterior.AddMonths(1).AddDays(-1);
                    break;
                case "Trimestre":
                    var trimestre = (hoy.Month - 1) / 3;
                    FechaInicio = new DateTime(hoy.Year, trimestre * 3 + 1, 1);
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
}