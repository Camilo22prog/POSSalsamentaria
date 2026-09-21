using Microsoft.EntityFrameworkCore;
using POS.Domain.Entities.Purchases;
using POS.Domain.Enums;
using POS.Domain.Interfaces.Repositories;
using POS.Infrastructure.Data;

namespace POS.Infrastructure.Repositories
{
    public class ProveedorRepository : Repository<Proveedor>, IProveedorRepository
    {
        public ProveedorRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Proveedor>> GetProveedoresActivosAsync()
        {
            return await _context.Proveedores
                .Where(p => p.Estado == EstadoRegistro.Activo)
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        public async Task<bool> ExisteNombreAsync(string nombre, int? excludeId = null)
        {
            var query = _context.Proveedores.Where(p => p.Nombre == nombre);
            if (excludeId.HasValue)
                query = query.Where(p => p.Id != excludeId.Value);
            return await query.AnyAsync();
        }
    }
}
