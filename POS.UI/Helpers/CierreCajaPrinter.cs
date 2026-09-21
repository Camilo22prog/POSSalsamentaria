using System.Drawing;
using System.Drawing.Printing;
using POS.Application.DTOs.Sales;
using POS.Domain.Enums;

namespace POS.UI.Helpers
{
    public class CierreCajaPrinter
    {
        private PrintDocument _printDocument;
        private CajaDto? _caja;
        private List<RetiroCajaDto> _retiros;
        private List<DetalleDenominacionDto> _denominacionesApertura;
        private List<DetalleDenominacionDto> _denominacionesCierre;

        public CierreCajaPrinter()
        {
            _printDocument = new PrintDocument();
            _printDocument.PrintPage += PrintDocument_PrintPage;
            
            // Configurar para impresora de 58mm
            var paperSize = new PaperSize("58mm", 228, 3000);
            _printDocument.DefaultPageSettings.PaperSize = paperSize;
            _printDocument.DefaultPageSettings.Margins = new Margins(5, 5, 5, 5);
            
            _retiros = new List<RetiroCajaDto>();
            _denominacionesApertura = new List<DetalleDenominacionDto>();
            _denominacionesCierre = new List<DetalleDenominacionDto>();
        }

        public void ImprimirCierre(
            CajaDto caja, 
            List<RetiroCajaDto> retiros,
            List<DetalleDenominacionDto> denominacionesApertura,
            List<DetalleDenominacionDto> denominacionesCierre,
            bool abrirCajon = false)
        {
            _caja = caja;
            _retiros = retiros;
            _denominacionesApertura = denominacionesApertura;
            _denominacionesCierre = denominacionesCierre;

            try
            {
                // Buscar impresora térmica
                foreach (string printer in PrinterSettings.InstalledPrinters)
                {
                    if (printer.Contains("XP") || printer.Contains("58") || 
                        printer.Contains("Thermal") || printer.Contains("POS"))
                    {
                        _printDocument.PrinterSettings.PrinterName = printer;
                        break;
                    }
                }

                _printDocument.Print();

                if (abrirCajon)
                {
                    AbrirCajon();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al imprimir cierre: {ex.Message}");
            }
        }

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            if (_caja == null || e.Graphics == null) return;

            var fuente = new Font("Courier New", 7, FontStyle.Regular);
            var fuenteBold = new Font("Courier New", 7, FontStyle.Bold);
            var fuenteTitulo = new Font("Courier New", 9, FontStyle.Bold);
            
            float y = 5;
            float lineHeight = 12;
            float x = 5;
            const int ANCHO = 32;

            // === ENCABEZADO ===
            e.Graphics.DrawString(Centrar("CIERRE DE CAJA", ANCHO), fuenteTitulo, Brushes.Black, x, y);
            y += lineHeight + 3;
            e.Graphics.DrawString(Linea('=', ANCHO), fuente, Brushes.Black, x, y);
            y += lineHeight;

            // === DATOS DE LA CAJA ===
            e.Graphics.DrawString($"Caja ID: {_caja.Id}", fuente, Brushes.Black, x, y);
            y += lineHeight;
            e.Graphics.DrawString($"Cajero: {Truncar(_caja.UsuarioNombre, 24)}", fuente, Brushes.Black, x, y);
            y += lineHeight;
            e.Graphics.DrawString($"Apertura: {_caja.FechaApertura:dd/MM/yy HH:mm}", fuente, Brushes.Black, x, y);
            y += lineHeight;
            e.Graphics.DrawString($"Cierre: {_caja.FechaCierre:dd/MM/yy HH:mm}", fuente, Brushes.Black, x, y);
            y += lineHeight;
            
            e.Graphics.DrawString(Linea('-', ANCHO), fuente, Brushes.Black, x, y);
            y += lineHeight;

            // === DENOMINACIONES APERTURA ===
            e.Graphics.DrawString("APERTURA:", fuenteBold, Brushes.Black, x, y);
            y += lineHeight;
            
            var totalApertura = ImprimirDenominaciones(e, ref y, x, fuente, _denominacionesApertura);
            
            e.Graphics.DrawString(FormatLinea("Total Apertura:", $"{totalApertura:C0}", ANCHO), fuenteBold, Brushes.Black, x, y);
            y += lineHeight + 3;

            e.Graphics.DrawString(Linea('-', ANCHO), fuente, Brushes.Black, x, y);
            y += lineHeight;

            // === MOVIMIENTOS DEL DÍA ===
            e.Graphics.DrawString("MOVIMIENTOS:", fuenteBold, Brushes.Black, x, y);
            y += lineHeight;

            e.Graphics.DrawString(FormatLinea("Efectivo ventas:", $"{_caja.TotalEfectivo:C0}", ANCHO), fuente, Brushes.Black, x, y);
            y += lineHeight;
            e.Graphics.DrawString(FormatLinea("Tarjeta:", $"{_caja.TotalTarjeta:C0}", ANCHO), fuente, Brushes.Black, x, y);
            y += lineHeight;
            e.Graphics.DrawString(FormatLinea("Nequi:", $"{_caja.TotalNequi:C0}", ANCHO), fuente, Brushes.Black, x, y);
            y += lineHeight;
            e.Graphics.DrawString(FormatLinea("Daviplata:", $"{_caja.TotalDaviplata:C0}", ANCHO), fuente, Brushes.Black, x, y);
            y += lineHeight;
            e.Graphics.DrawString(FormatLinea("Transferencia:", $"{_caja.TotalTransferencia:C0}", ANCHO), fuente, Brushes.Black, x, y);
            y += lineHeight;
            e.Graphics.DrawString(FormatLinea("QR:", $"{_caja.TotalQR:C0}", ANCHO), fuente, Brushes.Black, x, y);
            y += lineHeight;

            var totalVentas = _caja.TotalEfectivo + _caja.TotalTarjeta + _caja.TotalNequi + 
                             _caja.TotalDaviplata + _caja.TotalTransferencia + _caja.TotalQR;
            
            e.Graphics.DrawString(FormatLinea("TOTAL VENTAS:", $"{totalVentas:C0}", ANCHO), fuenteBold, Brushes.Black, x, y);
            y += lineHeight + 3;

            // === RETIROS ===
            if (_retiros.Any())
            {
                e.Graphics.DrawString(Linea('-', ANCHO), fuente, Brushes.Black, x, y);
                y += lineHeight;
                
                e.Graphics.DrawString("RETIROS:", fuenteBold, Brushes.Black, x, y);
                y += lineHeight;

                foreach (var retiro in _retiros)
                {
                    e.Graphics.DrawString($"{retiro.Fecha:HH:mm} {Truncar(retiro.Motivo, 17)}", fuente, Brushes.Black, x, y);
                    y += lineHeight;
                    e.Graphics.DrawString(FormatLinea("", $"-{retiro.Monto:C0}", ANCHO), fuente, Brushes.Black, x, y);
                    y += lineHeight;
                }

                e.Graphics.DrawString(FormatLinea("Total Retiros:", $"-{_caja.TotalRetiros:C0}", ANCHO), fuenteBold, Brushes.Black, x, y);
                y += lineHeight + 3;
            }

            e.Graphics.DrawString(Linea('=', ANCHO), fuente, Brushes.Black, x, y);
            y += lineHeight;

            // === EFECTIVO ESPERADO ===
            var efectivoEsperado = _caja.MontoInicial + _caja.TotalEfectivo - _caja.TotalRetiros;
            
            e.Graphics.DrawString(FormatLinea("Efectivo Esperado:", $"{efectivoEsperado:C0}", ANCHO), fuenteBold, Brushes.Black, x, y);
            y += lineHeight + 3;

            e.Graphics.DrawString(Linea('-', ANCHO), fuente, Brushes.Black, x, y);
            y += lineHeight;

            // === DENOMINACIONES CIERRE ===
            e.Graphics.DrawString("ARQUEO CIERRE:", fuenteBold, Brushes.Black, x, y);
            y += lineHeight;
            
            var totalCierre = ImprimirDenominaciones(e, ref y, x, fuente, _denominacionesCierre);
            
            e.Graphics.DrawString(FormatLinea("Total Contado:", $"{totalCierre:C0}", ANCHO), fuenteBold, Brushes.Black, x, y);
            y += lineHeight + 3;

            e.Graphics.DrawString(Linea('=', ANCHO), fuente, Brushes.Black, x, y);
            y += lineHeight;

            // === DIFERENCIA ===
            var diferencia = totalCierre - efectivoEsperado;
            
            if (diferencia == 0)
            {
                e.Graphics.DrawString(Centrar("✓ CUADRE EXACTO", ANCHO), fuenteBold, Brushes.Black, x, y);
            }
            else if (diferencia < 0)
            {
                e.Graphics.DrawString(Centrar("⚠ FALTANTE", ANCHO), fuenteBold, Brushes.Black, x, y);
                y += lineHeight;
                e.Graphics.DrawString(FormatLinea("Diferencia:", $"{diferencia:C0}", ANCHO), fuenteBold, Brushes.Black, x, y);
            }
            else
            {
                e.Graphics.DrawString(Centrar("⚠ SOBRANTE", ANCHO), fuenteBold, Brushes.Black, x, y);
                y += lineHeight;
                e.Graphics.DrawString(FormatLinea("Diferencia:", $"+{diferencia:C0}", ANCHO), fuenteBold, Brushes.Black, x, y);
            }
            y += lineHeight + 5;

            e.Graphics.DrawString(Linea('=', ANCHO), fuente, Brushes.Black, x, y);
            y += lineHeight;
            
            // === PIE ===
            e.Graphics.DrawString(Centrar("Firma Cajero:", ANCHO), fuente, Brushes.Black, x, y);
            y += lineHeight + 15;
            e.Graphics.DrawString(Centrar("_________________", ANCHO), fuente, Brushes.Black, x, y);
            y += lineHeight + 5;
            e.Graphics.DrawString(Centrar("Firma Supervisor:", ANCHO), fuente, Brushes.Black, x, y);
            y += lineHeight + 15;
            e.Graphics.DrawString(Centrar("_________________", ANCHO), fuente, Brushes.Black, x, y);

            e.HasMorePages = false;
        }

