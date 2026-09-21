using POS.Domain.Entities.Catalog;
using POS.Domain.Enums;

namespace POS.Domain.Entities.Inventory
{
    public class Lote
    {
        public int Id { get; set; }
        public string NumeroLote { get; set; } = string.Empty;
        
        // Producto
        public int ProductoId { get; set; }
        public virtual Producto Producto { get; set; } = null!;
        
        // Cantidades
        public decimal CantidadInicial { get; set; }
        public decimal CantidadActual { get; set; }
        
        // Fechas
        public DateTime FechaIngreso { get; set; } = DateTime.Now;
        public DateTime? FechaVencimiento { get; set; }
        
        // Costos
        public decimal CostoUnitario { get; set; }
        
        // Control
        public EstadoRegistro Estado { get; set; } = EstadoRegistro.Activo;
        
        // Navegación
        public virtual ICollection<MovimientoInventario> MovimientosInventario { get; set; } = new List<MovimientoInventario>();
    }
}