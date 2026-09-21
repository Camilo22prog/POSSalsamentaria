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
    public partial class AnalisisABCViewModel : ObservableObject
    {
        private readonly IReporteService _reporteService;

        [ObservableProperty]
        private DateTime _fechaInicio = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

        [ObservableProperty]
        private DateTime _fechaFin = DateTime.Now.Date;

        [ObservableProperty]
        private ResumenABCDto? _resumen;

        [ObservableProperty]
        private ObservableCollection<ProductoABCDto> _productos = new();

        [ObservableProperty]
        private string _filtroClasificacion = "Todos";

        [ObservableProperty]
        private bool _isLoading;

        private ObservableCollection<ProductoABCDto> _todosProductos = new();

        public ObservableCollection<string> OpcionesClasificacion { get; } = new()
        {
            "Todos",
            "Clase A",
            "Clase B",
            "Clase C"
        };

        public AnalisisABCViewModel(IReporteService reporteService)
        {
            _reporteService = reporteService;
        }

        partial void OnFiltroClasificacionChanged(string value)
        {
            AplicarFiltro();
        }

        private void AplicarFiltro()
        {
            if (FiltroClasificacion == "Todos")
            {
                Productos = new ObservableCollection<ProductoABCDto>(_todosProductos);
            }
            else
            {
                var clasificacion = FiltroClasificacion switch
                {
                    "Clase A" => "A",
                    "Clase B" => "B",
                    "Clase C" => "C",
                    _ => ""
                };

                var filtrados = _todosProductos.Where(p => p.ClasificacionABC == clasificacion).ToList();
                Productos = new ObservableCollection<ProductoABCDto>(filtrados);
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

                Resumen = await _reporteService.ObtenerAnalisisABCAsync(filtro);
                _todosProductos = new ObservableCollection<ProductoABCDto>(Resumen.Productos);
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
                    FileName = $"Analisis_ABC_{FechaInicio:yyyyMMdd}_{FechaFin:yyyyMMdd}.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    IsLoading = true;

                    var filtro = new FiltroReporteDto
                    {
                        FechaInicio = FechaInicio,
                        FechaFin = FechaFin
                    };

                    var excelData = await _reporteService.ExportarAnalisisABCAsync(filtro);
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