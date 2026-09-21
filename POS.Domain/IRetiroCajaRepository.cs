using POS.Domain.Entities.Sales;

namespace POS.Domain.Interfaces.Repositories
{
    public interface IRetiroCajaRepository : IRepository<RetiroCaja>
    {
        Task<List<RetiroCaja>> ObtenerPorCajaAsync(int cajaId);
    }
}