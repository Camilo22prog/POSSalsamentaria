using POS.Domain.Entities.Catalog;

namespace POS.Domain.Interfaces.Repositories
{
    public interface ICategoriaRepository : IRepository<Categoria>
    {
        Task<Categoria?> GetByNombreAsync(string nombre);
        Task<IEnumerable<Categoria>> GetCategoriasActivasAsync();
        Task<bool> ExisteNombreAsync(string nombre, int? excludeId = null);
    }
}