namespace POS.Domain.Entities.Settings
{
    public class Configuracion
    {
        // Siempre Id = 1 (fila única)
        public int Id { get; set; }

        // ── Datos del negocio ────────────────────────────────────────────────
        public string NombreNegocio { get; set; } = "Mi Negocio";
        public string? Nit { get; set; }
        public string? Direccion { get; set; }
        public string? Telefono { get; set; }
        public string MensajePieTicket { get; set; } = "¡Gracias por su compra!";

        // ── Impresora térmica ────────────────────────────────────────────────
        // null = auto-detectar por nombre (XP, 58, Thermal, POS)
        public string? NombreImpresora { get; set; }
        public bool AbrirCajonAutomatico { get; set; } = true;

        // ── Balanza serial ───────────────────────────────────────────────────
        // null = auto-detectar escaneando puertos COM
        public string? PuertoBalanza { get; set; }
        public int BaudRateBalanza { get; set; } = 9600;

        // ── Actualizaciones ──────────────────────────────────────────────────
        public string? UrlActualizaciones { get; set; }

        public DateTime FechaModificacion { get; set; } = DateTime.Now;
    }
}
