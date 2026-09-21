using POS.Domain.Enums;

namespace POS.Domain.Entities.Sales
{
    public class Pago
    {
        public int Id { get; set; }
        
        // Venta
        public int VentaId { get; set; }
        public virtual Venta Venta { get; set; } = null!;
        
        // Método de pago
        public MetodoPago Metodo { get; set; }
        
        // Montos
        public decimal Monto { get; set; }
        
        // Para efectivo
        public decimal? MontoRecibido { get; set; }
        public decimal? Cambio { get; set; }
        
        // Para transferencia/tarjeta
        public string? Referencia { get; set; }
        public string? NumeroAutorizacion { get; set; }
        
        // Auditoría
        public DateTime Fecha { get; set; } = DateTime.Now;
    }
}