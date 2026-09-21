using POS.Application.Interfaces;
using System;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;

namespace POS.Application.Services
{
    /// <summary>
    /// Servicio para balanza serial en modo stream continuo.
    /// La balanza envía tramas periódicamente sin necesidad de comando.
    /// Estrategia de lectura manual:
    ///   1. Vaciar buffer (descartar datos viejos)
    ///   2. Esperar a que llegue una trama fresca (~1.5 s máximo)
    ///   3. Leer y parsear la última trama recibida
    /// </summary>
    public class BalanzaService : IBalanzaService, IDisposable
    {
        private SerialPort? _serialPort;
        private bool _disposed = false;

        // Tiempo máximo esperando el primer byte tras limpiar el buffer.
        // Cubre balanzas lentas que envían cada ~500 ms.
        private const int TIMEOUT_ESPERA_MS    = 1500;
        // Tiempo extra para dejar acumular la trama completa una vez que
        // ya hay al menos un byte disponible.
        private const int ESPERA_TRAMA_MS      = 150;
        // Intervalo de polling mientras se espera el primer byte.
        private const int INTERVALO_POLLING_MS = 50;

        public bool EstaConectada => _serialPort?.IsOpen ?? false;

        // ─────────────────────────────────────────────────────────────────────
        // CONEXIÓN
        // ─────────────────────────────────────────────────────────────────────
        public async Task<bool> ConectarAsync(string puerto, int baudRate = 9600)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"\n🔌 Conectando a {puerto}...");

