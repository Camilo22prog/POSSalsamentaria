using POS.Domain.Entities.Security;

namespace POS.Domain.Interfaces.Repositories
{
    public interface IRolRepository : IRepository<Rol>
    {
        Task<Rol?> GetByNombreAsync(string nombre);
        Task<IEnumerable<Rol>> GetRolesActivosAsync();
    }
}