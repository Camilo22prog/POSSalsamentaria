using POS.Domain.Enums;

namespace POS.Application.DTOs.Inventory
{
    public class InventarioProductoDto
    {
        public int ProductoId { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string CategoriaNombre { get; set; } = string.Empty;
        public string? CategoriaColor { get; set; }
        public decimal StockActual { get; set; }
        public decimal StockMinimo { get; set; }
        public bool TieneStockBajo { get; set; }
        public string? UnidadMedida { get; set; }
        public decimal? CostoPromedio { get; set; }
        public decimal? ValorInventario { get; set; }
        public DateTime? UltimoMovimiento { get; set; }
        public decimal PrecioCompra { get; set; }
        public decimal PrecioVenta { get; set; }
        public TipoIVA TipoIVA { get; set; }
    }
}