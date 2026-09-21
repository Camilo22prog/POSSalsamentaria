using POS.Domain.Entities.Sales;

namespace POS.Domain.Interfaces.Repositories
{
    public interface IVentaRepository : IRepository<Venta>
    {
        Task<Venta?> GetVentaCompletaAsync(int ventaId);
        Task<IEnumerable<Venta>> GetVentasPorCajaAsync(int cajaId);
        Task<IEnumerable<Venta>> GetVentasPorFechaAsync(DateTime fecha);
        Task<string> GenerarNumeroVentaAsync();
        Task<int> ContarVentasDelDiaAsync();
        Task<IEnumerable<Venta>> GetVentasPorFechaRangoAsync(DateTime fechaInicio, DateTime fechaFin);
    }
}