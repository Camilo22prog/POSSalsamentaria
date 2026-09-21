namespace POS.Application.DTOs.Reports
{
    public class ResumenDesempenoDto
    {
        public int TotalUsuarios { get; set; }
        public int TotalVentas { get; set; }
        public decimal TotalIngresos { get; set; }
        public decimal TicketPromedio { get; set; }
        public string MejorVendedor { get; set; } = string.Empty;
        public List<DesempenoUsuarioDto> Usuarios { get; set; } = new();
    }

    public class DesempenoUsuarioDto
    {
        public int UsuarioId { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public int CantidadVentas { get; set; }
        public decimal TotalVentas { get; set; }
        public decimal PorcentajeVentas { get; set; }
        public decimal TicketPromedio { get; set; }
        public int VentasAnuladas { get; set; }
        public decimal PorcentajeAnulacion { get; set; }
        public decimal DescuentosAplicados { get; set; }
        public decimal PorcentajeDescuentos { get; set; }
        public DateTime? UltimaVenta { get; set; }
        public int DiasActivo { get; set; }
        public decimal VentasPorDia { get; set; }
        
        // Distribución por método de pago
        public decimal VentasEfectivo { get; set; }
        public decimal VentasTarjeta { get; set; }
        public decimal VentasTransferencia { get; set; }
        public decimal VentasNequi { get; set; }
        public decimal VentasDaviplata { get; set; }
        public decimal VentasQR { get; set; }
        
        // Métricas de tiempo
        public int HorasActivo { get; set; }
        public decimal VentasPorHora { get; set; }
    }
}