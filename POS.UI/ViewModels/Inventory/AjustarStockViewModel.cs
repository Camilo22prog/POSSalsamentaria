using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Application.DTOs.Inventory;
using POS.Application.Interfaces;
using POS.Domain.Enums;
using POS.UI.Services;
using POS.UI.Views.Inventory;
using System.Collections.ObjectModel;
using System.Windows;

namespace POS.UI.ViewModels.Inventory
{
    public partial class AjustarStockViewModel : ObservableObject
    {
        private readonly IInventarioService _inventarioService;
        private readonly InventarioProductoDto _producto;
        private readonly Action _onGuardado;

        [ObservableProperty]
        private string _productoNombre = string.Empty;

        [ObservableProperty]
        private decimal _stockActual;

        [ObservableProperty]
        private decimal _cantidadNueva;

        [ObservableProperty]
        private TipoMovimientoInventario _tipoMovimiento = TipoMovimientoInventario.Ajuste;

        [ObservableProperty]
        private ObservableCollection<TipoMovimientoItem> _tiposMovimiento = new();

        [ObservableProperty]
        private string _motivo = string.Empty;

        [ObservableProperty]
        private decimal? _costoUnitario;

        // Campos calculados — NO se persisten en BD
        private bool _recalculando = false;

        [ObservableProperty]
        private decimal _precioCompra;

        [ObservableProperty]
        private decimal _precioCompraConIVA;

        [ObservableProperty]
        private decimal _margenGanancia = 30m;

        [ObservableProperty]
        private decimal _precioVentaSugerido;

        [ObservableProperty]
        private decimal _precioVenta;

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private string _mensajeError = string.Empty;

        public AjustarStockViewModel(
            IInventarioService inventarioService,
            InventarioProductoDto producto,
            Action onGuardado)
        {
            _inventarioService = inventarioService;
            _producto = producto;
            _onGuardado = onGuardado;

            ProductoNombre = $"{producto.Codigo} - {producto.Nombre}";
            StockActual = producto.StockActual;
            CantidadNueva = producto.StockActual;
            CostoUnitario = producto.CostoPromedio;
            
            // Suprimir cascadas de precios durante la carga
            _recalculando = true;
            PrecioCompra = producto.PrecioCompra;
            PrecioVenta = producto.PrecioVenta;
            _recalculando = false;
            RecalcularTodo();
 
            CargarTiposMovimiento();
        }

        private void CargarTiposMovimiento()
        {
            TiposMovimiento.Clear();
            TiposMovimiento.Add(new TipoMovimientoItem { Valor = TipoMovimientoInventario.Entrada, Texto = "Entrada (aumentar stock)" });
            TiposMovimiento.Add(new TipoMovimientoItem { Valor = TipoMovimientoInventario.Salida, Texto = "Salida (reducir stock)" });
            TiposMovimiento.Add(new TipoMovimientoItem { Valor = TipoMovimientoInventario.Ajuste, Texto = "Ajuste (corrección)" });
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
                var dto = new AjustarStockDto
                {
                    ProductoId = _producto.ProductoId,
                    CantidadNueva = CantidadNueva,
                    Tipo = TipoMovimiento,
                    Motivo = string.IsNullOrWhiteSpace(Motivo) ? null : Motivo.Trim(),
                    CostoUnitario = CostoUnitario,
                    PrecioCompraNuevo = PrecioCompra,
                    PrecioVentaNuevo = PrecioVenta
                };

                var usuarioId = SessionService.Instance.UsuarioActual?.Id ?? 0;
                await _inventarioService.AjustarStockAsync(dto, usuarioId);

                MessageBox.Show("Stock ajustado correctamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                _onGuardado?.Invoke();

                System.Windows.Application.Current.Windows
                    .OfType<AjustarStockView>()
                    .FirstOrDefault(w => w.DataContext == this)
                    ?.Close();
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al ajustar stock: {ex.Message}";
                MessageBox.Show(MensajeError, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private decimal PorcentajeIVADecimal => (decimal)_producto.TipoIVA / 100m;

        private void RecalcularTodo()
        {
            _recalculando = true;
            try
            {
                PrecioCompraConIVA = Math.Round(PrecioCompra * (1 + PorcentajeIVADecimal), 2);
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

        partial void OnMargenGananciaChanged(decimal value)
        {
            if (_recalculando) return;
            _recalculando = true;
            try { RecalcularSugerido(); }
            finally { _recalculando = false; }
        }

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

        private bool ValidarFormulario()
        {
            if (CantidadNueva < 0)
            {
                MensajeError = "La cantidad no puede ser negativa";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Motivo))
            {
                MensajeError = "Debe ingresar un motivo para el ajuste";
                return false;
            }

            MensajeError = string.Empty;
            return true;
        }
    }

    public class TipoMovimientoItem
    {
        public TipoMovimientoInventario Valor { get; set; }
        public string Texto { get; set; } = string.Empty;
    }
}