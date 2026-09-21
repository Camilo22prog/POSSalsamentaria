using System.Drawing;
using System.Drawing.Printing;
using POS.Application.DTOs.Sales;
using POS.Domain.Enums;
using POS.UI.Services;

namespace POS.UI.Helpers
{
    public class TicketPrinter
    {
        private PrintDocument _printDocument;
        private VentaDto? _venta;
        private const int CARACTERES_POR_LINEA = 32; // Para 58mm

        public TicketPrinter()
        {
            _printDocument = new PrintDocument();
            _printDocument.PrintPage += PrintDocument_PrintPage;

            // 58mm = 2.28 inches = 228 hundredths of an inch
            var paperSize = new PaperSize("58mm", 228, 3000);
            _printDocument.DefaultPageSettings.PaperSize = paperSize;
            _printDocument.DefaultPageSettings.Margins = new Margins(2, 2, 5, 5);
        }

        /// <summary>Abre el cajón de dinero sin imprimir ticket.</summary>
        public void AbrirCajonSolo()
        {
            try
            {
                SeleccionarImpresora();
                AbrirCajon();
            }
            catch { }
        }

        public void ImprimirTicket(VentaDto venta, bool abrirCajon = true)
        {
            _venta = venta;

            try
            {
                SeleccionarImpresora();
                _printDocument.Print();

                if (abrirCajon && AppConfig.Current.AbrirCajonAutomatico)
                    AbrirCajon();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al imprimir ticket: {ex.Message}");
            }
        }

        private void SeleccionarImpresora()
        {
            var impresoraConfigurada = AppConfig.Current.NombreImpresora;
            if (!string.IsNullOrWhiteSpace(impresoraConfigurada))
            {
                _printDocument.PrinterSettings.PrinterName = impresoraConfigurada;
                return;
            }
            // Auto-detección por nombre
            foreach (string printer in PrinterSettings.InstalledPrinters)
            {
                if (printer.Contains("XP") || printer.Contains("58") ||
                    printer.Contains("Thermal") || printer.Contains("POS"))
                {
                    _printDocument.PrinterSettings.PrinterName = printer;
                    return;
                }
            }
        }

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            if (_venta == null || e.Graphics == null) return;

            // Fuentes más pequeñas para 58mm
            var fuente = new Font("Courier New", 7, FontStyle.Regular);
            var fuenteBold = new Font("Courier New", 7, FontStyle.Bold);
            var fuenteTitulo = new Font("Courier New", 9, FontStyle.Bold);
            
            float x = e.MarginBounds.Left;
            float y = e.MarginBounds.Top;
            float lineHeight = 13;

            // === ENCABEZADO ===
            var cfg = AppConfig.Current;
            e.Graphics.DrawString(Centrar(cfg.NombreNegocio.ToUpper()), fuenteTitulo, Brushes.Black, x, y);
            y += lineHeight + 3;
            if (!string.IsNullOrEmpty(cfg.Nit))
            {
                e.Graphics.DrawString(Centrar($"NIT: {cfg.Nit}"), fuente, Brushes.Black, x, y);
                y += lineHeight;
            }
            if (!string.IsNullOrEmpty(cfg.Direccion))
            {
                e.Graphics.DrawString(Centrar(Truncar(cfg.Direccion, CARACTERES_POR_LINEA)), fuente, Brushes.Black, x, y);
                y += lineHeight;
            }
            if (!string.IsNullOrEmpty(cfg.Telefono))
            {
                e.Graphics.DrawString(Centrar($"Tel: {cfg.Telefono}"), fuente, Brushes.Black, x, y);
                y += lineHeight;
            }
            e.Graphics.DrawString(Linea('='), fuente, Brushes.Black, x, y);
            y += lineHeight;

