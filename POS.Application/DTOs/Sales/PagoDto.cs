using POS.Domain.Enums;

namespace POS.Application.DTOs.Sales
{
    public class PagoDto
    {
        public int Id { get; set; }
        public int VentaId { get; set; }
        public MetodoPago Metodo { get; set; }
        public string MetodoTexto { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public decimal? Cambio { get; set; }
        public string? Referencia { get; set; }
    }
}