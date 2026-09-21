using POS.Application.DTOs.Reports;

using POS.Domain.Enums;
using POS.Application.DTOs.Dashboard;

namespace POS.Application.Interfaces
{
    public interface IReporteService
    {
        // VENTAS
        Task<ResumenVentasPorPeriodoDto> ObtenerVentasPorPeriodoAsync(FiltroReporteDto filtro);
        Task<List<VentasPorProductoDto>> ObtenerVentasPorProductoAsync(FiltroReporteDto filtro);
        Task<List<VentasPorClienteDto>> ObtenerVentasPorClienteAsync(FiltroReporteDto filtro);
        Task<List<VentasPorMetodoPagoDto>> ObtenerVentasPorMetodoPagoAsync(FiltroReporteDto filtro);
        Task<ResumenDesempenoDto> ObtenerDesempenoUsuariosAsync(FiltroReporteDto filtro);
        Task<ResumenArqueosDto> ObtenerReporteArqueosAsync(FiltroReporteDto filtro);
        Task<ResumenVentasAnuladasDto> ObtenerVentasAnuladasAsync(FiltroReporteDto filtro);
        // FINANCIERO
        Task<EstadoResultadosDto> ObtenerEstadoResultadosAsync(FiltroReporteDto filtro);

        // INVENTARIO
        Task<ResumenInventarioDto> ObtenerReporteInventarioAsync();
        Task<ResumenABCDto> ObtenerAnalisisABCAsync(FiltroReporteDto filtro);


        // CAJA
        Task<ResumenCajaDto> ObtenerReporteCajaAsync(FiltroReporteDto filtro);

        // Dashboard
        Task<DashboardPrincipalDto> ObtenerDashboardPrincipalAsync();
        Task<DashboardVentasDto> ObtenerDashboardVentasAsync(string periodo);
        Task<DashboardInventarioDto> ObtenerDashboardInventarioAsync();
        Task<DashboardFinancieroDto> ObtenerDashboardFinancieroAsync();
        // EXPORTACIÓN
        Task<byte[]> ExportarVentasPorPeriodoAsync(FiltroReporteDto filtro);
        Task<byte[]> ExportarVentasPorProductoAsync(FiltroReporteDto filtro);
        Task<byte[]> ExportarEstadoResultadosAsync(FiltroReporteDto filtro);
        Task<byte[]> ExportarVentasPorClienteAsync(FiltroReporteDto filtro);
        Task<byte[]> ExportarVentasPorMetodoPagoGeneralAsync(FiltroReporteDto filtro);
        Task<byte[]> ExportarVentasPorMetodoPagoDetalladoAsync(FiltroReporteDto filtro, MetodoPago metodo);
        Task<byte[]> ExportarInventarioAsync();
        Task<byte[]> ExportarAnalisisABCAsync(FiltroReporteDto filtro);
        Task<byte[]> ExportarDesempenoUsuariosAsync(FiltroReporteDto filtro);
        Task<byte[]> ExportarReporteArqueosAsync(FiltroReporteDto filtro);
        Task<byte[]> ExportarVentasAnuladasAsync(FiltroReporteDto filtro);
    }
}