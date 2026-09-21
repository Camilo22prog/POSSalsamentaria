using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Application.DTOs.Catalog;
using POS.Application.DTOs.Purchases;
using POS.Application.Interfaces;
using POS.UI.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace POS.UI.ViewModels.Purchases
{
    public partial class NuevaCompraViewModel : ObservableObject
    {
        private readonly ICompraService _compraService;
        private readonly IProductoService _productoService;
        private readonly IProveedorService _proveedorService;

        // ── Encabezado ──────────────────────────────────────────────────────────

        [ObservableProperty]
        private ObservableCollection<ProveedorDto> _proveedores = new();

        [ObservableProperty]
        private int? _proveedorId;

        [ObservableProperty]
        private string _numeroFactura = string.Empty;

        [ObservableProperty]
        private DateTime _fechaCompra = DateTime.Now;

        [ObservableProperty]
        private bool _pagado = true;

        // ── Búsqueda de producto ─────────────────────────────────────────────────

        [ObservableProperty]
        private string _terminoBusqueda = string.Empty;

        [ObservableProperty]
        private ObservableCollection<ProductoDto> _resultadosBusqueda = new();

        [ObservableProperty]
        private ProductoDto? _productoSeleccionado;

        [ObservableProperty]
        private decimal _cantidadAgregar = 1;

        [ObservableProperty]
        private decimal _precioSinIvaAgregar;

        [ObservableProperty]
        private decimal _precioConIvaAgregar;

        [ObservableProperty]
        private decimal _subtotalAgregar;

        [ObservableProperty]
        private decimal _ivaPorcentajeAgregar;

        [ObservableProperty]
        private decimal _ivaValorAgregar;

        [ObservableProperty]
        private decimal _descuentoPorcentajeAgregar;

        [ObservableProperty]
        private decimal _descuentoValorAgregar;

        [ObservableProperty]
        private decimal _ibuaPorcentajeAgregar;

        [ObservableProperty]
        private decimal _ibuaValorAgregar;

        [ObservableProperty]
        private decimal _icuiPorcentajeAgregar;

        [ObservableProperty]
        private decimal _icuiValorAgregar;

        [ObservableProperty]
        private decimal _costoUnitarioLiquidadoAgregar;

        [ObservableProperty]
        private decimal _porcentajeGananciaAgregar;

        [ObservableProperty]
        private decimal _precioVentaCalculadoAgregar;

        [ObservableProperty]
        private DateTime? _fechaVencimientoAgregar;
        
        [ObservableProperty]
        private string? _loteCodigoAgregar = string.Empty;

        private bool _isRecalculatingAgregar = false;

        // ── Items de la compra ────────────────────────────────────────────────────

        [ObservableProperty]
        private ObservableCollection<ItemCompraViewModel> _items = new();

        // ── Totales ───────────────────────────────────────────────────────────────

        [ObservableProperty]
        private decimal _totalCompra;

        // ── Estado UI ─────────────────────────────────────────────────────────────

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _mensajeError = string.Empty;

        public bool CompraRegistrada { get; private set; }

        public NuevaCompraViewModel(
            ICompraService compraService,
            IProductoService productoService,
            IProveedorService proveedorService)
        {
            _compraService = compraService;
            _productoService = productoService;
            _proveedorService = proveedorService;
            
            Items.CollectionChanged += Items_CollectionChanged;
            _ = CargarDatosAsync();
        }

        private async Task CargarDatosAsync()
        {
            try
            {
                var provs = await _proveedorService.ObtenerActivosAsync();
                Proveedores.Clear();
                foreach (var p in provs) Proveedores.Add(p);
                if (Proveedores.Any()) ProveedorId = Proveedores.First().Id;
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al cargar proveedores: {ex.Message}";
            }
        }

        private void Items_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (ItemCompraViewModel item in e.NewItems)
                {
                    item.PropertyChanged += Item_PropertyChanged;
                }
            }

            if (e.OldItems != null)
            {
                foreach (ItemCompraViewModel item in e.OldItems)
                {
                    item.PropertyChanged -= Item_PropertyChanged;
                }
            }
        }

        private void Item_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ItemCompraViewModel.Subtotal))
            {
                RecalcularTotal();
            }
        }

        [RelayCommand]
        private async Task BuscarProductoAsync()
        {
            if (string.IsNullOrWhiteSpace(TerminoBusqueda)) return;

            try
            {
                var lista = await _productoService.BuscarAsync(TerminoBusqueda);
                ResultadosBusqueda.Clear();
                foreach (var p in lista) ResultadosBusqueda.Add(p);

                if (!ResultadosBusqueda.Any())
                    MensajeError = "No se encontraron productos.";
                else
                    MensajeError = string.Empty;
            }
            catch (Exception ex)
            {
                MensajeError = ex.Message;
            }
        }

        partial void OnProductoSeleccionadoChanged(ProductoDto? value)
        {
            if (value == null) return;
            
            _isRecalculatingAgregar = true;
            try
            {
                PrecioSinIvaAgregar = value.PrecioCompra;
                IvaPorcentajeAgregar = value.PorcentajeIVA;
                PrecioConIvaAgregar = Math.Round(value.PrecioCompra * (1 + value.PorcentajeIVA / 100), 2);
                
                IbuaPorcentajeAgregar = 0;
                IcuiPorcentajeAgregar = 0;
                
                CantidadAgregar = 1;
                DescuentoPorcentajeAgregar = 0;
                DescuentoValorAgregar = 0;
                LoteCodigoAgregar = string.Empty;
                
                FechaVencimientoAgregar = value.ControlaVencimiento
                    ? DateTime.Today.AddDays(30)
                    : null;
            }
            finally
            {
                _isRecalculatingAgregar = false;
            }
            
            // Calcula costos netos y valor IVA
            RecalcularLiquidacionAgregar();
            
            // Asigna el precio de venta histórico para que calcule el % de ganancia real que tiene el producto
            PrecioVentaCalculadoAgregar = value.PrecioVenta;
            
            MensajeError = string.Empty;
        }

        partial void OnCantidadAgregarChanged(decimal value) => RecalcularLiquidacionAgregar();

        partial void OnPrecioSinIvaAgregarChanged(decimal value)
        {
            if (_isRecalculatingAgregar) return;
            _isRecalculatingAgregar = true;
            try
            {
                PrecioConIvaAgregar = Math.Round(value * (1 + IvaPorcentajeAgregar / 100), 2);
                RecalcularLiquidacionAgregar();
            }
            finally { _isRecalculatingAgregar = false; }
        }

        partial void OnPrecioConIvaAgregarChanged(decimal value)
        {
            if (_isRecalculatingAgregar) return;
            _isRecalculatingAgregar = true;
            try
            {
                PrecioSinIvaAgregar = Math.Round(value / (1 + IvaPorcentajeAgregar / 100), 2);
                RecalcularLiquidacionAgregar();
            }
            finally { _isRecalculatingAgregar = false; }
        }

        partial void OnIvaPorcentajeAgregarChanged(decimal value)
        {
            if (_isRecalculatingAgregar) return;
            _isRecalculatingAgregar = true;
            try
            {
                PrecioConIvaAgregar = Math.Round(PrecioSinIvaAgregar * (1 + value / 100), 2);
                RecalcularLiquidacionAgregar();
            }
            finally { _isRecalculatingAgregar = false; }
        }

        partial void OnDescuentoPorcentajeAgregarChanged(decimal value)
        {
            if (_isRecalculatingAgregar) return;
            _isRecalculatingAgregar = true;
            try
            {
                DescuentoValorAgregar = Math.Round(PrecioSinIvaAgregar * (value / 100), 2);
                RecalcularLiquidacionAgregar();
            }
            finally { _isRecalculatingAgregar = false; }
        }

        partial void OnDescuentoValorAgregarChanged(decimal value)
        {
            if (_isRecalculatingAgregar) return;
            _isRecalculatingAgregar = true;
            try
            {
                DescuentoPorcentajeAgregar = PrecioSinIvaAgregar > 0 ? Math.Round((value / PrecioSinIvaAgregar) * 100, 2) : 0;
                RecalcularLiquidacionAgregar();
            }
            finally { _isRecalculatingAgregar = false; }
        }

        partial void OnIbuaPorcentajeAgregarChanged(decimal value) => RecalcularLiquidacionAgregar();
        partial void OnIbuaValorAgregarChanged(decimal value) => RecalcularLiquidacionAgregar();
        partial void OnIcuiPorcentajeAgregarChanged(decimal value) => RecalcularLiquidacionAgregar();

        partial void OnPorcentajeGananciaAgregarChanged(decimal value)
        {
            if (_isRecalculatingAgregar) return;
            _isRecalculatingAgregar = true;
            try
            {
                PrecioVentaCalculadoAgregar = Math.Round(CostoUnitarioLiquidadoAgregar * (1 + value / 100), 0, MidpointRounding.AwayFromZero);
            }
            finally { _isRecalculatingAgregar = false; }
        }

        partial void OnSubtotalAgregarChanged(decimal value)
        {
            if (_isRecalculatingAgregar) return;
            _isRecalculatingAgregar = true;
            try
            {
                if (CantidadAgregar > 0)
                {
                    var precioConIva = value / CantidadAgregar;
                    PrecioConIvaAgregar = Math.Round(precioConIva, 2);
                    PrecioSinIvaAgregar = Math.Round(precioConIva / (1 + IvaPorcentajeAgregar / 100), 2);
                    RecalcularLiquidacionAgregar();
                }
            }
            finally { _isRecalculatingAgregar = false; }
        }

        partial void OnPrecioVentaCalculadoAgregarChanged(decimal value)
        {
            if (_isRecalculatingAgregar) return;
            _isRecalculatingAgregar = true;
            try
            {
                PorcentajeGananciaAgregar = CostoUnitarioLiquidadoAgregar > 0 ? Math.Round(((value / CostoUnitarioLiquidadoAgregar) - 1) * 100, 2) : 0;
            }
            finally { _isRecalculatingAgregar = false; }
        }

        public void RecalcularLiquidacionAgregar()
        {
            var baseNeto = PrecioSinIvaAgregar - DescuentoValorAgregar;
            if (baseNeto < 0) baseNeto = 0;

            IvaValorAgregar = Math.Round(baseNeto * (IvaPorcentajeAgregar / 100), 2);
            IcuiValorAgregar = Math.Round(baseNeto * (IcuiPorcentajeAgregar / 100), 2);
            IbuaValorAgregar = Math.Round(baseNeto * (IbuaPorcentajeAgregar / 100), 2);

            CostoUnitarioLiquidadoAgregar = baseNeto + IvaValorAgregar + IcuiValorAgregar + IbuaValorAgregar;

            if (!_isRecalculatingAgregar)
            {
                _isRecalculatingAgregar = true;
                try
                {
                    SubtotalAgregar = Math.Round((baseNeto + IvaValorAgregar) * CantidadAgregar, 2);
                    PrecioVentaCalculadoAgregar = Math.Round(CostoUnitarioLiquidadoAgregar * (1 + PorcentajeGananciaAgregar / 100), 0, MidpointRounding.AwayFromZero);
                }
                finally { _isRecalculatingAgregar = false; }
            }
        }

        [RelayCommand]
        private void AgregarItem()
        {
            if (ProductoSeleccionado == null)
            {
                MensajeError = "Seleccione un producto de la búsqueda.";
                return;
            }
            if (CantidadAgregar <= 0)
            {
                MensajeError = "La cantidad debe ser mayor a cero.";
                return;
            }
            if (PrecioSinIvaAgregar < 0)
            {
                MensajeError = "El precio sin IVA no puede ser negativo.";
                return;
            }

            var existente = Items.FirstOrDefault(i => i.ProductoId == ProductoSeleccionado.Id);
            if (existente != null)
            {
                existente.Cantidad = CantidadAgregar; // Usar el del formulario, no sumar, para permitir correcciones directas
                existente.ActualizarValores(
                    PrecioSinIvaAgregar, PrecioConIvaAgregar, IvaPorcentajeAgregar, IvaValorAgregar,
                    DescuentoPorcentajeAgregar, DescuentoValorAgregar, IbuaPorcentajeAgregar, IbuaValorAgregar,
                    IcuiPorcentajeAgregar, IcuiValorAgregar, CostoUnitarioLiquidadoAgregar, PorcentajeGananciaAgregar,
                    PrecioVentaCalculadoAgregar, FechaVencimientoAgregar, LoteCodigoAgregar);
            }
            else
            {
                var nuevoItem = new ItemCompraViewModel
                {
                    ProductoId = ProductoSeleccionado.Id,
                    ProductoCodigo = ProductoSeleccionado.Codigo,
                    ProductoNombre = ProductoSeleccionado.Nombre,
                    Cantidad = CantidadAgregar,
                    ActualizarCatalogo = true,
                    PrecioVentaActual = ProductoSeleccionado.PrecioVenta,
                    CostoAnterior = ProductoSeleccionado.PrecioCompra
                };
                
                nuevoItem.ActualizarValores(
                    PrecioSinIvaAgregar, PrecioConIvaAgregar, IvaPorcentajeAgregar, IvaValorAgregar,
                    DescuentoPorcentajeAgregar, DescuentoValorAgregar, IbuaPorcentajeAgregar, IbuaValorAgregar,
                    IcuiPorcentajeAgregar, IcuiValorAgregar, CostoUnitarioLiquidadoAgregar, PorcentajeGananciaAgregar,
                    PrecioVentaCalculadoAgregar, FechaVencimientoAgregar, LoteCodigoAgregar);
                
                Items.Add(nuevoItem);
            }

            RecalcularTotal();
            LimpiarBusqueda();
            MensajeError = string.Empty;
        }

        [RelayCommand]
        private void QuitarItem(ItemCompraViewModel? item)
        {
            if (item == null) return;
            Items.Remove(item);
            RecalcularTotal();
        }

        [RelayCommand]
        private async Task RegistrarCompraAsync()
        {
            if (!ProveedorId.HasValue || ProveedorId == 0)
            {
                MensajeError = "Seleccione un proveedor.";
                return;
            }
            if (!Items.Any())
            {
                MensajeError = "Agregue al menos un producto.";
                return;
            }

            IsLoading = true;
            MensajeError = string.Empty;
            try
            {
                var dto = new CrearCompraDto
                {
                    NumeroFactura = string.IsNullOrWhiteSpace(NumeroFactura) ? null : NumeroFactura.Trim(),
                    ProveedorId = ProveedorId!.Value,
                    FechaCompra = FechaCompra,
                    Pagado = Pagado,
                    Detalles = Items.Select(i => new CrearDetalleCompraDto
                    {
                        ProductoId = i.ProductoId,
                        Cantidad = i.Cantidad,
                        PrecioUnitario = i.PrecioSinIva,
                        Descuento = i.DescuentoValor * i.Cantidad,
                        FechaVencimiento = i.FechaVencimiento,
                        PrecioSinIva = i.PrecioSinIva,
                        PrecioConIva = i.PrecioConIva,
                        IvaPorcentaje = i.IvaPorcentaje,
                        IvaValor = i.IvaValor,
                        DescuentoPorcentaje = i.DescuentoPorcentaje,
                        DescuentoValor = i.DescuentoValor,
                        IbuaPorcentaje = i.IbuaPorcentaje,
                        IbuaValor = i.IbuaValor,
                        IcuiPorcentaje = i.IcuiPorcentaje,
                        IcuiValor = i.IcuiValor,
                        CostoUnitarioLiquidado = i.CostoUnitarioLiquidado,
                        PorcentajeGanancia = i.PorcentajeGanancia,
                        PrecioVentaCalculado = i.PrecioVentaCalculado,
                        LoteCodigo = i.LoteCodigo,
                        ActualizarCatalogo = i.ActualizarCatalogo
                    }).ToList()
                };

                var usuarioId = SessionService.Instance.UsuarioActual?.Id ?? 0;
                var resultado = await _compraService.RegistrarCompraAsync(dto, usuarioId);

                MessageBox.Show(
                    $"Compra registrada exitosamente.\nNúmero: {resultado.NumeroCompra}\nTotal: {resultado.Total:C0}",
                    "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                CompraRegistrada = true;

                var window = System.Windows.Application.Current.Windows
                    .OfType<Views.Purchases.NuevaCompraView>()
                    .FirstOrDefault(w => w.DataContext == this);

                if (window != null)
                {
                    window.DialogResult = true;
                }
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al registrar: {ex.Message}";
                MessageBox.Show(MensajeError, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally { IsLoading = false; }
        }

        [RelayCommand]
        private void Cancelar()
        {
            System.Windows.Application.Current.Windows
                .OfType<Views.Purchases.NuevaCompraView>()
                .FirstOrDefault(w => w.DataContext == this)
                ?.Close();
        }

        private void RecalcularTotal()
        {
            TotalCompra = Items.Sum(i => i.Subtotal);
        }

        private void LimpiarBusqueda()
        {
            TerminoBusqueda = string.Empty;
            ResultadosBusqueda.Clear();
            ProductoSeleccionado = null;
            CantidadAgregar = 1;
            PrecioSinIvaAgregar = 0;
            PrecioConIvaAgregar = 0;
            SubtotalAgregar = 0;
            IvaPorcentajeAgregar = 0;
            IvaValorAgregar = 0;
            DescuentoPorcentajeAgregar = 0;
            DescuentoValorAgregar = 0;
            IbuaPorcentajeAgregar = 0;
            IbuaValorAgregar = 0;
            IcuiPorcentajeAgregar = 0;
            IcuiValorAgregar = 0;
            CostoUnitarioLiquidadoAgregar = 0;
            PorcentajeGananciaAgregar = 0;
            PrecioVentaCalculadoAgregar = 0;
            FechaVencimientoAgregar = null;
            LoteCodigoAgregar = string.Empty;
        }
    }

    public partial class ItemCompraViewModel : ObservableObject
    {
        public int ProductoId { get; set; }
        public string ProductoCodigo { get; set; } = string.Empty;
        public string ProductoNombre { get; set; } = string.Empty;

        [ObservableProperty]
        private decimal _cantidad;

        [ObservableProperty]
        private decimal _precioUnitario;

        [ObservableProperty]
        private decimal _descuento;

        [ObservableProperty]
        private decimal _subtotal;

        [ObservableProperty]
        private DateTime? _fechaVencimiento;

        // Nuevos campos de liquidación
        [ObservableProperty]
        private decimal _precioSinIva;

        [ObservableProperty]
        private decimal _precioConIva;

        [ObservableProperty]
        private decimal _ivaPorcentaje;

        [ObservableProperty]
        private decimal _ivaValor;

        [ObservableProperty]
        private decimal _descuentoPorcentaje;

        [ObservableProperty]
        private decimal _descuentoValor;

        [ObservableProperty]
        private decimal _ibuaPorcentaje;

        [ObservableProperty]
        private decimal _ibuaValor;

        [ObservableProperty]
        private decimal _icuiPorcentaje;

        [ObservableProperty]
        private decimal _icuiValor;

        [ObservableProperty]
        private decimal _costoUnitarioLiquidado;

        [ObservableProperty]
        private decimal _porcentajeGanancia;

        [ObservableProperty]
        private decimal _precioVentaCalculado;

        [ObservableProperty]
        private string? _loteCodigo = string.Empty;

        [ObservableProperty]
        private bool _actualizarCatalogo = true;

        [ObservableProperty]
        private decimal _precioVentaActual;

        [ObservableProperty]
        private decimal _costoAnterior;

        private bool _isRecalculating = false;

        public void ActualizarValores(
            decimal pSinIva, decimal pConIva, decimal ivaP, decimal ivaV,
            decimal descP, decimal descV, decimal ibuaP, decimal ibuaV,
            decimal icuiP, decimal icuiV, decimal costoLiq, decimal porcGan,
            decimal pVenta, DateTime? fechaVenc, string? lote)
        {
            _isRecalculating = true;
            try
            {
                PrecioSinIva = pSinIva;
                PrecioConIva = pConIva;
                IvaPorcentaje = ivaP;
                IvaValor = ivaV;
                DescuentoPorcentaje = descP;
                DescuentoValor = descV;
                IbuaPorcentaje = ibuaP;
                IbuaValor = ibuaV;
                IcuiPorcentaje = icuiP;
                IcuiValor = icuiV;
                CostoUnitarioLiquidado = costoLiq;
                PorcentajeGanancia = porcGan;
                PrecioVentaCalculado = pVenta;
                FechaVencimiento = fechaVenc;
                if (!string.IsNullOrWhiteSpace(lote))
                {
                    LoteCodigo = lote;
                }
            }
            finally
            {
                _isRecalculating = false;
                RecalcularLiquidacion();
            }
        }

        partial void OnCantidadChanged(decimal value) => RecalcularLiquidacion();

        partial void OnPrecioSinIvaChanged(decimal value)
        {
            if (_isRecalculating) return;
            _isRecalculating = true;
            try
            {
                PrecioConIva = Math.Round(value * (1 + IvaPorcentaje / 100), 2);
                RecalcularLiquidacion();
            }
            finally { _isRecalculating = false; }
        }

        partial void OnPrecioConIvaChanged(decimal value)
        {
            if (_isRecalculating) return;
            _isRecalculating = true;
            try
            {
                PrecioSinIva = Math.Round(value / (1 + IvaPorcentaje / 100), 2);
                RecalcularLiquidacion();
            }
            finally { _isRecalculating = false; }
        }

        partial void OnIvaPorcentajeChanged(decimal value)
        {
            if (_isRecalculating) return;
            _isRecalculating = true;
            try
            {
                PrecioConIva = Math.Round(PrecioSinIva * (1 + value / 100), 2);
                RecalcularLiquidacion();
            }
            finally { _isRecalculating = false; }
        }

        partial void OnDescuentoPorcentajeChanged(decimal value)
        {
            if (_isRecalculating) return;
            _isRecalculating = true;
            try
            {
                DescuentoValor = Math.Round(PrecioSinIva * (value / 100), 2);
                RecalcularLiquidacion();
            }
            finally { _isRecalculating = false; }
        }

        partial void OnDescuentoValorChanged(decimal value)
        {
            if (_isRecalculating) return;
            _isRecalculating = true;
            try
            {
                DescuentoPorcentaje = PrecioSinIva > 0 ? Math.Round((value / PrecioSinIva) * 100, 2) : 0;
                RecalcularLiquidacion();
            }
            finally { _isRecalculating = false; }
        }

        partial void OnIbuaPorcentajeChanged(decimal value) => RecalcularLiquidacion();
        partial void OnIbuaValorChanged(decimal value) => RecalcularLiquidacion();
        partial void OnIcuiPorcentajeChanged(decimal value) => RecalcularLiquidacion();

        partial void OnPorcentajeGananciaChanged(decimal value)
        {
            if (_isRecalculating) return;
            _isRecalculating = true;
            try
            {
                PrecioVentaCalculado = Math.Round(CostoUnitarioLiquidado * (1 + value / 100), 0, MidpointRounding.AwayFromZero);
            }
            finally { _isRecalculating = false; }
        }

        partial void OnPrecioVentaCalculadoChanged(decimal value)
        {
            if (_isRecalculating) return;
            _isRecalculating = true;
            try
            {
                PorcentajeGanancia = CostoUnitarioLiquidado > 0 ? Math.Round(((value / CostoUnitarioLiquidado) - 1) * 100, 2) : 0;
            }
            finally { _isRecalculating = false; }
        }

        public void RecalcularLiquidacion()
        {
            var baseNeto = PrecioSinIva - DescuentoValor;
            if (baseNeto < 0) baseNeto = 0;

            IvaValor = Math.Round(baseNeto * (IvaPorcentaje / 100), 2);
            IcuiValor = Math.Round(baseNeto * (IcuiPorcentaje / 100), 2);
            IbuaValor = Math.Round(baseNeto * (IbuaPorcentaje / 100), 2);

            CostoUnitarioLiquidado = baseNeto + IvaValor + IcuiValor + IbuaValor;
            PrecioUnitario = CostoUnitarioLiquidado; // Para compatibilidad
            Descuento = DescuentoValor * Cantidad;   // Para compatibilidad

            // Subtotal refleja el valor de la factura (Compra) sin impuestos ultraprocesados internos
            Subtotal = Math.Round((baseNeto + IvaValor) * Cantidad, 2);

            if (!_isRecalculating)
            {
                _isRecalculating = true;
                try
                {
                    PrecioVentaCalculado = Math.Round(CostoUnitarioLiquidado * (1 + PorcentajeGanancia / 100), 0, MidpointRounding.AwayFromZero);
                }
                finally { _isRecalculating = false; }
            }
        }
    }
}
