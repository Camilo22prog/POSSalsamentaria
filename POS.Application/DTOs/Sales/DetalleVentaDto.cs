namespace POS.Application.DTOs.Sales
{
    public class DetalleVentaDto
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public string ProductoCodigo { get; set; } = string.Empty;
        public string ProductoNombre { get; set; } = string.Empty;
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Descuento { get; set; }
        public decimal PorcentajeIVA { get; set; }
        public decimal MontoIVA { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Total { get; set; }
    }
}