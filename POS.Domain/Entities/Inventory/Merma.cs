using POS.Domain.Entities.Catalog;
using POS.Domain.Entities.Security;

namespace POS.Domain.Entities.Inventory
{
    public class Merma
    {
        public int Id { get; set; }
        
        // Producto
        public int ProductoId { get; set; }
        public virtual Producto Producto { get; set; } = null!;
        
        public int? LoteId { get; set; }
        public virtual Lote? Lote { get; set; }
        
        // Detalles
        public decimal Cantidad { get; set; }
        public string Motivo { get; set; } = string.Empty; // Vencido, Dañado, Robo, etc.
        public string? Observaciones { get; set; }
        
        // Costos
        public decimal CostoUnitario { get; set; }
        public decimal CostoTotal { get; set; }
        
        // Usuario que registra
        public int UsuarioId { get; set; }
        public virtual Usuario Usuario { get; set; } = null!;
        
        // Auditoría
        public DateTime Fecha { get; set; } = DateTime.Now;
    }
}