namespace POS.Application.DTOs.Purchases
{
    public class CompraDto
    {
        public int Id { get; set; }
        public string NumeroCompra { get; set; } = string.Empty;
        public string? NumeroFactura { get; set; }
        public int ProveedorId { get; set; }
        public string ProveedorNombre { get; set; } = string.Empty;
        public string UsuarioNombre { get; set; } = string.Empty;
        public DateTime FechaCompra { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Descuento { get; set; }
        public decimal Impuesto { get; set; }
        public decimal Total { get; set; }
        public bool Pagado { get; set; }
        public bool Anulada { get; set; }
        public string? MotivoAnulacion { get; set; }
        public List<DetalleCompraDto> Detalles { get; set; } = new();
    }
}
