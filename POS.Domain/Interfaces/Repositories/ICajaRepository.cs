using POS.Domain.Entities.Sales;

namespace POS.Domain.Interfaces.Repositories
{
    public interface ICajaRepository : IRepository<Caja>
    {
        Task<Caja?> GetCajaAbiertaAsync(int usuarioId);
        Task<Caja?> GetCajaActivaAsync();
        Task<bool> TieneCajaAbiertaAsync(int usuarioId);
        Task<IEnumerable<Caja>> ObtenerCajasPorRangoFechaAsync(DateTime fechaInicio, DateTime fechaFin);
        Task<IEnumerable<Caja>> GetCajasConVentasAsync(DateTime fechaInicio, DateTime fechaFin);
    }
}