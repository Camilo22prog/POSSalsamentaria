namespace POS.Application.DTOs.Reports
{
    public class ResumenVentasAnuladasDto
    {
        public int TotalVentasAnuladas { get; set; }
        public decimal MontoTotalAnulado { get; set; }
        public decimal PorcentajeAnulacion { get; set; }
        public int VentasTotales { get; set; }
        public decimal MontoTotalVentas { get; set; }
        public string UsuarioConMasAnulaciones { get; set; } = string.Empty;
        public int CantidadMasAnulaciones { get; set; }
        public List<VentaAnuladaDto> Ventas { get; set; } = new();
    }

    public class VentaAnuladaDto
    {
        public int VentaId { get; set; }
        public string NumeroVenta { get; set; } = string.Empty;
        public DateTime FechaVenta { get; set; }
        public DateTime FechaAnulacion { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public string MotivoAnulacion { get; set; } = string.Empty;
        public int TiempoHastaAnulacion { get; set; } // Minutos
        public int CantidadProductos { get; set; }
        public string MetodoPago { get; set; } = string.Empty;
    }
}