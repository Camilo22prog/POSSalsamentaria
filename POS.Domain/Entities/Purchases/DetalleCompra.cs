using POS.Domain.Entities.Catalog;
using POS.Domain.Entities.Inventory;

namespace POS.Domain.Entities.Purchases
{
    public class DetalleCompra
    {
        public int Id { get; set; }
        
        // Compra
        public int CompraId { get; set; }
        public virtual Compra Compra { get; set; } = null!;
        
        // Producto
        public int ProductoId { get; set; }
        public virtual Producto Producto { get; set; } = null!;
        
        // Lote generado
        public int? LoteId { get; set; }
        public virtual Lote? Lote { get; set; }
        
        // Cantidad
        public decimal Cantidad { get; set; }
        
        // Precios
        public decimal PrecioUnitario { get; set; }
        public decimal Descuento { get; set; } = 0;
        public decimal Subtotal { get; set; }
        
        // Vencimiento
        public DateTime? FechaVencimiento { get; set; }

        // Nuevos campos de liquidación
        public decimal PrecioSinIva { get; set; }
        public decimal PrecioConIva { get; set; }
        public decimal IvaPorcentaje { get; set; }
        public decimal IvaValor { get; set; }
        public decimal DescuentoPorcentaje { get; set; }
        public decimal DescuentoValor { get; set; }
        public decimal IbuaPorcentaje { get; set; }
        public decimal IbuaValor { get; set; }
        public decimal IcuiPorcentaje { get; set; }
        public decimal IcuiValor { get; set; }
        public decimal CostoUnitarioLiquidado { get; set; }
        public decimal PorcentajeGanancia { get; set; }
        public decimal PrecioVentaCalculado { get; set; }
        public string? LoteCodigo { get; set; }
        public bool ActualizarCatalogo { get; set; }
        
        // Observaciones
        public string? Observaciones { get; set; }
    }
}