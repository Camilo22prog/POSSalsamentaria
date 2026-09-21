namespace POS.Application.DTOs.Purchases
{
    public class CrearDetalleCompraDto
    {
        public int ProductoId { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Descuento { get; set; } = 0;
        public DateTime? FechaVencimiento { get; set; }
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
        public string? Observaciones { get; set; }
    }
}
