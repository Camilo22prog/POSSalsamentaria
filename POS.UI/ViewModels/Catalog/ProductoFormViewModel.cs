using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Application.DTOs.Catalog;
using POS.Application.Interfaces;
using POS.Domain.Enums;
using POS.UI.Services;
using POS.UI.Views.Catalog;
using System.Collections.ObjectModel;
using System.Windows;

namespace POS.UI.ViewModels.Catalog
{
    public partial class ProductoFormViewModel : ObservableObject
    {
        private readonly IProductoService _productoService;
        private readonly ICategoriaService _categoriaService;
        private readonly IProveedorService _proveedorService;
        private readonly Action _onGuardado;
        private int? _productoId;

        [ObservableProperty]
        private string _titulo = "Nuevo Producto";

        [ObservableProperty]
        private string _codigo = string.Empty;

        [ObservableProperty]
        private string _nombre = string.Empty;

        [ObservableProperty]
        private string _descripcion = string.Empty;

        [ObservableProperty]
        private int? _categoriaId;

        [ObservableProperty]
        private ObservableCollection<CategoriaDto> _categorias = new();

        [ObservableProperty]
        private TipoUnidad _tipoUnidad = TipoUnidad.Unidad;

        [ObservableProperty]
        private ObservableCollection<TipoUnidadItem> _tiposUnidad = new();

        [ObservableProperty]
        private TipoIVA _tipoIVA = TipoIVA.Diecinueve;

        [ObservableProperty]
        private ObservableCollection<TipoIVAItem> _tiposIVA = new();

        [ObservableProperty]
        private decimal _precioCompra;

        [ObservableProperty]
        private decimal _precioVenta;

        // Campos calculados — NO se persisten en BD
        // _recalculando evita loops infinitos entre los campos relacionados
        private bool _recalculando = false;

        [ObservableProperty]
        private decimal _precioCompraConIVA;

        [ObservableProperty]
        private decimal _margenGanancia = 30m;

        [ObservableProperty]
        private decimal _precioVentaSugerido;

        [ObservableProperty]
        private decimal _stockMinimo;

        [ObservableProperty]
        private string _unidadMedida = string.Empty;

        [ObservableProperty]
        private bool _controlaVencimiento = false;

        [ObservableProperty]
        private int _diasAlertaVencimiento = 7;

        [ObservableProperty]
        private bool _permiteFraccionado = false;

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private string _mensajeError = string.Empty;

        [ObservableProperty]
        private bool _productoGuardado;

        // ✅ Campos de venta por peso
        [ObservableProperty]
        private bool _ventaPorPeso;

        [ObservableProperty]
        private CategoriaProductoPeso? _categoriaPeso;

        [ObservableProperty]
        private decimal _precioPorKilo;

        [ObservableProperty]
        private bool _mostrarCamposPeso;

        // Lista de categorías de peso para el ComboBox
        public ObservableCollection<CategoriasPesoItem> CategoriasPeso { get; set; }

        [ObservableProperty]
        private ObservableCollection<ProveedorDto> _proveedores = new();

        [ObservableProperty]
        private int? _proveedorId;

        public ProductoFormViewModel(
            IProductoService productoService,
            ICategoriaService categoriaService,
            IProveedorService proveedorService,
            Action onGuardado)
        {
            _productoService = productoService;
            _categoriaService = categoriaService;
            _proveedorService = proveedorService;
            _onGuardado = onGuardado;

            CargarTiposUnidad();
            CargarTiposIVA();
            
            // ✅ Inicializar categorías de peso
            CategoriasPeso = new ObservableCollection<CategoriasPesoItem>
            {
                new CategoriasPesoItem { Valor = CategoriaProductoPeso.Queso, Nombre = "Quesos", Icono = "🧀" },
                new CategoriasPesoItem { Valor = CategoriaProductoPeso.Jamon, Nombre = "Jamón", Icono = "🥓" },
                new CategoriasPesoItem { Valor = CategoriaProductoPeso.Pollo, Nombre = "Pollo", Icono = "🍗" },
                new CategoriasPesoItem { Valor = CategoriaProductoPeso.Harinas, Nombre = "Harinas", Icono = "🌾" },
                new CategoriasPesoItem { Valor = CategoriaProductoPeso.Otros, Nombre = "Otros", Icono = "📦" }
            };
        }

        public async Task CargarAsync(int? productoId = null)
        {
            _productoId = productoId;
            Titulo = productoId.HasValue ? "Editar Producto" : "Nuevo Producto";

            await CargarCategoriasAsync();
            await CargarProveedoresAsync();

            if (productoId.HasValue)
            {
                await CargarProductoAsync(productoId.Value);
            }
        }

        private async Task CargarProveedoresAsync()
        {
            try
            {
                var lista = await _proveedorService.ObtenerActivosAsync();
                Proveedores.Clear();
                Proveedores.Add(new ProveedorDto { Id = 0, Nombre = "Sin proveedor" });
                foreach (var p in lista)
                    Proveedores.Add(p);
            }
            catch { /* no bloquear si falla */ }
        }

