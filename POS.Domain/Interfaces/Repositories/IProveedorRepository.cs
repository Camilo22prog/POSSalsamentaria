using POS.Domain.Entities.Purchases;

namespace POS.Domain.Interfaces.Repositories
{
    public interface IProveedorRepository : IRepository<Proveedor>
    {
        Task<IEnumerable<Proveedor>> GetProveedoresActivosAsync();
        Task<bool> ExisteNombreAsync(string nombre, int? excludeId = null);
    }
}
