using POS.Domain.Enums;

namespace POS.Application.DTOs.Inventory
{
    public class AjustarStockDto
    {
        public int ProductoId { get; set; }
        public decimal CantidadNueva { get; set; }
        public TipoMovimientoInventario Tipo { get; set; }
        public string? Motivo { get; set; }
        public decimal? CostoUnitario { get; set; }
        public decimal? PrecioCompraNuevo { get; set; }
        public decimal? PrecioVentaNuevo { get; set; }
    }
}