using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Application.DTOs.Sales;
using POS.Application.Interfaces;
using POS.Domain.Enums;
using POS.UI.Models;
using POS.UI.Services;
using POS.UI.Views.Sales;
using System.Collections.ObjectModel;
using System.Windows;
using POS.UI.Helpers;


namespace POS.UI.ViewModels.Sales
{
    public partial class ProcesarPagoViewModel : ObservableObject
    {
        private readonly IVentaService _ventaService;
        private readonly List<ItemCarrito> _items;
        private readonly decimal _totalVenta;
        private readonly int _cajaId;
        private readonly int? _clienteId;
        private readonly Action _onVentaCompletada;

        [ObservableProperty]
        private decimal _total;

        // Montos de cada método de pago
        [ObservableProperty]
        private decimal _montoEfectivo;

        [ObservableProperty]
        private decimal _montoTarjeta;

        [ObservableProperty]
        private decimal _montoNequi;

        [ObservableProperty]
        private decimal _montoDaviplata;

        [ObservableProperty]
        private decimal _montoTransferencia;

        [ObservableProperty]
        private decimal _montoQR;

        // Totales calculados
        [ObservableProperty]
        private decimal _totalPagado;

        [ObservableProperty]
        private decimal _faltaPorPagar;

        [ObservableProperty]
        private decimal _cambio;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _mensajeError = string.Empty;

        public ProcesarPagoViewModel(
            IVentaService ventaService,
            List<ItemCarrito> items,
            decimal totalVenta,
            int cajaId,
            int? clienteId,
            Action onVentaCompletada)
        {
            _ventaService = ventaService;
            _items = items;
            _totalVenta = totalVenta;
            _cajaId = cajaId;
            _clienteId = clienteId;
            _onVentaCompletada = onVentaCompletada;

            Total = totalVenta;
            FaltaPorPagar = totalVenta; // Inicialmente falta todo
        }

        partial void OnMontoEfectivoChanged(decimal value)
        {
            CalcularTotales();
        }

        partial void OnMontoTarjetaChanged(decimal value)
        {
            CalcularTotales();
        }

        partial void OnMontoNequiChanged(decimal value)
        {
            CalcularTotales();
        }

        partial void OnMontoDaviplataChanged(decimal value)
        {
            CalcularTotales();
        }

        partial void OnMontoTransferenciaChanged(decimal value)
        {
            CalcularTotales();
        }

        partial void OnMontoQRChanged(decimal value)
        {
            CalcularTotales();
        }

        private void CalcularTotales()
        {
            // Sumar todos los pagos
            TotalPagado = MontoEfectivo + MontoTarjeta + MontoNequi + 
                          MontoDaviplata + MontoTransferencia + MontoQR;

            // Calcular falta por pagar
            FaltaPorPagar = Total - TotalPagado;
            
            // Asegurar que no sea negativo
            if (FaltaPorPagar < 0)
            {
                FaltaPorPagar = 0;
            }

            // Calcular cambio
            // El cambio es la diferencia si el total pagado es mayor al total de la venta
            Cambio = 0;
            if (TotalPagado > Total)
            {
                Cambio = TotalPagado - Total;
            }
        }

        [RelayCommand]
        private async Task CompletarVentaAsync()
        {
            if (!ValidarPagos())
                return;

            IsLoading = true;
            MensajeError = string.Empty;

            try
            {
                // Construir DTO de venta
                var dto = new CrearVentaDto
                {
                    ClienteId = _clienteId,
                    Items = _items.Select(i => new ItemVentaDto
                    {
                        ProductoId = i.ProductoId,
                        Cantidad = i.Cantidad,
                        DescuentoLinea = i.Descuento
                    }).ToList(),
                    Pagos = new List<PagoVentaDto>()
                };

                // Agregar pagos SOLO si tienen monto > 0
                if (MontoEfectivo > 0)
                {
                    dto.Pagos.Add(new PagoVentaDto
                    {
                        Metodo = MetodoPago.Efectivo,
                        Monto = MontoEfectivo
                    });
                }

                if (MontoTarjeta > 0)
                {
                    dto.Pagos.Add(new PagoVentaDto
                    {
                        Metodo = MetodoPago.Tarjeta,
                        Monto = MontoTarjeta
                    });
                }

                if (MontoNequi > 0)
                {
                    dto.Pagos.Add(new PagoVentaDto
                    {
                        Metodo = MetodoPago.Nequi,
                        Monto = MontoNequi
                    });
                }

                if (MontoDaviplata > 0)
                {
                    dto.Pagos.Add(new PagoVentaDto
                    {
                        Metodo = MetodoPago.Daviplata,
                        Monto = MontoDaviplata
                    });
                }

                if (MontoTransferencia > 0)
                {
                    dto.Pagos.Add(new PagoVentaDto
                    {
                        Metodo = MetodoPago.Transferencia,
                        Monto = MontoTransferencia
                    });
                }

                if (MontoQR > 0)
                {
                    dto.Pagos.Add(new PagoVentaDto
                    {
                        Metodo = MetodoPago.QR,
                        Monto = MontoQR
                    });
                }

                var usuarioId = SessionService.Instance.UsuarioActual?.Id ?? 0;
                var venta = await _ventaService.CrearVentaAsync(dto, usuarioId, _cajaId);

                // Guardar última venta para reimprimir con F7
                POSViewModel.UltimaVenta = venta;

                // Abrir cajón siempre, independiente de si se imprime
                try { new TicketPrinter().AbrirCajonSolo(); } catch { }

                // Preguntar si desea imprimir ticket — NO es la opción por defecto
                var resultadoImpresion = MessageBox.Show(
                    $"Venta: {venta.NumeroVenta}\n" +
                    $"Total: {venta.Total:C}\n" +
                    $"Cambio: {Cambio:C}\n\n" +
                    $"¿Desea imprimir el ticket?",
                    "Venta Exitosa",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question,
                    MessageBoxResult.No);

                if (resultadoImpresion == MessageBoxResult.Yes)
                {
                    try
                    {
                        var printer = new TicketPrinter();
                        printer.ImprimirTicket(venta, abrirCajon: false);
                    }
                    catch (Exception exPrint)
                    {
                        MessageBox.Show(
                            $"Error al imprimir: {exPrint.Message}\n\nLa venta se registró correctamente.",
                            "Error de Impresión",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                    }
                }

                _onVentaCompletada?.Invoke();

                // Cerrar ventana
                System.Windows.Application.Current.Windows
                    .OfType<ProcesarPagoView>()
                    .FirstOrDefault(w => w.DataContext == this)
                    ?.Close();
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al procesar venta: {ex.Message}";
                MessageBox.Show(MensajeError, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool ValidarPagos()
        {
            MensajeError = string.Empty;

            // Validar que haya al menos un método de pago con monto > 0
            if (TotalPagado <= 0)
            {
                MensajeError = "Debe ingresar al menos un monto de pago";
                return false;
            }

            // Validar que el total pagado sea al menos igual al total de la venta
            if (TotalPagado < Total)
            {
                MensajeError = $"Falta por pagar: {FaltaPorPagar:C}";
                return false;
            }

            return true;
        }
    }
}