namespace POS.Application.DTOs.Reports
{
    public class VentasPorClienteDto
    {
        public int? ClienteId { get; set; }
        public string ClienteNombre { get; set; } = "Consumidor Final";
        public string NumeroDocumento { get; set; } = "";
        public int CantidadCompras { get; set; }
        public decimal TotalCompras { get; set; }
        public decimal TicketPromedio { get; set; }
        public DateTime? UltimaCompra { get; set; }
        public DateTime? PrimeraCompra { get; set; }
        public int DiasDesdeUltimaCompra { get; set; }
    }
}