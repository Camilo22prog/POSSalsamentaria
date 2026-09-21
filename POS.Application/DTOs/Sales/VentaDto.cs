using POS.Domain.Enums;

namespace POS.Application.DTOs.Sales
{
    public class VentaDto
    {
        public int Id { get; set; }
        public string NumeroVenta { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        
        // Cliente (opcional)
        public int? ClienteId { get; set; }
        public string? ClienteNombre { get; set; }
        
        // Usuario y Caja
        public int UsuarioId { get; set; }
        public string UsuarioNombre { get; set; } = string.Empty;
        public int CajaId { get; set; }
        
        // Totales
        public decimal Subtotal { get; set; }
        public decimal DescuentoTotal { get; set; }
        public decimal IVATotal { get; set; }
        public decimal Total { get; set; }
        
        // Detalles
        public List<DetalleVentaDto> Detalles { get; set; } = new();
        public List<PagoDto> Pagos { get; set; } = new();
        
        // Estado
        public bool Anulada { get; set; }
        public string? MotivoAnulacion { get; set; }
        public DateTime? FechaAnulacion { get; set; }
    }
}