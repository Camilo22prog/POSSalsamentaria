using POS.Domain.Entities.Purchases;

namespace POS.Domain.Interfaces.Repositories
{
    public interface ICompraRepository : IRepository<Compra>
    {
        Task<string> GenerarNumeroCompraAsync();
        Task<IEnumerable<Compra>> GetComprasPorRangoAsync(DateTime inicio, DateTime fin);
        Task<Compra?> GetCompraConDetallesAsync(int compraId);
    }
}
