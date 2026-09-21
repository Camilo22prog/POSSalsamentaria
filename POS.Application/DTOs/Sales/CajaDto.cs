namespace POS.Application.DTOs.Sales
{
    public class CajaDto
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string UsuarioNombre { get; set; } = string.Empty;
        public DateTime FechaApertura { get; set; }
        public DateTime? FechaCierre { get; set; }
        public decimal MontoInicial { get; set; }
        public decimal MontoFinal { get; set; }
        public decimal Diferencia { get; set; }
        public bool Abierta { get; set; }
        
        public decimal TotalEfectivo { get; set; }
        public decimal TotalTarjeta { get; set; }
        public decimal TotalNequi { get; set; }
        public decimal TotalDaviplata { get; set; }
        public decimal TotalTransferencia { get; set; }
        public decimal TotalQR { get; set; }
        
        // NUEVAS propiedades para arqueo
        public decimal TotalRetiros { get; set; }
        public decimal EfectivoEsperado { get; set; }
        public decimal EfectivoContado { get; set; }
        public decimal DiferenciaEfectivo { get; set; }
        
        public int CantidadVentas { get; set; }
    }
}