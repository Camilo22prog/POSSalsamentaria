namespace POS.Application.DTOs.Sales
{
    public class CerrarCajaDto
    {
        public decimal MontoEfectivoContado { get; set; }
        public decimal MontoTarjetaContado { get; set; }
        public decimal MontoNequiContado { get; set; }
        public decimal MontoDaviplataContado { get; set; }
        public decimal MontoTransferenciaContado { get; set; }
        public decimal MontoQRContado { get; set; }
    }
}