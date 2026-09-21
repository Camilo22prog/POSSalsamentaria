namespace POS.Application.DTOs.Reports
{
    public class InventarioReporteDto
    {
        public int ProductoId { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
        public decimal PrecioCompra { get; set; }
        public decimal PrecioVenta { get; set; }
        public decimal ValorInventario { get; set; }
        public int DiasDeInventario { get; set; }
        public decimal RotacionInventario { get; set; }
        public string EstadoStock { get; set; } = string.Empty; // "Normal", "Bajo", "Crítico", "Sin stock"
    }

    public class ResumenInventarioDto
    {
        public int TotalProductos { get; set; }
        public int ProductosConStock { get; set; }
        public int ProductosSinStock { get; set; }
        public int ProductosStockBajo { get; set; }
        public decimal ValorTotalInventario { get; set; }
        public decimal RotacionPromedio { get; set; }
        public List<InventarioReporteDto> Productos { get; set; } = new();
    }
}