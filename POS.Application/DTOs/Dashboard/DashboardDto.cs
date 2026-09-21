namespace POS.Application.DTOs.Dashboard
{
    // ── Dashboard Principal ─────────────────────────────────────────────────────
    public class DashboardPrincipalDto
    {
        public decimal VentasHoy { get; set; }
        public int TransaccionesHoy { get; set; }
        public decimal TicketPromedio { get; set; }
        public decimal CrecimientoVsAyer { get; set; }
        public bool CajaAbierta { get; set; }
        public string? UsuarioCaja { get; set; }
        public decimal MontoCajaActual { get; set; }
        public DateTime? FechaAperturaCaja { get; set; }
        public int ProductosStockBajo { get; set; }
        public int VentasAnuladasHoy { get; set; }
        public List<TopProductoDto> TopProductos { get; set; } = new();
        public List<VentaDiariaDto> VentasSemanales { get; set; } = new();
        public decimal VentasSemana { get; set; }
        public decimal VentasMes { get; set; }
        public decimal CrecimientoSemanal { get; set; }
        public decimal CrecimientoMensual { get; set; }
    }

    public class TopProductoDto
    {
        public string Nombre { get; set; } = string.Empty;
        public int CantidadVendida { get; set; }
        public decimal Total { get; set; }
    }

    public class VentaDiariaDto
    {
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public int Transacciones { get; set; }
    }

    // ── Dashboard Ventas ────────────────────────────────────────────────────────
    public class DashboardVentasDto
    {
        public decimal VentasPeriodo { get; set; }
        public decimal VentasPeriodoAnterior { get; set; }
        public decimal CrecimientoPeriodo { get; set; }
        public int TransaccionesPeriodo { get; set; }
        public decimal TicketPromedioPeriodo { get; set; }
        public List<VentaDiariaDto> VentasTendencia { get; set; } = new();
        public List<VentasCategoriaDto> VentasPorCategoria { get; set; } = new();
        public List<MetodoPagoResumenDto> MetodosPago { get; set; } = new();
    }

    public class VentasCategoriaDto
    {
        public string NombreCategoria { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public decimal Porcentaje { get; set; }
        public int Transacciones { get; set; }
    }

    public class MetodoPagoResumenDto
    {
        public string MetodoPago { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public decimal Porcentaje { get; set; }
        public int Cantidad { get; set; }
    }

    // ── Dashboard Inventario ────────────────────────────────────────────────────
    public class DashboardInventarioDto
    {
        public int TotalProductos { get; set; }
        public int ProductosConStock { get; set; }
        public int ProductosSinStock { get; set; }
        public int ProductosStockBajo { get; set; }
        public int ProductosStockCritico { get; set; }
        public decimal ValorTotalInventario { get; set; }
        public List<StockCategoriaDto> StockPorCategoria { get; set; } = new();
        public List<ProductoCriticoDto> ProductosCriticos { get; set; } = new();
        public List<TopProductoDto> ProductosMasRotados { get; set; } = new();
    }

    public class StockCategoriaDto
    {
        public string NombreCategoria { get; set; } = string.Empty;
        public int TotalProductos { get; set; }
        public int UnidadesStock { get; set; }
        public decimal ValorTotal { get; set; }
    }

    public class ProductoCriticoDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
        public string EstadoStock { get; set; } = string.Empty;
    }

    // ── Dashboard Financiero ────────────────────────────────────────────────────
    public class DashboardFinancieroDto
    {
        public decimal IngresosDelMes { get; set; }
        public decimal CostoProductosVendidos { get; set; }
        public decimal ComprasDelMes { get; set; }
        public decimal MermasDelMes { get; set; }
        public decimal GastosOperativosDelMes { get; set; }
        public decimal UtilidadBruta { get; set; }
        public decimal MargenBruto { get; set; }
        public decimal UtilidadNeta { get; set; }
        public decimal VentasMesAnterior { get; set; }
        public decimal CrecimientoVsMesAnterior { get; set; }
        public decimal ProyeccionMes { get; set; }
        public List<VentaDiariaDto> VentasMensuales { get; set; } = new();
        public List<FlujoCajaDto> FlujoCaja { get; set; } = new();
    }

    public class FlujoCajaDto
    {
        public DateTime Fecha { get; set; }
        public decimal Ingresos { get; set; }
        public decimal Egresos { get; set; }
    }
}