            // === INFORMACIÓN DE VENTA ===
            e.Graphics.DrawString($"Ticket: {_venta.NumeroVenta}", fuente, Brushes.Black, x, y);
            y += lineHeight;
            e.Graphics.DrawString($"Fecha: {_venta.Fecha:dd/MM/yy HH:mm}", fuente, Brushes.Black, x, y);
            y += lineHeight;
            e.Graphics.DrawString($"Cajero: {Truncar(_venta.UsuarioNombre, 24)}", fuente, Brushes.Black, x, y);
            y += lineHeight;
            e.Graphics.DrawString(Linea('-'), fuente, Brushes.Black, x, y);
            y += lineHeight;

            // === PRODUCTOS ===
            foreach (var detalle in _venta.Detalles)
            {
                // Nombre del producto (máximo 32 caracteres)
                var nombre = Truncar(detalle.ProductoNombre, CARACTERES_POR_LINEA);
                e.Graphics.DrawString(nombre, fuenteBold, Brushes.Black, x, y);
                y += lineHeight;

                // Cantidad x Precio = Total (formato compacto)
                var cant = detalle.Cantidad.ToString("N2");
                var precio = detalle.PrecioUnitario.ToString("C0");
                var total = detalle.Total.ToString("C0");
                
                var lineaDetalle = $"{cant}x{precio}";
                e.Graphics.DrawString(AlignRight(lineaDetalle, total), fuente, Brushes.Black, x, y);
                y += lineHeight;
            }

            y += 2;
            e.Graphics.DrawString(Linea('-'), fuente, Brushes.Black, x, y);
            y += lineHeight;

            // === TOTALES ===
            e.Graphics.DrawString(FormatLinea("SUBTOTAL:", _venta.Subtotal.ToString("C0")), fuente, Brushes.Black, x, y);
            y += lineHeight;
            
            if (_venta.DescuentoTotal > 0)
            {
                e.Graphics.DrawString(FormatLinea("Descuento:", "-" + _venta.DescuentoTotal.ToString("C0")), fuente, Brushes.Black, x, y);
                y += lineHeight;
            }

            e.Graphics.DrawString(FormatLinea("IVA:", _venta.IVATotal.ToString("C0")), fuente, Brushes.Black, x, y);
            y += lineHeight;
            
            e.Graphics.DrawString(Linea('='), fuente, Brushes.Black, x, y);
            y += lineHeight;
            
            e.Graphics.DrawString(FormatLinea("TOTAL:", _venta.Total.ToString("C0")), fuenteBold, Brushes.Black, x, y);
            y += lineHeight + 3;

            // === MÉTODOS DE PAGO ===
            e.Graphics.DrawString("PAGO:", fuenteBold, Brushes.Black, x, y);
            y += lineHeight;
            
            foreach (var pago in _venta.Pagos)
{
    // Obtener nombre del método de pago
                string metodo = pago.Metodo switch
                {
                    MetodoPago.Efectivo => "EFECTIVO",
                    MetodoPago.Tarjeta => "TARJETA",
                    MetodoPago.Nequi => "NEQUI",
                    MetodoPago.Daviplata => "DAVIPLATA",
                    MetodoPago.Transferencia => "TRANSFERENCIA",
                    MetodoPago.QR => "QR",
                    MetodoPago.Credito => "CREDITO",
                    _ => pago.Metodo.ToString().ToUpper()
                };
                
                e.Graphics.DrawString(FormatLinea($" {metodo}:", $"{pago.Monto:C0}"), fuente, Brushes.Black, x, y);
                y += lineHeight;
                
                if (pago.Cambio > 0)
                {
                    e.Graphics.DrawString(FormatLinea(" CAMBIO:", $"{pago.Cambio:C0}"), fuenteBold, Brushes.Black, x, y);
                    y += lineHeight;
                }
            }

            y += 5;
            e.Graphics.DrawString(Linea('='), fuente, Brushes.Black, x, y);
            y += lineHeight;
            
            // === PIE ===
            var pie = AppConfig.Current.MensajePieTicket;
            e.Graphics.DrawString(Centrar(pie.ToUpper()), fuenteBold, Brushes.Black, x, y);

            e.HasMorePages = false;
        }

