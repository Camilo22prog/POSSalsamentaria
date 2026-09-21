namespace POS.Application.DTOs.Purchases
{
    public class CrearCompraDto
    {
        public string? NumeroFactura { get; set; }
        public int ProveedorId { get; set; }
        public DateTime FechaCompra { get; set; } = DateTime.Now;
        public bool Pagado { get; set; } = true;
        public List<CrearDetalleCompraDto> Detalles { get; set; } = new();
    }
}