                var puertosDisponibles = SerialPort.GetPortNames();
                if (!puertosDisponibles.Contains(puerto))
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Puerto {puerto} no existe. Disponibles: {string.Join(", ", puertosDisponibles)}");
                    return false;
                }

                // Cerrar conexión previa si existe
                if (_serialPort?.IsOpen == true)
                {
                    _serialPort.Close();
                    _serialPort.Dispose();
                }

                _serialPort = new SerialPort
                {
                    PortName     = puerto,
                    BaudRate     = baudRate,
                    DataBits     = 8,
                    Parity       = Parity.None,
                    StopBits     = StopBits.One,
                    Handshake    = Handshake.None,
                    ReadTimeout  = 2000,
                    WriteTimeout = 1000
                };

                try
                {
                    _serialPort.Open();
                    System.Diagnostics.Debug.WriteLine($"✅ Puerto {puerto} abierto");
                }
                catch (UnauthorizedAccessException)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Puerto {puerto} en uso");
                    return false;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Error abriendo {puerto}: {ex.Message}");
                    return false;
                }

                // Estabilización inicial
                await Task.Delay(400);

                // Verificar que hay una balanza respondiendo:
                // limpiamos buffer y esperamos que llegue algo del stream.
                _serialPort.DiscardInBuffer();
                _serialPort.DiscardOutBuffer();

                int esperado = 0;
                while (_serialPort.BytesToRead == 0 && esperado < TIMEOUT_ESPERA_MS)
                {
                    await Task.Delay(INTERVALO_POLLING_MS);
                    esperado += INTERVALO_POLLING_MS;
                }

                // Si sigue vacío, puede que requiera comando para empezar
                if (_serialPort.BytesToRead == 0)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ Sin stream espontáneo, intentando con comando 'R'...");
                    try { _serialPort.WriteLine("R"); } catch { /* ignorar */ }
                    await Task.Delay(600);
                }

                if (_serialPort.BytesToRead == 0)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ No hay respuesta en {puerto}");
                    _serialPort.Close();
                    _serialPort.Dispose();
                    _serialPort = null;
                    return false;
                }

                string respuestaValidacion = _serialPort.ReadExisting();
                System.Diagnostics.Debug.WriteLine($"✅ Balanza detectada en {puerto}. Muestra: '{respuestaValidacion.Trim()}'");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error general: {ex.Message}");
                return false;
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // LECTURA MANUAL DE PESO
        // El buffer se vacía PRIMERO para garantizar que leemos el peso actual
        // y no un dato que quedó de una lectura anterior.
        // ─────────────────────────────────────────────────────────────────────
        public async Task<decimal> LeerPesoAsync()
        {
            if (_serialPort == null || !_serialPort.IsOpen)
            {
                System.Diagnostics.Debug.WriteLine("❌ Puerto no abierto");
                return -1;
            }

            try
            {
                System.Diagnostics.Debug.WriteLine("\n⚖️ === LEYENDO PESO ===");

                // ── Paso 1: vaciar buffer para no leer un dato viejo ──────────
                _serialPort.DiscardInBuffer();
                System.Diagnostics.Debug.WriteLine("🧹 Buffer vaciado, esperando trama fresca...");

                // ── Paso 2: esperar al primer byte del stream ─────────────────
                int esperado = 0;
                while (_serialPort.BytesToRead == 0 && esperado < TIMEOUT_ESPERA_MS)
                {
                    await Task.Delay(INTERVALO_POLLING_MS);
                    esperado += INTERVALO_POLLING_MS;
                }

                // Si no llegó nada, intentar con comando "R"
                if (_serialPort.BytesToRead == 0)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ Sin datos espontáneos, enviando comando 'R'...");
                    try { _serialPort.WriteLine("R"); } catch { /* ignorar */ }

                    esperado = 0;
                    while (_serialPort.BytesToRead == 0 && esperado < TIMEOUT_ESPERA_MS)
                    {
                        await Task.Delay(INTERVALO_POLLING_MS);
                        esperado += INTERVALO_POLLING_MS;
                    }
                }

                if (_serialPort.BytesToRead == 0)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ La balanza no respondió");
                    return 0;
                }

                // ── Paso 3: esperar que llegue la trama completa ──────────────
                await Task.Delay(ESPERA_TRAMA_MS);

                // ── Paso 4: leer y parsear ────────────────────────────────────
                string respuesta = _serialPort.ReadExisting();
                System.Diagnostics.Debug.WriteLine($"📥 Raw ({respuesta.Length} chars): '{respuesta.Trim()}'");
                System.Diagnostics.Debug.WriteLine($"🔢 HEX: {BitConverter.ToString(System.Text.Encoding.ASCII.GetBytes(respuesta))}");

                decimal peso = ParsearPeso(respuesta);
                System.Diagnostics.Debug.WriteLine($"🎯 PESO FINAL: {peso:F3} kg\n");
                return peso;
            }
            catch (TimeoutException)
            {
                System.Diagnostics.Debug.WriteLine("⏱️ Timeout leyendo balanza");
                return 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error: {ex.Message}");
                return -1;
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // PARSEO — usa la última línea con dígitos de la respuesta
        // ─────────────────────────────────────────────────────────────────────
        private decimal ParsearPeso(string respuesta)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(respuesta))
                    return 0;

                // En modo stream pueden llegar varias tramas juntas.
                // Dividir por saltos de línea y usar la ÚLTIMA línea con dígitos.
                var lineas = respuesta.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                string? lineaValida = null;
                for (int i = lineas.Length - 1; i >= 0; i--)
                {
                    string l = lineas[i].Trim();
                    if (System.Text.RegularExpressions.Regex.IsMatch(l, @"\d"))
                    {
                        lineaValida = l;
                        break;
                    }
                }

                if (lineaValida == null)
                {
                    System.Diagnostics.Debug.WriteLine("❌ Ninguna línea con dígitos");
                    return 0;
                }

                System.Diagnostics.Debug.WriteLine($"📌 Línea a parsear: '{lineaValida}'");

                // Extraer solo dígitos, punto y coma
                string limpio = System.Text.RegularExpressions.Regex.Replace(lineaValida, @"[^\d.,]", "");
                System.Diagnostics.Debug.WriteLine($"🧹 Limpio: '{limpio}'");

                if (string.IsNullOrWhiteSpace(limpio))
                    return 0;

                // Normalizar separador decimal
                if (limpio.Contains(',') && !limpio.Contains('.'))
                    limpio = limpio.Replace(',', '.');
                else if (limpio.Contains(',') && limpio.Contains('.'))
                    limpio = limpio.Replace(",", ""); // comas = separadores de miles

                if (decimal.TryParse(limpio,
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out decimal peso))
                {
                    System.Diagnostics.Debug.WriteLine($"✅ Parseado: {peso} kg");
                    return Math.Abs(peso);
                }

                System.Diagnostics.Debug.WriteLine($"❌ No se pudo parsear '{limpio}'");
                return 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error parseo: {ex.Message}");
                return 0;
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // DETECCIÓN AUTOMÁTICA
        // ─────────────────────────────────────────────────────────────────────
        public async Task<string?> DetectarPuertoAsync()
        {
            var puertos = SerialPort.GetPortNames();

            if (puertos.Length == 0)
            {
                System.Diagnostics.Debug.WriteLine("❌ No hay puertos COM");
                return null;
            }

            System.Diagnostics.Debug.WriteLine($"\n🔍 Buscando balanza en: {string.Join(", ", puertos)}");

            foreach (var puerto in puertos)
            {
                System.Diagnostics.Debug.WriteLine($"\n--- Probando {puerto} ---");
                if (await ConectarAsync(puerto))
                {
                    System.Diagnostics.Debug.WriteLine($"✅ BALANZA EN {puerto}\n");
                    return puerto;
                }
            }

            System.Diagnostics.Debug.WriteLine("\n❌ No se encontró balanza\n");
            return null;
        }

        // ─────────────────────────────────────────────────────────────────────
        // UTILIDADES
        // ─────────────────────────────────────────────────────────────────────
        public string[] ObtenerPuertosDisponibles()
        {
            var puertos = SerialPort.GetPortNames();
            System.Diagnostics.Debug.WriteLine($"📍 Puertos disponibles: {string.Join(", ", puertos)}");
            return puertos;
        }

        public void Desconectar()
        {
            try
            {
                if (_serialPort?.IsOpen == true)
                {
                    _serialPort.Close();
                    System.Diagnostics.Debug.WriteLine("🔌 Balanza desconectada");
                }
                _serialPort?.Dispose();
                _serialPort = null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error desconectando: {ex.Message}");
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                Desconectar();
                _disposed = true;
            }
        }
    }
}