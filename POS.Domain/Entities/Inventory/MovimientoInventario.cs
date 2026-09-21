using POS.Domain.Entities.Catalog;
using POS.Domain.Entities.Security;
using POS.Domain.Enums;

namespace POS.Domain.Entities.Inventory
{
    public class MovimientoInventario
    {
        public int Id { get; set; }
        
        // Producto y Lote
        public int ProductoId { get; set; }
        public virtual Producto Producto { get; set; } = null!;
        
        public int? LoteId { get; set; }
        public virtual Lote? Lote { get; set; }
        
        // Tipo de movimiento
        public TipoMovimientoInventario Tipo { get; set; }
        
        // Cantidades
        public decimal Cantidad { get; set; }
        public decimal StockAnterior { get; set; }
        public decimal StockNuevo { get; set; }
        
        // Costos
        public decimal? CostoUnitario { get; set; }
        public decimal? CostoTotal { get; set; }
        
        // Referencias
        public int? VentaId { get; set; }
        public int? CompraId { get; set; }
        public string? Referencia { get; set; } // Para ajustes manuales
        public string? Motivo { get; set; }
        
        // Usuario que realiza el movimiento
        public int UsuarioId { get; set; }
        public virtual Usuario Usuario { get; set; } = null!;
        
        // Auditoría
        public DateTime Fecha { get; set; } = DateTime.Now;
    }
}