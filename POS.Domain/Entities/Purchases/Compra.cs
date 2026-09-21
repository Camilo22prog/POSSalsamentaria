using POS.Domain.Entities.Security;
using POS.Domain.Enums;

namespace POS.Domain.Entities.Purchases
{
    public class Compra
    {
        public int Id { get; set; }
        public string NumeroCompra { get; set; } = string.Empty;
        public string? NumeroFactura { get; set; }
        
        // Proveedor
        public int ProveedorId { get; set; }
        public virtual Proveedor Proveedor { get; set; } = null!;
        
        // Usuario que registra
        public int UsuarioId { get; set; }
        public virtual Usuario Usuario { get; set; } = null!;
        
        // Totales
        public decimal Subtotal { get; set; }
        public decimal Descuento { get; set; } = 0;
        public decimal Impuesto { get; set; } = 0;
        public decimal Total { get; set; }
        
        // Control de pago
        public bool Pagado { get; set; } = false;
        public DateTime? FechaPago { get; set; }
        
        // Estado
        public bool Anulada { get; set; } = false;
        public DateTime? FechaAnulacion { get; set; }
        public string? MotivoAnulacion { get; set; }
        
        // Auditoría
        public DateTime FechaCompra { get; set; } = DateTime.Now;
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        
        // Navegación
        public virtual ICollection<DetalleCompra> DetallesCompra { get; set; } = new List<DetalleCompra>();
    }
}