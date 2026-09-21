using Microsoft.EntityFrameworkCore;
using POS.Domain.Entities.Catalog;
using POS.Domain.Enums;
using POS.Domain.Interfaces.Repositories;
using POS.Infrastructure.Data;

namespace POS.Infrastructure.Repositories
{
    public class ProductoRepository : Repository<Producto>, IProductoRepository
    {
        public ProductoRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Producto?> GetByCodigoAsync(string codigo)
        {
            return await _dbSet
                .Include(p => p.Categoria).Include(p => p.Proveedor)
                .FirstOrDefaultAsync(p => p.Codigo == codigo);
        }

        public async Task<Producto?> GetProductoConCategoriaAsync(int productoId)
        {
            return await _dbSet
                .Include(p => p.Categoria).Include(p => p.Proveedor)
                .FirstOrDefaultAsync(p => p.Id == productoId);
        }

        public async Task<IEnumerable<Producto>> GetProductosActivosAsync()
        {
            return await _dbSet
                .Include(p => p.Categoria).Include(p => p.Proveedor)
                .Where(p => p.Estado == EstadoRegistro.Activo)
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        public async Task<IEnumerable<Producto>> GetProductosPorCategoriaAsync(int categoriaId)
        {
            return await _dbSet
                .Include(p => p.Categoria).Include(p => p.Proveedor)
                .Where(p => p.CategoriaId == categoriaId && p.Estado == EstadoRegistro.Activo)
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        public async Task<IEnumerable<Producto>> GetProductosConStockBajoAsync()
        {
            return await _dbSet
                .Include(p => p.Categoria).Include(p => p.Proveedor)
                .Where(p => p.Estado == EstadoRegistro.Activo && p.StockActual <= p.StockMinimo)
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        public async Task<IEnumerable<Producto>> BuscarProductosAsync(string termino)
        {
            termino = termino.ToLower();
            
            return await _dbSet
                .Include(p => p.Categoria).Include(p => p.Proveedor)
                .Where(p => p.Estado == EstadoRegistro.Activo &&
                           (p.Nombre.ToLower().Contains(termino) ||
                            p.Codigo.ToLower().Contains(termino) ||
                            p.Descripcion!.ToLower().Contains(termino)))
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        public async Task<bool> ExisteCodigoAsync(string codigo, int? excludeId = null)
        {
            var query = _dbSet.Where(p => p.Codigo == codigo);

            if (excludeId.HasValue)
                query = query.Where(p => p.Id != excludeId.Value);

            return await query.AnyAsync();
        }

        public async Task<IEnumerable<Producto>> GetProductosPorProveedorAsync(int proveedorId)
        {
            return await _dbSet
                .Include(p => p.Categoria).Include(p => p.Proveedor)
                .Where(p => p.ProveedorId == proveedorId && p.Estado == EstadoRegistro.Activo)
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }
    }
}