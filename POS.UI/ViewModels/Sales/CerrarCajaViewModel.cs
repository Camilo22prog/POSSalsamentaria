using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Application.DTOs.Sales;
using POS.Application.Interfaces;
using POS.UI.Helpers;
using POS.UI.Models;
using POS.UI.Services;
using System.Collections.ObjectModel;
using System.Windows;


namespace POS.UI.ViewModels.Sales
{
    public partial class CerrarCajaViewModel : ObservableObject
    {
        private readonly ICajaService _cajaService;
        private readonly int _cajaId;

        [ObservableProperty]
        private ObservableCollection<DenominacionItem> _monedas = new();

        [ObservableProperty]
        private ObservableCollection<DenominacionItem> _billetes = new();

        [ObservableProperty]
        private decimal _totalMonedasContado;

        [ObservableProperty]
        private decimal _totalBilletesContado;

        [ObservableProperty]
        private decimal _totalEfectivoContado;

        // Totales del sistema
        [ObservableProperty]
        private decimal _montoInicial;

        [ObservableProperty]
        private decimal _totalEfectivoSistema;

        [ObservableProperty]
        private decimal _totalVentasEfectivo;

        [ObservableProperty]
        private decimal _totalRetiros;

        [ObservableProperty]
        private decimal _efectivoEsperado;

        [ObservableProperty]
        private decimal _diferenciaEfectivo;

        [ObservableProperty]
        private bool _hayFaltante;

        [ObservableProperty]
        private bool _haySobrante;

        // Otros métodos de pago
        [ObservableProperty]
        private decimal _totalTarjetaSistema;

        [ObservableProperty]
        private decimal _totalNequiSistema;

        [ObservableProperty]
        private decimal _totalDaviplataSistema;

        [ObservableProperty]
        private decimal _totalTransferenciaSistema;

        [ObservableProperty]
        private decimal _totalQRSistema;

        [ObservableProperty]
        private decimal _totalVentas;

        [ObservableProperty]
        private int _cantidadVentas;

        [ObservableProperty]
        private bool _isLoading;

        public CerrarCajaViewModel(ICajaService cajaService, int cajaId)
        {
            _cajaService = cajaService;
            _cajaId = cajaId;
            InicializarDenominaciones();
            _ = CargarDatosAsync();
        }

        private void InicializarDenominaciones()
        {
            // Monedas
            Monedas.Add(new DenominacionItem { Nombre = "$50", Valor = 50, Cantidad = 0 });
            Monedas.Add(new DenominacionItem { Nombre = "$100", Valor = 100, Cantidad = 0 });
            Monedas.Add(new DenominacionItem { Nombre = "$200", Valor = 200, Cantidad = 0 });
            Monedas.Add(new DenominacionItem { Nombre = "$500", Valor = 500, Cantidad = 0 });
            Monedas.Add(new DenominacionItem { Nombre = "$1,000", Valor = 1000, Cantidad = 0 });
            

            // Billetes
            Billetes.Add(new DenominacionItem { Nombre = "$2,000", Valor = 2000, Cantidad = 0 });
            Billetes.Add(new DenominacionItem { Nombre = "$5,000", Valor = 5000, Cantidad = 0 });
            Billetes.Add(new DenominacionItem { Nombre = "$10,000", Valor = 10000, Cantidad = 0 });
            Billetes.Add(new DenominacionItem { Nombre = "$20,000", Valor = 20000, Cantidad = 0 });
            Billetes.Add(new DenominacionItem { Nombre = "$50,000", Valor = 50000, Cantidad = 0 });
            Billetes.Add(new DenominacionItem { Nombre = "$100,000", Valor = 100000, Cantidad = 0 });

            // Suscribirse a cambios
            foreach (var moneda in Monedas)
            {
                moneda.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(DenominacionItem.Total))
                        CalcularTotales();
                };
            }

