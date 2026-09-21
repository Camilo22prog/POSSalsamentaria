using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Application.DTOs.Catalog;
using POS.Application.Interfaces;
using POS.UI.Services;
using POS.UI.Views.Catalog;
using System.Collections.ObjectModel;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;

namespace POS.UI.ViewModels.Catalog
{
    public partial class ProductosViewModel : ObservableObject
    {
        private readonly IProductoService _productoService;
        private readonly ICategoriaService _categoriaService;
        private readonly IProveedorService _proveedorService;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private bool _inicializando = false;

        [ObservableProperty]
        private ObservableCollection<ProductoDto> _productos = new();

        [ObservableProperty]
        private ObservableCollection<CategoriaDto> _categorias = new();

        [ObservableProperty]
        private ObservableCollection<ProveedorDto> _proveedores = new();

        [ObservableProperty]
        private ProductoDto? _productoSeleccionado;

        [ObservableProperty]
        private string _textoBusqueda = string.Empty;

        [ObservableProperty]
        private int? _categoriaFiltroId;

        [ObservableProperty]
        private int? _proveedorFiltroId;

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private string _mensajeError = string.Empty;

        public ProductosViewModel(IProductoService productoService, ICategoriaService categoriaService, IProveedorService proveedorService)
        {
            _productoService = productoService;
            _categoriaService = categoriaService;
            _proveedorService = proveedorService;
        }

        public async Task CargarAsync()
        {
            _inicializando = true;
            try
            {
                await CargarCategoriasAsync();
                await CargarProveedoresAsync();
                await CargarProductosAsync();
            }
            finally
            {
                _inicializando = false;
            }
        }

        [RelayCommand]
        private async Task CargarProductosAsync()
        {
            // ✅ EVITAR CONCURRENCIA
            await _semaphore.WaitAsync();
            
            IsLoading = true;
            MensajeError = string.Empty;

            try
            {
                IEnumerable<ProductoDto> productos;

                if (!string.IsNullOrWhiteSpace(TextoBusqueda))
                {
                    productos = await _productoService.BuscarAsync(TextoBusqueda);
                    if (CategoriaFiltroId.HasValue && CategoriaFiltroId.Value > 0)
                        productos = productos.Where(p => p.CategoriaId == CategoriaFiltroId.Value);
                    if (ProveedorFiltroId.HasValue && ProveedorFiltroId.Value > 0)
                        productos = productos.Where(p => p.ProveedorId == ProveedorFiltroId.Value);
                }
                else if (ProveedorFiltroId.HasValue && ProveedorFiltroId.Value > 0)
                {
                    productos = await _productoService.ObtenerPorProveedorAsync(ProveedorFiltroId.Value);
                    if (CategoriaFiltroId.HasValue && CategoriaFiltroId.Value > 0)
                        productos = productos.Where(p => p.CategoriaId == CategoriaFiltroId.Value);
                }
                else if (CategoriaFiltroId.HasValue && CategoriaFiltroId.Value > 0)
                {
                    productos = await _productoService.ObtenerPorCategoriaAsync(CategoriaFiltroId.Value);
                }
                else
                {
                    productos = await _productoService.ObtenerActivosAsync();
                }

                Productos.Clear();
                foreach (var producto in productos)
                {
                    Productos.Add(producto);
                }
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al cargar productos: {ex.Message}";
                MessageBox.Show(MensajeError, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
                _semaphore.Release(); // ✅ LIBERAR SEMÁFORO
            }
        }

        private async Task CargarCategoriasAsync()
        {
            try
            {
                var categorias = await _categoriaService.ObtenerActivasAsync();

                Categorias.Clear();
                Categorias.Add(new CategoriaDto { Id = 0, Nombre = "Todas las categorías" });

                foreach (var categoria in categorias)
                    Categorias.Add(categoria);

                CategoriaFiltroId = 0;
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al cargar categorías: {ex.Message}";
                MessageBox.Show(MensajeError, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task CargarProveedoresAsync()
        {
            try
            {
                var proveedores = await _proveedorService.ObtenerActivosAsync();

                Proveedores.Clear();
                Proveedores.Add(new ProveedorDto { Id = 0, Nombre = "Todos los proveedores" });

                foreach (var p in proveedores)
                    Proveedores.Add(p);

                ProveedorFiltroId = 0;
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al cargar proveedores: {ex.Message}";
            }
        }

        [RelayCommand]
        private void NuevoProducto()
        {
            var viewModel = new ProductoFormViewModel(
                _productoService,
                _categoriaService,
                _proveedorService,
                async () => await CargarProductosAsync());

            var window = new ProductoFormView(viewModel);
            _ = viewModel.CargarAsync();
            window.ShowDialog();
        }

        [RelayCommand]
        private void EditarProducto()
        {
            if (ProductoSeleccionado == null)
            {
                MessageBox.Show("Seleccione un producto para editar", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var viewModel = new ProductoFormViewModel(
                _productoService,
                _categoriaService,
                _proveedorService,
                async () => await CargarProductosAsync());

            var window = new ProductoFormView(viewModel);
            _ = viewModel.CargarAsync(ProductoSeleccionado.Id);
            window.ShowDialog();
        }

        [RelayCommand]
        private async Task DesactivarProductoAsync()
        {
            if (ProductoSeleccionado == null)
            {
                MessageBox.Show("Seleccione un producto", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"¿Está seguro de desactivar el producto '{ProductoSeleccionado.Nombre}'?",
                "Confirmar",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _productoService.DesactivarAsync(
                        ProductoSeleccionado.Id, 
                        SessionService.Instance.UsuarioActual?.Id ?? 0);
                    
                    MessageBox.Show("Producto desactivado correctamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    await CargarProductosAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al desactivar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private async Task BuscarAsync()
        {
            await CargarProductosAsync();
        }

        partial void OnCategoriaFiltroIdChanged(int? value)
        {
            if (!_inicializando)
                _ = CargarProductosAsync();
        }

        partial void OnProveedorFiltroIdChanged(int? value)
        {
            if (!_inicializando)
                _ = CargarProductosAsync();
        }
    }
}