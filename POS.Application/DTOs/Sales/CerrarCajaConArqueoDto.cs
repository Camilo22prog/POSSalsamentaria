namespace POS.Application.DTOs.Sales
{
    public class CerrarCajaConArqueoDto
    {
        public List<DetalleDenominacionDto> Monedas { get; set; } = new();
        public List<DetalleDenominacionDto> Billetes { get; set; } = new();
        public decimal EfectivoContado { get; set; } // Calculado automáticamente
        
        // Otros métodos (ya los tenías)
        public decimal MontoTarjetaContado { get; set; }
        public decimal MontoNequiContado { get; set; }
        public decimal MontoDaviplataContado { get; set; }
        public decimal MontoTransferenciaContado { get; set; }
        public decimal MontoQRContado { get; set; }
    }
}