            foreach (var billete in Billetes)
            {
                billete.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(DenominacionItem.Total))
                        CalcularTotales();
                };
            }
        }

        private async Task CargarDatosAsync()
        {
            IsLoading = true;

            try
            {
                var resumen = await _cajaService.ObtenerResumenCajaAsync(_cajaId);
                var retiros = await _cajaService.ObtenerRetirosCajaAsync(_cajaId);

                MontoInicial = resumen.MontoInicial;
                TotalVentasEfectivo = resumen.TotalEfectivo;
                TotalRetiros = retiros.Sum(r => r.Monto);
                
                EfectivoEsperado = MontoInicial + TotalVentasEfectivo - TotalRetiros;

                TotalTarjetaSistema = resumen.TotalTarjeta;
                TotalNequiSistema = resumen.TotalNequi;
                TotalDaviplataSistema = resumen.TotalDaviplata;
                TotalTransferenciaSistema = resumen.TotalTransferencia;
                TotalQRSistema = resumen.TotalQR;

                TotalVentas = resumen.TotalEfectivo + resumen.TotalTarjeta + resumen.TotalNequi +
                             resumen.TotalDaviplata + resumen.TotalTransferencia + resumen.TotalQR;
                
                CantidadVentas = resumen.CantidadVentas;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al cargar datos: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void CalcularTotales()
        {
            TotalMonedasContado = Monedas.Sum(m => m.Total);
            TotalBilletesContado = Billetes.Sum(b => b.Total);
            TotalEfectivoContado = TotalMonedasContado + TotalBilletesContado;

            DiferenciaEfectivo = TotalEfectivoContado - EfectivoEsperado;
            HayFaltante = DiferenciaEfectivo < 0;
            HaySobrante = DiferenciaEfectivo > 0;
        }

        [RelayCommand]
        private async Task CerrarCajaAsync()
        {
            if (TotalEfectivoContado <= 0)
            {
                MessageBox.Show(
                    "Debe contar el efectivo e ingresar las denominaciones.",
                    "Validación",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // Confirmar si hay diferencia
            if (DiferenciaEfectivo != 0)
            {
                var mensaje = DiferenciaEfectivo < 0
                    ? $"⚠️ FALTANTE detectado: {Math.Abs(DiferenciaEfectivo):C}\n\n¿Está seguro de cerrar la caja con este faltante?"
                    : $"⚠️ SOBRANTE detectado: {DiferenciaEfectivo:C}\n\n¿Está seguro de cerrar la caja con este sobrante?";

                var result = MessageBox.Show(
                    mensaje,
                    "Confirmar Cierre",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes)
                    return;
            }

            IsLoading = true;

            try
            {
                var dto = new CerrarCajaConArqueoDto
                {
                    Monedas = Monedas
                        .Where(m => m.Cantidad > 0)
                        .Select(m => new DetalleDenominacionDto
                        {
                            Valor = m.Valor,
                            Cantidad = m.Cantidad,
                            Total = m.Total
                        }).ToList(),
                    Billetes = Billetes
                        .Where(b => b.Cantidad > 0)
                        .Select(b => new DetalleDenominacionDto
                        {
                            Valor = b.Valor,
                            Cantidad = b.Cantidad,
                            Total = b.Total
                        }).ToList(),
                    EfectivoContado = TotalEfectivoContado,
                    MontoTarjetaContado = TotalTarjetaSistema,
                    MontoNequiContado = TotalNequiSistema,
                    MontoDaviplataContado = TotalDaviplataSistema,
                    MontoTransferenciaContado = TotalTransferenciaSistema,
                    MontoQRContado = TotalQRSistema
                };

                var caja = await _cajaService.CerrarCajaConArqueoAsync(_cajaId, dto);

                // Preguntar si desea imprimir
                var resultadoImpresion = MessageBox.Show(
                    $"✅ Caja cerrada exitosamente\n\n" +
                    $"Total Ventas: {TotalVentas:C}\n" +
                    $"Efectivo Esperado: {EfectivoEsperado:C}\n" +
                    $"Efectivo Contado: {TotalEfectivoContado:C}\n" +
                    $"Diferencia: {DiferenciaEfectivo:C}\n\n" +
                    $"¿Desea imprimir el reporte de cierre?",
                    "Cierre Exitoso",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information);

                if (resultadoImpresion == MessageBoxResult.Yes)
                {
                    try
                    {
                        ImprimirReporteCierre(caja);
                    }
                    catch (Exception exPrint)
                    {
                        MessageBox.Show(
                            $"Error al imprimir: {exPrint.Message}",
                            "Error de Impresión",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                    }
                }

                // Cerrar ventana con resultado exitoso
                var ventana = System.Windows.Application.Current.Windows
                    .OfType<Window>()
                    .FirstOrDefault(w => w.DataContext == this);

                if (ventana != null)
                {
                    ventana.DialogResult = true;  // ✅ AGREGAR ESTA LÍNEA
                    ventana.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al cerrar caja: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void ImprimirReporteCierre(CajaDto caja)
        {
            try
            {
                // Obtener denominaciones de apertura y cierre
                var denominacionesApertura = await _cajaService.ObtenerDenominacionesAperturaAsync(_cajaId);
                
                var denominacionesCierre = new List<DetalleDenominacionDto>();
                denominacionesCierre.AddRange(Monedas.Where(m => m.Cantidad > 0).Select(m => new DetalleDenominacionDto
                {
                    Valor = m.Valor,
                    Cantidad = m.Cantidad,
                    Total = m.Total
                }));
                denominacionesCierre.AddRange(Billetes.Where(b => b.Cantidad > 0).Select(b => new DetalleDenominacionDto
                {
                    Valor = b.Valor,
                    Cantidad = b.Cantidad,
                    Total = b.Total
                }));

                // Obtener retiros
                var retiros = await _cajaService.ObtenerRetirosCajaAsync(_cajaId);

                // Imprimir
                var printer = new CierreCajaPrinter();
                printer.ImprimirCierre(caja, retiros, denominacionesApertura, denominacionesCierre, abrirCajon: false);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al imprimir cierre: {ex.Message}");
            }
        }

        [RelayCommand]
        private void Cancelar()
        {
            System.Windows.Application.Current.Windows
                .OfType<Window>()
                .FirstOrDefault(w => w.DataContext == this)
                ?.Close();
        }
    }
}