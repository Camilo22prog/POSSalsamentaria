using POS.Domain.Entities.Catalog;  // AGREGAR

namespace POS.Domain.Entities.Sales
{
    public class DetalleVenta
    {
        public int Id { get; set; }
        public int VentaId { get; set; }
        public Venta Venta { get; set; } = null!;
        
        public int ProductoId { get; set; }
        public Producto Producto { get; set; } = null!;
        
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Descuento { get; set; }
        
        // IVA
        public decimal PorcentajeIVA { get; set; }
        public decimal MontoIVA { get; set; }
        
        public decimal Subtotal { get; set; }
        public decimal Total { get; set; }
        
        public decimal CostoUnitario { get; set; }
    }
}