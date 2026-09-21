namespace POS.Application.DTOs.Reports
{
    public class ResumenABCDto
    {
        public int TotalProductos { get; set; }
        public int ProductosA { get; set; }
        public int ProductosB { get; set; }
        public int ProductosC { get; set; }
        public decimal VentasTotales { get; set; }
        public decimal VentasA { get; set; }
        public decimal VentasB { get; set; }
        public decimal VentasC { get; set; }
        public decimal PorcentajeA { get; set; }
        public decimal PorcentajeB { get; set; }
        public decimal PorcentajeC { get; set; }
        public List<ProductoABCDto> Productos { get; set; } = new();
    }

    public class ProductoABCDto
    {
        public int ProductoId { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public int CantidadVendida { get; set; }
        public decimal TotalVentas { get; set; }
        public decimal PorcentajeVentas { get; set; }
        public decimal PorcentajeAcumulado { get; set; }
        public string ClasificacionABC { get; set; } = string.Empty;
        public decimal MargenUnitario { get; set; }
        public decimal UtilidadTotal { get; set; }
        public int StockActual { get; set; }
        public decimal RotacionInventario { get; set; }
    }
}