namespace POS.Application.DTOs.Sales
{
    public class AbrirCajaConDenominacionesDto
    {
        public List<DetalleDenominacionDto> Monedas { get; set; } = new();
        public List<DetalleDenominacionDto> Billetes { get; set; } = new();
        public decimal MontoInicial { get; set; } // Calculado automáticamente
    }
}