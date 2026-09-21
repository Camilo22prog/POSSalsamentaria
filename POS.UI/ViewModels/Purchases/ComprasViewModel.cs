using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using POS.Application.DTOs.Purchases;
using POS.Application.Interfaces;
using POS.UI.Services;
using POS.UI.Views.Purchases;
using System.Collections.ObjectModel;
using System.Windows;

namespace POS.UI.ViewModels.Purchases
{
    public partial class ComprasViewModel : ObservableObject
    {
        private readonly ICompraService _compraService;

        [ObservableProperty]
        private ObservableCollection<CompraDto> _compras = new();

        [ObservableProperty]
        private CompraDto? _compraSeleccionada;

        [ObservableProperty]
        private DateTime _fechaInicio = DateTime.Today;

        [ObservableProperty]
        private DateTime _fechaFin = DateTime.Today;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _totalCompras = "$0";

        [ObservableProperty]
        private int _cantidadCompras;

        public ComprasViewModel(ICompraService compraService)
        {
            _compraService = compraService;
            _ = CargarAsync();
        }

        [RelayCommand]
        private async Task CargarAsync()
        {
            IsLoading = true;
            try
            {
                var lista = await _compraService.ObtenerPorRangoAsync(FechaInicio, FechaFin);
                Compras = new ObservableCollection<CompraDto>(lista);
                CantidadCompras = Compras.Count;
                TotalCompras = Compras.Where(c => !c.Anulada).Sum(c => c.Total).ToString("C0");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar compras: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally { IsLoading = false; }
        }

        [RelayCommand]
        private void NuevaCompra()
        {
            var ventana = App.Services.GetRequiredService<NuevaCompraView>();
            var owner = System.Windows.Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsVisible && w != ventana);
            if (owner != null) ventana.Owner = owner;
            var resultado = ventana.ShowDialog();
            if (resultado == true)
                _ = CargarAsync();
        }

        [RelayCommand]
        private async Task VerDetalle()
        {
            if (CompraSeleccionada == null)
            {
                MessageBox.Show("Seleccione una compra para ver su detalle.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            IsLoading = true;
            try
            {
                var detalle = await _compraService.ObtenerPorIdAsync(CompraSeleccionada.Id);
                if (detalle == null) return;

                var lineas = string.Join("\n\n", detalle.Detalles.Select(d =>
                    $"  • {d.ProductoNombre} ({d.ProductoCodigo}):\n" +
                    $"    Cant: {d.Cantidad} | Costo s/IVA: {d.PrecioSinIva:C0} | IVA: {d.IvaPorcentaje}% ({d.IvaValor:C0})\n" +
                    $"    Desc: {d.DescuentoPorcentaje}% ({d.DescuentoValor:C0}) | ICUI: {d.IcuiPorcentaje}% | IBUA: {d.IbuaPorcentaje}%\n" +
                    $"    Costo Liq: {d.CostoUnitarioLiquidado:C0} | Margen: {d.PorcentajeGanancia:F1}% | P. Venta: {d.PrecioVentaCalculado:C0}\n" +
                    $"    Lote: {d.LoteCodigo ?? "N/A"} | Subtotal: {d.Subtotal:C0}" +
                    (d.FechaVencimiento.HasValue ? $" | Vence: {d.FechaVencimiento:dd/MM/yyyy}" : "")));

                MessageBox.Show(
                    $"Compra: {detalle.NumeroCompra}\n" +
                    $"Proveedor: {detalle.ProveedorNombre}\n" +
                    $"Fecha: {detalle.FechaCompra:dd/MM/yyyy}\n" +
                    $"Factura: {detalle.NumeroFactura ?? "N/A"}\n" +
                    $"Estado: {(detalle.Anulada ? "ANULADA" : detalle.Pagado ? "Pagada" : "Pendiente pago")}\n\n" +
                    $"Productos:\n{lineas}\n\n" +
                    $"TOTAL: {detalle.Total:C0}",
                    "Detalle de Compra", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally { IsLoading = false; }
        }

        [RelayCommand]
        private async Task AnularCompraAsync()
        {
            if (CompraSeleccionada == null)
            {
                MessageBox.Show("Seleccione una compra para anular.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (CompraSeleccionada.Anulada)
            {
                MessageBox.Show("Esta compra ya está anulada.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"¿Anular la compra {CompraSeleccionada.NumeroCompra}?\n\nEsto revertirá el stock de todos los productos ingresados.",
                "Confirmar anulación", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            var motivo = Microsoft.VisualBasic.Interaction.InputBox(
                "Ingrese el motivo de anulación:", "Motivo", "Error en factura");

            if (string.IsNullOrWhiteSpace(motivo)) return;

            IsLoading = true;
            try
            {
                var usuarioId = SessionService.Instance.UsuarioActual?.Id ?? 0;
                await _compraService.AnularCompraAsync(CompraSeleccionada.Id, motivo, usuarioId);
                MessageBox.Show("Compra anulada correctamente.", "Éxito",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                await CargarAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al anular: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally { IsLoading = false; }
        }

        [RelayCommand]
        private async Task MarcarPagadaAsync()
        {
            if (CompraSeleccionada == null || CompraSeleccionada.Pagado || CompraSeleccionada.Anulada) return;

            var result = MessageBox.Show(
                $"¿Marcar como pagada la compra {CompraSeleccionada.NumeroCompra}?",
                "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            IsLoading = true;
            try
            {
                await _compraService.MarcarPagadaAsync(CompraSeleccionada.Id);
                await CargarAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally { IsLoading = false; }
        }

        [RelayCommand]
        private void FiltroHoy()
        {
            FechaInicio = DateTime.Today;
            FechaFin = DateTime.Today;
            _ = CargarAsync();
        }

        [RelayCommand]
        private void FiltroEstaSemana()
        {
            FechaInicio = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            FechaFin = DateTime.Today;
            _ = CargarAsync();
        }

        [RelayCommand]
        private void FiltroEsteMes()
        {
            FechaInicio = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            FechaFin = DateTime.Today;
            _ = CargarAsync();
        }
    }
}
