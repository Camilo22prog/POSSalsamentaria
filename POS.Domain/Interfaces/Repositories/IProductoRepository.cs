using POS.Domain.Entities.Catalog;
using POS.Domain.Enums;

namespace POS.Domain.Interfaces.Repositories
{
    public interface IProductoRepository : IRepository<Producto>
    {
        Task<Producto?> GetByCodigoAsync(string codigo);
        Task<Producto?> GetProductoConCategoriaAsync(int productoId);
        Task<IEnumerable<Producto>> GetProductosActivosAsync();
        Task<IEnumerable<Producto>> GetProductosPorCategoriaAsync(int categoriaId);
        Task<IEnumerable<Producto>> GetProductosConStockBajoAsync();
        Task<IEnumerable<Producto>> BuscarProductosAsync(string termino);
        Task<bool> ExisteCodigoAsync(string codigo, int? excludeId = null);
        Task<IEnumerable<Producto>> GetProductosPorProveedorAsync(int proveedorId);
    }
}