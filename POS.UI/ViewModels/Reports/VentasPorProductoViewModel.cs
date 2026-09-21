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
    public partial class VentasPorProductoViewModel : ObservableObject
    {
        private readonly IReporteService _reporteService;

        [ObservableProperty]
        private DateTime _fechaInicio = DateTime.Now.Date.AddMonths(-1);

        [ObservableProperty]
        private DateTime _fechaFin = DateTime.Now.Date;

        [ObservableProperty]
        private ObservableCollection<VentasPorProductoDto> _productos = new();

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _filtroTexto = string.Empty;

        private List<VentasPorProductoDto> _productosTodos = new();

        public VentasPorProductoViewModel(IReporteService reporteService)
        {
            _reporteService = reporteService;
        }

        [RelayCommand]
        private async Task GenerarReporteAsync()
        {
            IsLoading = true;

            try
            {
                var filtro = new FiltroReporteDto
                {
                    FechaInicio = FechaInicio,
                    FechaFin = FechaFin
                };

                _productosTodos = await _reporteService.ObtenerVentasPorProductoAsync(filtro);
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

        partial void OnFiltroTextoChanged(string value)
        {
            AplicarFiltro();
        }

        private void AplicarFiltro()
        {
            if (string.IsNullOrWhiteSpace(FiltroTexto))
            {
                Productos = new ObservableCollection<VentasPorProductoDto>(_productosTodos);
            }
            else
            {
                var filtrados = _productosTodos.Where(p =>
                    p.Nombre.Contains(FiltroTexto, StringComparison.OrdinalIgnoreCase) ||
                    p.Codigo.Contains(FiltroTexto, StringComparison.OrdinalIgnoreCase) ||
                    p.Categoria.Contains(FiltroTexto, StringComparison.OrdinalIgnoreCase)
                ).ToList();

                Productos = new ObservableCollection<VentasPorProductoDto>(filtrados);
            }
        }

        [RelayCommand]
        private async Task ExportarExcelAsync()
        {
            if (!Productos.Any())
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
                    FileName = $"Ventas_Por_Producto_{FechaInicio:yyyyMMdd}_{FechaFin:yyyyMMdd}.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    IsLoading = true;

                    var filtro = new FiltroReporteDto
                    {
                        FechaInicio = FechaInicio,
                        FechaFin = FechaFin
                    };

                    var excelData = await _reporteService.ExportarVentasPorProductoAsync(filtro);
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
    }
}