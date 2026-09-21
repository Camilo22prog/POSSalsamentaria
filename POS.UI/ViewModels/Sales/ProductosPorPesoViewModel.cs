using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Application.DTOs.Catalog;
using POS.Application.Interfaces;
using POS.Domain.Enums;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace POS.UI.ViewModels.Sales
{
    public partial class ProductosPorPesoViewModel : ObservableObject
    {
        private readonly IProductoService _productoService;
        private readonly IBalanzaService  _balanzaService;

        private Action<ProductoDto, decimal>? _onProductoAgregado;
        private Action? _onCerrarModulo;

        public ObservableCollection<CategoriaInfo> Categorias { get; set; }

        [ObservableProperty] private ObservableCollection<ProductoDto> _productosDisponibles = new();
        [ObservableProperty] private ProductoDto?    _productoSeleccionado;
        [ObservableProperty] private decimal         _pesoIngresado;
        [ObservableProperty] private CategoriaInfo?  _categoriaActual;
        [ObservableProperty] private bool            _mostrandoProductos;
        [ObservableProperty] private bool            _balanzaConectada;
        [ObservableProperty] private string          _puertoBalanza = "COM3";

        // Índice de la categoría resaltada por teclado (0..Categorias.Count-1)
        [ObservableProperty] private int _indiceCategoriaFocused = 0;

        public ProductosPorPesoViewModel(IProductoService productoService, IBalanzaService balanzaService)
        {
            _productoService = productoService;
            _balanzaService  = balanzaService;

            Categorias = new ObservableCollection<CategoriaInfo>
            {
                new CategoriaInfo { Categoria = CategoriaProductoPeso.Queso,   Nombre = "Quesos",  Icono = "🧀", Color = "#FFF9C4" },
                new CategoriaInfo { Categoria = CategoriaProductoPeso.Jamon,   Nombre = "Jamón",   Icono = "🥓", Color = "#FFCCBC" },
                new CategoriaInfo { Categoria = CategoriaProductoPeso.Pollo,   Nombre = "Pollo",   Icono = "🍗", Color = "#FFE0B2" },
                new CategoriaInfo { Categoria = CategoriaProductoPeso.Harinas, Nombre = "Harinas", Icono = "🌾", Color = "#F0F4C3" },
                new CategoriaInfo { Categoria = CategoriaProductoPeso.Otros,   Nombre = "Otros",   Icono = "📦", Color = "#E1BEE7" }
            };
        }

        // ─────────────────────────────────────────────────────────────────────
        // INICIALIZACIÓN Y CALLBACKS
        // ─────────────────────────────────────────────────────────────────────

        public async Task InicializarAsync()
        {
            BalanzaConectada = _balanzaService.EstaConectada;
        }

        public void SetCallbackAgregarProducto(Action<ProductoDto, decimal> callback)
            => _onProductoAgregado = callback;

        public void SetCallbackCerrarModulo(Action callback)
            => _onCerrarModulo = callback;

        /// Llamado desde el code-behind cuando el usuario presiona Esc en categorías.
        public void CerrarModulo() => _onCerrarModulo?.Invoke();

        // ─────────────────────────────────────────────────────────────────────
        // NAVEGACIÓN POR TECLADO — solo lógica, sin UI
        // El code-behind llama a estos métodos al capturar KeyDown.
        // ─────────────────────────────────────────────────────────────────────

        public void NavCategoriaArriba()
        {
            if (IndiceCategoriaFocused > 0)
                IndiceCategoriaFocused--;
        }

        public void NavCategoriaAbajo()
        {
            if (IndiceCategoriaFocused < Categorias.Count - 1)
                IndiceCategoriaFocused++;
        }

        public async Task ConfirmarCategoriaFocusedAsync()
            => await SeleccionarCategoriaAsync(Categorias[IndiceCategoriaFocused].Categoria);

        public void NavProductoArriba()
        {
            if (!ProductosDisponibles.Any()) return;
            if (ProductoSeleccionado == null) { ProductoSeleccionado = ProductosDisponibles.Last(); return; }
            int idx = ProductosDisponibles.IndexOf(ProductoSeleccionado);
            if (idx > 0) ProductoSeleccionado = ProductosDisponibles[idx - 1];
        }

        public void NavProductoAbajo()
        {
            if (!ProductosDisponibles.Any()) return;
            if (ProductoSeleccionado == null) { ProductoSeleccionado = ProductosDisponibles.First(); return; }
            int idx = ProductosDisponibles.IndexOf(ProductoSeleccionado);
            if (idx < ProductosDisponibles.Count - 1) ProductoSeleccionado = ProductosDisponibles[idx + 1];
        }

        // ─────────────────────────────────────────────────────────────────────
        // CATEGORÍA
        // ─────────────────────────────────────────────────────────────────────

        [RelayCommand]
        private async Task SeleccionarCategoriaAsync(CategoriaProductoPeso categoria)
        {
            try
            {
                CategoriaActual = Categorias.First(c => c.Categoria == categoria);
                var productosDto = await _productoService.ObtenerActivosAsync();

                ProductosDisponibles = new ObservableCollection<ProductoDto>(
                    productosDto.Where(p => p.VentaPorPeso && p.CategoriaPeso == categoria));

                if (!ProductosDisponibles.Any())
                {
                    MessageBox.Show($"No hay productos en la categoría {CategoriaActual.Nombre}.",
                        "Sin productos", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                MostrandoProductos   = true;
                PesoIngresado        = 0;
                // Preseleccionar el primero para que ↑↓ funcionen de inmediato
                ProductoSeleccionado = ProductosDisponibles.First();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar productos: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void Volver()
        {
            MostrandoProductos   = false;
            ProductoSeleccionado = null;
            PesoIngresado        = 0;
            ProductosDisponibles.Clear();
        }

        // ─────────────────────────────────────────────────────────────────────
        // CARRITO — sin MessageBox de confirmación, cierra el módulo solo
        // ─────────────────────────────────────────────────────────────────────

        [RelayCommand]
        private void AgregarAlCarrito()
        {
            if (ProductoSeleccionado == null)
            {
                MessageBox.Show("Seleccione un producto de la lista.",
                    "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (PesoIngresado <= 0)
            {
                MessageBox.Show("El peso debe ser mayor a cero.\nPresione P para leer la balanza.",
                    "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                decimal precioTotal      = ProductoSeleccionado.PrecioPorKilo * PesoIngresado;
                decimal precioRedondeado = RedondearAPesoColombianos(precioTotal);
                decimal pesoAjustado     = precioRedondeado / ProductoSeleccionado.PrecioPorKilo;

                var productoParaAgregar = ProductoSeleccionado;
                var pesoParaAgregar     = pesoAjustado;

                // Limpiar estado antes de invocar callbacks
                ProductoSeleccionado   = null;
                PesoIngresado          = 0;
                MostrandoProductos     = false;
                IndiceCategoriaFocused = 0;
                ProductosDisponibles.Clear();

                // 1. Agregar al carrito
                _onProductoAgregado?.Invoke(productoParaAgregar, pesoParaAgregar);
                // 2. Cerrar módulo automáticamente
                _onCerrarModulo?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al agregar producto: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // BALANZA
        // ─────────────────────────────────────────────────────────────────────

        [RelayCommand]
        private async Task LeerPesoBalanzaAsync()
        {
            if (!BalanzaConectada)
            {
                MessageBox.Show("La balanza no está conectada.", "Advertencia",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                var peso = await _balanzaService.LeerPesoAsync();
                if      (peso > 0)  PesoIngresado = peso;
                else if (peso == 0) MessageBox.Show("La balanza indica 0 kg. Coloque el producto y vuelva a leer.",
                                        "Sin peso", MessageBoxButton.OK, MessageBoxImage.Warning);
                else                MessageBox.Show("No se pudo leer el peso. Verifique la conexión.",
                                        "Error de lectura", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al leer peso: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task ConectarBalanzaAsync()
        {
            try
            {
                var puertos = _balanzaService.ObtenerPuertosDisponibles();
                if (!puertos.Contains(PuertoBalanza))
                {
                    MessageBox.Show($"El puerto {PuertoBalanza} no existe.\nDisponibles: {string.Join(", ", puertos)}",
                        "Puerto no válido", MessageBoxButton.OK, MessageBoxImage.Warning);
                    BalanzaConectada = false;
                    return;
                }
                var conectado = await _balanzaService.ConectarAsync(PuertoBalanza);
                BalanzaConectada = conectado;
                MessageBox.Show(conectado ? $"Balanza conectada en {PuertoBalanza}" : $"No se pudo conectar en {PuertoBalanza}",
                    conectado ? "Éxito" : "Error", MessageBoxButton.OK,
                    conectado ? MessageBoxImage.Information : MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                BalanzaConectada = false;
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task DetectarBalanzaAsync()
        {
            try
            {
                var puerto = await _balanzaService.DetectarPuertoAsync();
                if (puerto != null)
                {
                    PuertoBalanza    = puerto;
                    BalanzaConectada = true;
                    MessageBox.Show($"¡Balanza encontrada en {puerto}!", "Éxito",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                    MessageBox.Show("No se encontró ninguna balanza.", "No encontrada",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void DesconectarBalanza()
        {
            try
            {
                _balanzaService.Desconectar();
                BalanzaConectada = false;
                PesoIngresado    = 0;
                MessageBox.Show("Balanza desconectada.", "Información",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al desconectar: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // UTILIDADES
        // ─────────────────────────────────────────────────────────────────────

        private decimal RedondearAPesoColombianos(decimal valor)
            => Math.Round(valor / 50m) * 50m;
    }

    public class CategoriaInfo
    {
        public CategoriaProductoPeso Categoria { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Icono  { get; set; } = string.Empty;
        public string Color  { get; set; } = string.Empty;
    }
}