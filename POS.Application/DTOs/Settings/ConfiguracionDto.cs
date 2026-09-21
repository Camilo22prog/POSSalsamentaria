namespace POS.Application.DTOs.Settings
{
    public class ConfiguracionDto
    {
        public string NombreNegocio { get; set; } = "Mi Negocio";
        public string? Nit { get; set; }
        public string? Direccion { get; set; }
        public string? Telefono { get; set; }
        public string MensajePieTicket { get; set; } = "¡Gracias por su compra!";

        public string? NombreImpresora { get; set; }
        public bool AbrirCajonAutomatico { get; set; } = true;

        public string? PuertoBalanza { get; set; }
        public int BaudRateBalanza { get; set; } = 9600;

        public string? UrlActualizaciones { get; set; }
    }
}
