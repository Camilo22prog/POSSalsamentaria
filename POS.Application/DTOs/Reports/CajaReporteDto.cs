namespace POS.Application.DTOs.Reports
{
    public class CajaReporteDto
    {
        public int CajaId { get; set; }
        public DateTime FechaApertura { get; set; }
        public DateTime? FechaCierre { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public decimal MontoApertura { get; set; }
        public decimal TotalVentas { get; set; }
        public decimal TotalRetiros { get; set; }
        public decimal EfectivoEsperado { get; set; }
        public decimal EfectivoContado { get; set; }
        public decimal Diferencia { get; set; }
        public string Estado { get; set; } = string.Empty;
    }

    public class ResumenCajaDto
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int TotalArqueos { get; set; }
        public decimal TotalAperturas { get; set; }
        public decimal TotalVentas { get; set; }
        public decimal TotalRetiros { get; set; }
        public decimal TotalDiferencias { get; set; }
        public decimal PromedioVentasPorCaja { get; set; }
        public List<CajaReporteDto> Detalles { get; set; } = new();
    }

    public class ArqueoCajaDto
    {
        public int ArqueoId { get; set; }
        public DateTime FechaApertura { get; set; }
        public DateTime? FechaCierre { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public decimal MontoInicial { get; set; }
        public decimal TotalVentas { get; set; }
        public decimal TotalEfectivo { get; set; }
        public decimal TotalTarjeta { get; set; }
        public decimal TotalTransferencia { get; set; }
        public decimal MontoEsperado { get; set; }
        public decimal MontoContado { get; set; }
        public decimal Diferencia { get; set; }
        public string Estado { get; set; } = string.Empty;
        public int NumeroVentas { get; set; }
        public decimal VentaPromedio { get; set; }
        
        // Detalles de billetes y monedas
        public Dictionary<string, int> DesgloseBilletes { get; set; } = new();
    }
}