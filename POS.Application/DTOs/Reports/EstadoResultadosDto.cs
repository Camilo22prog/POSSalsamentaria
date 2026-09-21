namespace POS.Application.DTOs.Reports
{
    public class EstadoResultadosDto
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }

        // ── INGRESOS ────────────────────────────────────────────────────────────
        // Suma de todas las ventas completadas (no anuladas)
        public decimal VentasBrutas { get; set; }

        // Valor total de ventas anuladas en el período
        public decimal Devoluciones { get; set; }

        // Descuentos aplicados sobre ventas no anuladas
        public decimal Descuentos { get; set; }

        // VentasNetas = VentasBrutas - Devoluciones - Descuentos
        public decimal VentasNetas { get; set; }

        // ── COSTO DE VENTAS (método perpetuo) ───────────────────────────────────
        // Costo directo de cada artículo vendido (CostoUnitario × Cantidad)
        public decimal CostoProductosVendidos { get; set; }

        // Mermas registradas en el período (vencidos, daños, robos)
        public decimal MermasDelPeriodo { get; set; }

        // TotalCostoVentas = CostoProductosVendidos + MermasDelPeriodo
        public decimal TotalCostoVentas { get; set; }

        // ── UTILIDAD BRUTA ───────────────────────────────────────────────────────
        // UtilidadBruta = VentasNetas - TotalCostoVentas
        public decimal UtilidadBruta { get; set; }
        public decimal MargenBrutoPorcentaje { get; set; }

        // ── GASTOS OPERATIVOS ────────────────────────────────────────────────────
        // Gastos de administración, servicios, sueldos, arriendo, etc.
        // (pendiente de implementar módulo de gastos)
        public decimal GastosOperativos { get; set; }

        // ── UTILIDAD OPERACIONAL ─────────────────────────────────────────────────
        // UtilidadOperacional = UtilidadBruta - GastosOperativos
        public decimal UtilidadOperacional { get; set; }
        public decimal MargenOperacionalPorcentaje { get; set; }

        // ── UTILIDAD NETA ────────────────────────────────────────────────────────
        // UtilidadNeta = UtilidadOperacional  (se separa para futuros impuestos)
        public decimal UtilidadNeta { get; set; }
        public decimal MargenNetoPorcentaje { get; set; }

        // ── DATOS COMPLEMENTARIOS ────────────────────────────────────────────────
        // Total de compras a proveedores registradas en el período
        public decimal ComprasDelPeriodo { get; set; }

        // Valor del inventario en existencia al momento de generar el reporte
        public decimal ValorInventarioActual { get; set; }
    }
}
