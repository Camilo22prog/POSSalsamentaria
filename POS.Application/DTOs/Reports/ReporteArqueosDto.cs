namespace POS.Application.DTOs.Reports
{
    public class ResumenArqueosDto
    {
        public int TotalArqueos { get; set; }
        public decimal TotalIngresos { get; set; }
        public decimal TotalEgresos { get; set; }
        public decimal SaldoNeto { get; set; }
        public decimal DiferenciaTotal { get; set; }
        public List<ArqueoDetalleDto> Arqueos { get; set; } = new();
    }

    public class ArqueoDetalleDto
    {
        public int ArqueoId { get; set; }
        public DateTime FechaApertura { get; set; }
        public DateTime? FechaCierre { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public decimal MontoInicial { get; set; }
        public decimal TotalVentas { get; set; }
        public decimal TotalEfectivo { get; set; }
        public decimal TotalTarjeta { get; set; }
        public decimal TotalTransferencia { get; set; }
        public decimal MontoEsperado { get; set; }
        public decimal MontoContado { get; set; }
        public decimal Diferencia { get; set; }
        public string EstadoCaja { get; set; } = string.Empty;
        public int NumeroVentas { get; set; }
        public decimal VentaPromedio { get; set; }
        public string TipoDiferencia => Diferencia == 0 ? "Cuadrado" 
        : Diferencia > 0 ? "Excedente" 
        : "Faltante";
    }
}