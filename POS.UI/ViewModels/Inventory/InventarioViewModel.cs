using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Application.DTOs.Inventory;
using POS.Application.Interfaces;
using POS.UI.Views.Inventory;
using System.Collections.ObjectModel;
using System.Windows;

namespace POS.UI.ViewModels.Inventory
{
    public partial class InventarioViewModel : ObservableObject
    {
        private readonly IInventarioService _inventarioService;

        [ObservableProperty]
        private ObservableCollection<InventarioProductoDto> _inventario = new();

        [ObservableProperty]
        private InventarioProductoDto? _productoSeleccionado;

        [ObservableProperty]
        private string _textoBusqueda = string.Empty;

        [ObservableProperty]
        private bool _soloStockBajo = false;

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private string _mensajeError = string.Empty;

        [ObservableProperty]
        private decimal _valorTotalInventario;

        [ObservableProperty]
        private int _productosConStock;

        [ObservableProperty]
        private int _productosStockBajo;

        public InventarioViewModel(IInventarioService inventarioService)
        {
            _inventarioService = inventarioService;
        }

        public async Task CargarAsync()
        {
            await CargarInventarioAsync();
            await CargarEstadisticasAsync();
        }

        [RelayCommand]
        private async Task CargarInventarioAsync()
        {
            IsLoading = true;
            MensajeError = string.Empty;

            try
            {
                IEnumerable<InventarioProductoDto> inventario;

                if (SoloStockBajo)
                {
                    inventario = await _inventarioService.ObtenerProductosConStockBajoAsync();
                }
                else
                {
                    inventario = await _inventarioService.ObtenerInventarioActualAsync();
                }

                if (!string.IsNullOrWhiteSpace(TextoBusqueda))
                {
                    var busqueda = TextoBusqueda.ToLower();
                    inventario = inventario.Where(i => 
                        i.Nombre.ToLower().Contains(busqueda) || 
                        i.Codigo.ToLower().Contains(busqueda));
                }

                Inventario.Clear();
                foreach (var item in inventario)
                {
                    Inventario.Add(item);
                }
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al cargar inventario: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task CargarEstadisticasAsync()
        {
            try
            {
                ValorTotalInventario = await _inventarioService.ObtenerValorTotalInventarioAsync();
                ProductosConStock = await _inventarioService.ContarProductosConStockAsync();
                ProductosStockBajo = await _inventarioService.ContarProductosConStockBajoAsync();
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al cargar estadísticas: {ex.Message}";
            }
        }

        [RelayCommand]
        private void AjustarStock()
        {
            if (ProductoSeleccionado == null)
            {
                MessageBox.Show("Seleccione un producto", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var viewModel = new AjustarStockViewModel(
                _inventarioService,
                ProductoSeleccionado,
                async () => await CargarAsync());

            var window = new AjustarStockView(viewModel);
            window.ShowDialog();
        }

        [RelayCommand]
        private void VerKardex()
        {
            if (ProductoSeleccionado == null)
            {
                MessageBox.Show("Seleccione un producto", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var viewModel = new KardexViewModel(_inventarioService, ProductoSeleccionado.ProductoId);
            var window = new KardexView(viewModel);
            _ = viewModel.CargarAsync();
            window.ShowDialog();
        }

        [RelayCommand]
        private async Task BuscarAsync()
        {
            await CargarInventarioAsync();
        }

        partial void OnSoloStockBajoChanged(bool value)
        {
            _ = CargarInventarioAsync();
        }
    }
}