        private void AbrirCajon()
        {
            try
            {
                // Comando ESC/POS para abrir cajón: ESC p m t1 t2
                byte[] comando = { 0x1B, 0x70, 0x00, 0x19, 0x19 };
                RawPrinterHelper.SendBytesToPrinter(_printDocument.PrinterSettings.PrinterName, comando);
            }
            catch
            {
                // Si falla, ignorar (cajón puede no estar conectado)
            }
        }

        // === MÉTODOS DE FORMATO ===

        private string Centrar(string texto)
        {
            if (texto.Length >= CARACTERES_POR_LINEA) return texto;
            int espacios = (CARACTERES_POR_LINEA - texto.Length) / 2;
            return new string(' ', espacios) + texto;
        }

        private string Linea(char caracter = '-')
        {
            return new string(caracter, CARACTERES_POR_LINEA);
        }

        private string Truncar(string texto, int maxLength)
        {
            return texto.Length > maxLength ? texto.Substring(0, maxLength) : texto;
        }

        private string FormatLinea(string izquierda, string derecha)
        {
            int espaciosDisponibles = CARACTERES_POR_LINEA - izquierda.Length - derecha.Length;
            if (espaciosDisponibles < 1) espaciosDisponibles = 1;
            return izquierda + new string(' ', espaciosDisponibles) + derecha;
        }

        private string AlignRight(string izquierda, string derecha)
        {
            // Alinea derecha con espacio entre izquierda y derecha
            int espacios = CARACTERES_POR_LINEA - izquierda.Length - derecha.Length;
            if (espacios < 1) espacios = 1;
            return izquierda + new string(' ', espacios) + derecha;
        }
    }

    // === HELPER PARA COMANDOS RAW ===
    public class RawPrinterHelper
    {
        [System.Runtime.InteropServices.DllImport("winspool.drv", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private static extern bool OpenPrinter(string pPrinterName, out IntPtr phPrinter, IntPtr pDefault);

        [System.Runtime.InteropServices.DllImport("winspool.drv")]
        private static extern bool ClosePrinter(IntPtr hPrinter);

        [System.Runtime.InteropServices.DllImport("winspool.drv", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private static extern bool StartDocPrinter(IntPtr hPrinter, int Level, ref DOC_INFO_1 pDocInfo);

        [System.Runtime.InteropServices.DllImport("winspool.drv")]
        private static extern bool EndDocPrinter(IntPtr hPrinter);

        [System.Runtime.InteropServices.DllImport("winspool.drv")]
        private static extern bool StartPagePrinter(IntPtr hPrinter);

        [System.Runtime.InteropServices.DllImport("winspool.drv")]
        private static extern bool EndPagePrinter(IntPtr hPrinter);

        [System.Runtime.InteropServices.DllImport("winspool.drv")]
        private static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, int dwCount, out int dwWritten);

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private struct DOC_INFO_1
        {
            public string pDocName;
            public string pOutputFile;
            public string pDataType;
        }

        public static bool SendBytesToPrinter(string printerName, byte[] bytes)
        {
            IntPtr hPrinter = IntPtr.Zero;
            DOC_INFO_1 di = new DOC_INFO_1
            {
                pDocName = "Ticket",
                pDataType = "RAW"
            };

            try
            {
                if (!OpenPrinter(printerName, out hPrinter, IntPtr.Zero))
                    return false;

                if (!StartDocPrinter(hPrinter, 1, ref di))
                    return false;

                if (!StartPagePrinter(hPrinter))
                    return false;

                IntPtr pBytes = System.Runtime.InteropServices.Marshal.AllocHGlobal(bytes.Length);
                System.Runtime.InteropServices.Marshal.Copy(bytes, 0, pBytes, bytes.Length);

                WritePrinter(hPrinter, pBytes, bytes.Length, out int written);
                System.Runtime.InteropServices.Marshal.FreeHGlobal(pBytes);

                EndPagePrinter(hPrinter);
                EndDocPrinter(hPrinter);

                return true;
            }
            finally
            {
                if (hPrinter != IntPtr.Zero)
                    ClosePrinter(hPrinter);
            }
        }
    }
}