        private decimal ImprimirDenominaciones(PrintPageEventArgs e, ref float y, float x, Font fuente, List<DetalleDenominacionDto> denominaciones)
        {
            decimal total = 0;
            
            // Agrupar por tipo
            var monedas = denominaciones.Where(d => d.Valor <= 1000).OrderBy(d => d.Valor).ToList();
            var billetes = denominaciones.Where(d => d.Valor > 1000).OrderBy(d => d.Valor).ToList();

            if (monedas.Any())
            {
                e.Graphics?.DrawString(" Monedas:", fuente, Brushes.Black, x, y);
                y += 12;
                
                foreach (var d in monedas)
                {
                    e.Graphics?.DrawString($"  {d.Cantidad}x${d.Valor} = ${d.Total:N0}", fuente, Brushes.Black, x, y);
                    y += 12;
                    total += d.Total;
                }
            }

            if (billetes.Any())
            {
                e.Graphics?.DrawString(" Billetes:", fuente, Brushes.Black, x, y);
                y += 12;
                
                foreach (var d in billetes)
                {
                    e.Graphics?.DrawString($"  {d.Cantidad}x${d.Valor:N0} = ${d.Total:N0}", fuente, Brushes.Black, x, y);
                    y += 12;
                    total += d.Total;
                }
            }

            return total;
        }

        private void AbrirCajon()
        {
            try
            {
                byte[] comando = { 0x1B, 0x70, 0x00, 0x19, 0x19 };
                RawPrinterHelper.SendBytesToPrinter(_printDocument.PrinterSettings.PrinterName, comando);
            }
            catch { }
        }

        private string Centrar(string texto, int ancho)
        {
            if (texto.Length >= ancho) return texto;
            int espacios = (ancho - texto.Length) / 2;
            return new string(' ', espacios) + texto;
        }

        private string Linea(char caracter, int ancho)
        {
            return new string(caracter, ancho);
        }

        private string Truncar(string texto, int maxLength)
        {
            return texto.Length > maxLength ? texto.Substring(0, maxLength) : texto;
        }

        private string FormatLinea(string izquierda, string derecha, int ancho)
        {
            int espacios = ancho - izquierda.Length - derecha.Length;
            if (espacios < 1) espacios = 1;
            return izquierda + new string(' ', espacios) + derecha;
        }
    }
}