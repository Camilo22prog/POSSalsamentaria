using Microsoft.EntityFrameworkCore;
using POS.Domain.Entities.Catalog;
using POS.Domain.Enums;
using POS.Domain.Interfaces.Repositories;
using POS.Infrastructure.Data;

namespace POS.Infrastructure.Repositories
{
    public class CategoriaRepository : Repository<Categoria>, ICategoriaRepository
    {
        public CategoriaRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Categoria?> GetByNombreAsync(string nombre)
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => c.Nombre == nombre);
        }

        public async Task<IEnumerable<Categoria>> GetCategoriasActivasAsync()
        {
            return await _dbSet
                .Where(c => c.Estado == EstadoRegistro.Activo)
                .OrderBy(c => c.Nombre)
                .ToListAsync();
        }

        public async Task<bool> ExisteNombreAsync(string nombre, int? excludeId = null)
        {
            var query = _dbSet.Where(c => c.Nombre == nombre);
            
            if (excludeId.HasValue)
                query = query.Where(c => c.Id != excludeId.Value);
            
            return await query.AnyAsync();
        }
    }
}