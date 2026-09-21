using POS.Application.DTOs.Sales;

namespace POS.Application.Interfaces
{
    public interface IVentaService
    {
        // Crear venta
        Task<VentaDto> CrearVentaAsync(CrearVentaDto dto, int usuarioId, int cajaId);
        
        // Consultar ventas
        Task<VentaDto?> ObtenerVentaPorIdAsync(int ventaId);
        Task<IEnumerable<VentaDto>> ObtenerVentasPorCajaAsync(int cajaId);
        Task<IEnumerable<VentaDto>> ObtenerVentasDelDiaAsync();
        Task<IEnumerable<VentaDto>> ObtenerVentasPorRangoAsync(DateTime fechaInicio, DateTime fechaFin);
        
        // Anular venta
        Task<bool> AnularVentaAsync(int ventaId, string motivo, int usuarioId);
        
        // Estadísticas
        Task<decimal> ObtenerTotalVentasDelDiaAsync();
        Task<int> ContarVentasDelDiaAsync();
    }
}