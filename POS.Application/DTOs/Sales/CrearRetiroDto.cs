namespace POS.Application.DTOs.Sales
{
    public class CrearRetiroDto
    {
        public decimal Monto { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public string? Observaciones { get; set; }
    }
}