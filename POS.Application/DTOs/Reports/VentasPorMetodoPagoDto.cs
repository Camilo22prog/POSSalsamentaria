using POS.Domain.Enums;

namespace POS.Application.DTOs.Reports
{
    public class VentasPorMetodoPagoDto
    {
        public MetodoPago Metodo { get; set; }
        public string MetodoTexto { get; set; } = string.Empty;
        public int CantidadTransacciones { get; set; }
        public decimal MontoTotal { get; set; }
        public decimal PorcentajeDelTotal { get; set; }
        
        public List<DetallePagoDto> Detalles { get; set; } = new();
    }

   
    public class DetallePagoDto
    {
        public int VentaId { get; set; }
        public string NumeroVenta { get; set; } = string.Empty;
        public DateTime FechaVenta { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public string? NumeroAutorizacion { get; set; }
        public string? Referencia { get; set; }
        public string Usuario { get; set; } = string.Empty;
    }
}