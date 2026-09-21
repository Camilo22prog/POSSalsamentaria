namespace POS.Application.DTOs.Reports
{
    public class VentasPorPeriodoDto
    {
        public DateTime Fecha { get; set; }
        public int CantidadVentas { get; set; }
        public decimal TotalVentas { get; set; }
        public decimal TicketPromedio { get; set; }
        public decimal TotalCosto { get; set; }
        public decimal UtilidadBruta { get; set; }
        public decimal MargenPorcentaje { get; set; }
    }

    public class ResumenVentasPorPeriodoDto
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int TotalVentas { get; set; }
        public decimal TotalIngresos { get; set; }
        public decimal TicketPromedio { get; set; }
        public decimal TotalCosto { get; set; }
        public decimal UtilidadBruta { get; set; }
        public decimal MargenPorcentaje { get; set; }
        public decimal CrecimientoVsPeriodoAnterior { get; set; }
        public List<VentasPorPeriodoDto> DetallesPorDia { get; set; } = new();
    }
}