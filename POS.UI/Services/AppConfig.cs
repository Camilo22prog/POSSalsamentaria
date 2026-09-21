using POS.Application.DTOs.Settings;

namespace POS.UI.Services
{
    /// <summary>
    /// Singleton estático que mantiene la configuración activa en memoria.
    /// Se carga una vez al iniciar la app y se actualiza al guardar cambios.
    /// </summary>
    public class AppConfig
    {
        private static AppConfig? _instance;
        public static AppConfig Current => _instance ??= new AppConfig();

        private AppConfig() { }

        public static void Cargar(ConfiguracionDto dto)
        {
            Current.NombreNegocio        = dto.NombreNegocio;
            Current.Nit                  = dto.Nit;
            Current.Direccion            = dto.Direccion;
            Current.Telefono             = dto.Telefono;
            Current.MensajePieTicket     = dto.MensajePieTicket;
            Current.NombreImpresora      = dto.NombreImpresora;
            Current.AbrirCajonAutomatico = dto.AbrirCajonAutomatico;
            Current.PuertoBalanza        = dto.PuertoBalanza;
            Current.BaudRateBalanza      = dto.BaudRateBalanza;
            Current.UrlActualizaciones   = dto.UrlActualizaciones;
        }

        // ── Datos del negocio ────────────────────────────────────────────────
        public string NombreNegocio { get; private set; } = "Mi Negocio";
        public string? Nit { get; private set; }
        public string? Direccion { get; private set; }
        public string? Telefono { get; private set; }
        public string MensajePieTicket { get; private set; } = "¡Gracias por su compra!";

        // ── Impresora ────────────────────────────────────────────────────────
        public string? NombreImpresora { get; private set; }
        public bool AbrirCajonAutomatico { get; private set; } = true;

        // ── Balanza ──────────────────────────────────────────────────────────
        public string? PuertoBalanza { get; private set; }
        public int BaudRateBalanza { get; private set; } = 9600;

        // ── Actualizaciones ──────────────────────────────────────────────────
        public string? UrlActualizaciones { get; private set; }
    }
}
