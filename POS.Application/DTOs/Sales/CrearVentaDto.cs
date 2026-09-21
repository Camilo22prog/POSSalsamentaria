using POS.Domain.Enums;

namespace POS.Application.DTOs.Sales
{
    public class CrearVentaDto
    {
        public int? ClienteId { get; set; }
        public List<ItemVentaDto> Items { get; set; } = new();
        public List<PagoVentaDto> Pagos { get; set; } = new();
        public decimal? DescuentoGeneral { get; set; }
    }

    public class ItemVentaDto
    {
        public int ProductoId { get; set; }
        public decimal Cantidad { get; set; }
        public decimal? DescuentoLinea { get; set; }
    }

    public class PagoVentaDto
    {
        public MetodoPago Metodo { get; set; }
        public decimal Monto { get; set; }
        public string? Referencia { get; set; }
    }
}