using POS.Domain.Entities.Security;

namespace POS.Domain.Entities.Sales
{
    public class Caja
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public DateTime FechaApertura { get; set; }
        public DateTime? FechaCierre { get; set; }
        public decimal MontoInicial { get; set; }
        public decimal MontoFinal { get; set; }
        public decimal Diferencia { get; set; }
        public bool Abierta { get; set; }

        // Totales por método de pago
        public decimal TotalEfectivo { get; set; }
        public decimal TotalTarjeta { get; set; }
        public decimal TotalNequi { get; set; }
        public decimal TotalDaviplata { get; set; }
        public decimal TotalTransferencia { get; set; }
        public decimal TotalQR { get; set; }

        // NUEVOS campos para arqueo
        public decimal EfectivoEsperado { get; set; }
        public decimal EfectivoContado { get; set; }
        public decimal DiferenciaEfectivo { get; set; }
        public decimal TotalRetiros { get; set; }

        // Navegación
        public Usuario Usuario { get; set; } = null!;
        public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
        public ICollection<DetalleDenominacion> DetallesDenominaciones { get; set; } = new List<DetalleDenominacion>();
        public ICollection<RetiroCaja> Retiros { get; set; } = new List<RetiroCaja>();
    }
}