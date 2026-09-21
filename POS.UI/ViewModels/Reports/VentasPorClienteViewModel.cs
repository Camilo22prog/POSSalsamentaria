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
    public partial class VentasPorClienteViewModel : ObservableObject
    {
        private readonly IReporteService _reporteService;

        [ObservableProperty]
        private DateTime _fechaInicio = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

        [ObservableProperty]
        private DateTime _fechaFin = DateTime.Now.Date;

        [ObservableProperty]
        private ObservableCollection<VentasPorClienteDto> _clientes = new();

        [ObservableProperty]
        private string _filtroTexto = string.Empty;

        [ObservableProperty]
        private bool _isLoading;

        private ObservableCollection<VentasPorClienteDto> _todosClientes = new();

        public VentasPorClienteViewModel(IReporteService reporteService)
        {
            _reporteService = reporteService;
        }

        partial void OnFiltroTextoChanged(string value)
        {
            AplicarFiltro();
        }

        private void AplicarFiltro()
        {
            if (string.IsNullOrWhiteSpace(FiltroTexto))
            {
                Clientes = new ObservableCollection<VentasPorClienteDto>(_todosClientes);
            }
            else
            {
                var filtrados = _todosClientes.Where(c =>
                    c.ClienteNombre.Contains(FiltroTexto, StringComparison.OrdinalIgnoreCase) ||
                    c.NumeroDocumento.Contains(FiltroTexto, StringComparison.OrdinalIgnoreCase)
                ).ToList();

                Clientes = new ObservableCollection<VentasPorClienteDto>(filtrados);
            }
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

                var resultado = await _reporteService.ObtenerVentasPorClienteAsync(filtro);
                _todosClientes = new ObservableCollection<VentasPorClienteDto>(resultado);
                AplicarFiltro();
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

        [RelayCommand]
        private async Task ExportarExcelAsync()
        {
            if (Clientes == null || Clientes.Count == 0)
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
                    FileName = $"Ventas_Por_Cliente_{FechaInicio:yyyyMMdd}_{FechaFin:yyyyMMdd}.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    IsLoading = true;

                    var filtro = new FiltroReporteDto
                    {
                        FechaInicio = FechaInicio,
                        FechaFin = FechaFin
                    };

                    var excelData = await _reporteService.ExportarVentasPorClienteAsync(filtro);
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
    }
}