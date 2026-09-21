using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Application.DTOs.Inventory;
using POS.Application.Interfaces;
using System.Collections.ObjectModel;
using System.Windows;

namespace POS.UI.ViewModels.Inventory
{
    public partial class AlertasStockViewModel : ObservableObject
    {
        private readonly IInventarioService _inventarioService;

        [ObservableProperty]
        private ObservableCollection<InventarioProductoDto> _productosAlerta = new();

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _resumen = string.Empty;

        public AlertasStockViewModel(IInventarioService inventarioService)
        {
            _inventarioService = inventarioService;
            _ = CargarAsync();
        }

        [RelayCommand]
        private async Task CargarAsync()
        {
            IsLoading = true;
            try
            {
                var lista = await _inventarioService.ObtenerProductosConStockBajoAsync();
                var ordenados = lista
                    .OrderBy(p => p.StockActual)
                    .ToList();

                ProductosAlerta = new ObservableCollection<InventarioProductoDto>(ordenados);

                var sinStock = ordenados.Count(p => p.StockActual <= 0);
                var bajoMinimo = ordenados.Count(p => p.StockActual > 0);
                Resumen = $"{sinStock} sin stock  •  {bajoMinimo} bajo mínimo";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar alertas: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally { IsLoading = false; }
        }

        [RelayCommand]
        private void Cerrar()
        {
            System.Windows.Application.Current.Windows
                .OfType<Window>()
                .FirstOrDefault(w => w.DataContext == this)
                ?.Close();
        }
    }
}
