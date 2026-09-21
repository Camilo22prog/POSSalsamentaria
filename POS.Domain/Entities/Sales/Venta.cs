using POS.Domain.Entities.Customers;
using POS.Domain.Entities.Security;

namespace POS.Domain.Entities.Sales
{
    public class Venta
    {
        public int Id { get; set; }
        public string NumeroVenta { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        
        // Cliente (opcional)
        public int? ClienteId { get; set; }
        public Cliente? Cliente { get; set; }
        
        // Usuario y Caja
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;
        
        public int CajaId { get; set; }
        public Caja Caja { get; set; } = null!;
        
        // Totales
        public decimal Subtotal { get; set; }
        public decimal DescuentoTotal { get; set; }
        public decimal IVATotal { get; set; }
        public decimal Total { get; set; }
        
        // Estado
        public bool Anulada { get; set; }
        public string? MotivoAnulacion { get; set; }
        public DateTime? FechaAnulacion { get; set; }  // ✅ AGREGAR si no existe
        public int? AnuladaPorUsuarioId { get; set; }
        
        // Relaciones
        public ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
        public ICollection<Pago> Pagos { get; set; } = new List<Pago>();
    }
}