        public async Task CargarCategoriasAsync()
        {
            try
            {
                var categorias = await _categoriaService.ObtenerActivasAsync();
                Categorias.Clear();
                foreach (var categoria in categorias)
                {
                    Categorias.Add(categoria);
                }

                if (Categorias.Any() && !CategoriaId.HasValue)
                {
                    CategoriaId = Categorias.First().Id;
                }
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al cargar categorías: {ex.Message}";
            }
        }

        private async Task CargarProductoAsync(int id)
        {
            IsLoading = true;
            try
            {
                var producto = await _productoService.ObtenerPorIdAsync(id);
                if (producto != null)
                {
                    // Suprimir cascadas de precios durante la carga masiva
                    _recalculando = true;
                    Codigo = producto.Codigo;
                    Nombre = producto.Nombre;
                    Descripcion = producto.Descripcion ?? string.Empty;
                    CategoriaId = producto.CategoriaId;
                    TipoUnidad = producto.TipoUnidad;
                    TipoIVA = producto.TipoIVA;
                    PrecioCompra = producto.PrecioCompra;
                    PrecioVenta = producto.PrecioVenta;
                    StockMinimo = producto.StockMinimo;
                    UnidadMedida = producto.UnidadMedida ?? string.Empty;
                    ControlaVencimiento = producto.ControlaVencimiento;
                    PermiteFraccionado = producto.PermiteFraccionado;
                    VentaPorPeso = producto.VentaPorPeso;
                    CategoriaPeso = producto.CategoriaPeso;
                    PrecioPorKilo = producto.PrecioPorKilo;
                    MostrarCamposPeso = producto.VentaPorPeso || producto.TipoUnidad == TipoUnidad.Peso || producto.TipoUnidad == TipoUnidad.Ambos;
                    ProveedorId = producto.ProveedorId.HasValue ? producto.ProveedorId : 0;
                    _recalculando = false;

                    // Calcular todos los campos derivados con los datos ya cargados
                    RecalcularTodo();
                }
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al cargar producto: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void CargarTiposUnidad()
        {
            TiposUnidad.Clear();
            TiposUnidad.Add(new TipoUnidadItem { Valor = TipoUnidad.Unidad, Texto = "Unidad" });
            TiposUnidad.Add(new TipoUnidadItem { Valor = TipoUnidad.Peso, Texto = "Peso" });
            TiposUnidad.Add(new TipoUnidadItem { Valor = TipoUnidad.Ambos, Texto = "Unidad/Peso" });
        }

        private void CargarTiposIVA()
        {
            TiposIVA.Clear();
            TiposIVA.Add(new TipoIVAItem { Valor = TipoIVA.Exento, Texto = "Exento (0%)" });
            TiposIVA.Add(new TipoIVAItem { Valor = TipoIVA.Cinco, Texto = "5%" });
            TiposIVA.Add(new TipoIVAItem { Valor = TipoIVA.Diecinueve, Texto = "19%" });
        }

        // ─── Lógica reactiva de precios ──────────────────────────────────────────

        // Porcentaje IVA como decimal (0, 0.05, 0.19)
        private decimal PorcentajeIVADecimal => (decimal)TipoIVA / 100m;

        // Recalcula todo desde PrecioCompra (sin IVA) y PrecioVenta actuales.
        // Llamar explícitamente al cargar un producto existente.
        private void RecalcularTodo()
        {
            _recalculando = true;
            try
            {
                PrecioCompraConIVA = Math.Round(PrecioCompra * (1 + PorcentajeIVADecimal), 2);
                // Deriva el margen real si ya existe un precio de venta final
                if (PrecioCompraConIVA > 0 && PrecioVenta > 0)
                    MargenGanancia = Math.Round((PrecioVenta / PrecioCompraConIVA - 1) * 100m, 2);
                PrecioVentaSugerido = PrecioCompraConIVA > 0
                    ? Math.Round(PrecioCompraConIVA * (1 + MargenGanancia / 100m), 2)
                    : 0m;
            }
            finally { _recalculando = false; }
        }

        private void RecalcularSugerido()
        {
            PrecioVentaSugerido = PrecioCompraConIVA > 0
                ? Math.Round(PrecioCompraConIVA * (1 + MargenGanancia / 100m), 2)
                : 0m;
        }

        // Usuario edita Precio Compra SIN IVA → actualiza conIVA y sugerido
        partial void OnPrecioCompraChanged(decimal value)
        {
            if (_recalculando) return;
            _recalculando = true;
            try
            {
                PrecioCompraConIVA = Math.Round(value * (1 + PorcentajeIVADecimal), 2);
                RecalcularSugerido();
            }
            finally { _recalculando = false; }
        }

        // Usuario edita Precio Compra CON IVA → back-calcula sinIVA y actualiza sugerido
        partial void OnPrecioCompraConIVAChanged(decimal value)
        {
            if (_recalculando) return;
            _recalculando = true;
            try
            {
                var pct = PorcentajeIVADecimal;
                PrecioCompra = pct > 0
                    ? Math.Round(value / (1 + pct), 2)
                    : value;
                RecalcularSugerido();
            }
            finally { _recalculando = false; }
        }

        // Usuario edita Margen (%) → actualiza sugerido
        partial void OnMargenGananciaChanged(decimal value)
        {
            if (_recalculando) return;
            _recalculando = true;
            try { RecalcularSugerido(); }
            finally { _recalculando = false; }
        }

        // Usuario edita Precio Venta Final → back-calcula el margen real sobre precio con IVA
        partial void OnPrecioVentaChanged(decimal value)
        {
            if (_recalculando) return;
            _recalculando = true;
            try
            {
                if (PrecioCompraConIVA > 0 && value > 0)
                    MargenGanancia = Math.Round((value / PrecioCompraConIVA - 1) * 100m, 2);
            }
            finally { _recalculando = false; }
        }

        // Cambio de IVA → mantiene PrecioCompra sinIVA fijo, recalcula conIVA
        partial void OnTipoIVAChanged(TipoIVA value)
        {
            if (_recalculando) return;
            _recalculando = true;
            try
            {
                PrecioCompraConIVA = Math.Round(PrecioCompra * (1 + (decimal)value / 100m), 2);
            }
            finally { _recalculando = false; }
        }

        // ─── Lógica de tipo de unidad (peso) ─────────────────────────────────────

        // ✅ Detectar cambios en TipoUnidad para mostrar/ocultar campos de peso
        partial void OnTipoUnidadChanged(TipoUnidad value)
        {
            MostrarCamposPeso = value == TipoUnidad.Peso || value == TipoUnidad.Ambos;

            if (value == TipoUnidad.Peso)
            {
                VentaPorPeso = true;
            }
            else if (value == TipoUnidad.Unidad)
            {
                VentaPorPeso = false;
                CategoriaPeso = null;
                PrecioPorKilo = 0;
            }
        }

        [RelayCommand]
        private async Task GuardarAsync()
        {
            if (!ValidarFormulario())
                return;

            IsLoading = true;
            MensajeError = string.Empty;

            try
            {
                var dto = new CrearProductoDto
                {
                    Codigo = Codigo.Trim(),
                    Nombre = Nombre.Trim(),
                    Descripcion = string.IsNullOrWhiteSpace(Descripcion) ? null : Descripcion.Trim(),
                    CategoriaId = CategoriaId!.Value,
                    TipoUnidad = TipoUnidad,
                    TipoIVA = TipoIVA,
                    PrecioCompra = PrecioCompra,
                    PrecioVenta = PrecioVenta,
                    StockMinimo = StockMinimo,
                    UnidadMedida = string.IsNullOrWhiteSpace(UnidadMedida) ? null : UnidadMedida.Trim(),
                    ControlaVencimiento = ControlaVencimiento,
                    DiasAlertaVencimiento = ControlaVencimiento ? DiasAlertaVencimiento : null,
                    PermiteFraccionado = PermiteFraccionado,
                    
                    // ✅ Campos de peso
                    VentaPorPeso = VentaPorPeso,
                    CategoriaPeso = CategoriaPeso,
                    PrecioPorKilo = PrecioPorKilo,

                    // Proveedor (0 = sin proveedor)
                    ProveedorId = ProveedorId.HasValue && ProveedorId.Value > 0 ? ProveedorId : null
                };

                var usuarioId = SessionService.Instance.UsuarioActual?.Id ?? 0;

                if (_productoId.HasValue)
                {
                    await _productoService.ActualizarAsync(_productoId.Value, dto, usuarioId);
                    MessageBox.Show("Producto actualizado correctamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    await _productoService.CrearAsync(dto, usuarioId);
                    MessageBox.Show("Producto creado correctamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                _onGuardado?.Invoke();
                ProductoGuardado = true;
                
                // Cerrar la ventana
                System.Windows.Application.Current.Windows
                    .OfType<ProductoFormView>()
                    .FirstOrDefault(w => w.DataContext == this)
                    ?.Close();
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al guardar: {ex.Message}";
                MessageBox.Show(MensajeError, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool ValidarFormulario()
        {
            if (string.IsNullOrWhiteSpace(Codigo))
            {
                MensajeError = "El código es requerido";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Nombre))
            {
                MensajeError = "El nombre es requerido";
                return false;
            }

            if (!CategoriaId.HasValue)
            {
                MensajeError = "Seleccione una categoría";
                return false;
            }

            if (PrecioVenta <= 0)
            {
                MensajeError = "El precio de venta debe ser mayor a cero";
                return false;
            }

            if (PrecioCompra < 0)
            {
                MensajeError = "El precio de compra no puede ser negativo";
                return false;
            }

            MensajeError = string.Empty;
            return true;
        }
    }

    // Clases auxiliares para los ComboBox
    public class TipoUnidadItem
    {
        public TipoUnidad Valor { get; set; }
        public string Texto { get; set; } = string.Empty;
    }

    public class TipoIVAItem
    {
        public TipoIVA Valor { get; set; }
        public string Texto { get; set; } = string.Empty;
    }

    public class CategoriasPesoItem
    {
        public CategoriaProductoPeso Valor { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Icono { get; set; } = string.Empty;
    }
}