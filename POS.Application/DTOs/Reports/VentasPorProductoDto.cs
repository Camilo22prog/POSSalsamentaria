namespace POS.Application.DTOs.Reports
{
    public class VentasPorProductoDto
    {
        public int ProductoId { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public int CantidadVendida { get; set; }
        public decimal PrecioPromedio { get; set; }
        public decimal TotalVentas { get; set; }
        public decimal CostoTotal { get; set; }
        public decimal UtilidadBruta { get; set; }
        public decimal MargenPorcentaje { get; set; }
        public decimal PorcentajeDelTotal { get; set; }
    }
}