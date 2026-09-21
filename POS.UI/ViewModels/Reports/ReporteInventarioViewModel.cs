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
    public partial class ReporteInventarioViewModel : ObservableObject
    {
        private readonly IReporteService _reporteService;

        [ObservableProperty]
        private ResumenInventarioDto? _resumen;

        [ObservableProperty]
        private ObservableCollection<InventarioReporteDto> _productos = new();

        [ObservableProperty]
        private string _filtroTexto = string.Empty;

        [ObservableProperty]
        private string _filtroEstado = "Todos";

        [ObservableProperty]
        private bool _isLoading;

        private ObservableCollection<InventarioReporteDto> _todosProductos = new();

        public ObservableCollection<string> OpcionesEstado { get; } = new()
        {
            "Todos",
            "Stock Bajo",
            "Stock Normal",
            "Sin Stock"
        };

        public ReporteInventarioViewModel(IReporteService reporteService)
        {
            _reporteService = reporteService;
        }

        partial void OnFiltroTextoChanged(string value)
        {
            AplicarFiltros();
        }

        partial void OnFiltroEstadoChanged(string value)
        {
            AplicarFiltros();
        }

        private void AplicarFiltros()
        {
            var filtrados = _todosProductos.AsEnumerable();

            // Filtro por texto
            if (!string.IsNullOrWhiteSpace(FiltroTexto))
            {
                filtrados = filtrados.Where(p =>
                    p.Nombre.Contains(FiltroTexto, StringComparison.OrdinalIgnoreCase) ||
                    p.Codigo.Contains(FiltroTexto, StringComparison.OrdinalIgnoreCase) ||
                    p.Categoria.Contains(FiltroTexto, StringComparison.OrdinalIgnoreCase)
                );
            }

            // Filtro por estado
            if (FiltroEstado != "Todos")
            {
                filtrados = FiltroEstado switch
                {
                    "Stock Bajo" => filtrados.Where(p => p.EstadoStock == "Stock Bajo"),
                    "Stock Normal" => filtrados.Where(p => p.EstadoStock == "Stock Normal"),
                    "Sin Stock" => filtrados.Where(p => p.EstadoStock == "Sin Stock"),
                    _ => filtrados
                };
            }

            Productos = new ObservableCollection<InventarioReporteDto>(filtrados);
        }

        [RelayCommand]
        private async Task GenerarReporteAsync()
        {
            IsLoading = true;

            try
            {
                var resultado = await _reporteService.ObtenerReporteInventarioAsync();
                
                Resumen = resultado;
                _todosProductos = new ObservableCollection<InventarioReporteDto>(resultado.Productos);
                AplicarFiltros();
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
            if (Resumen == null || Productos.Count == 0)
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
                    FileName = $"Inventario_{DateTime.Now:yyyyMMdd}.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    IsLoading = true;

                    var excelData = await _reporteService.ExportarInventarioAsync();
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