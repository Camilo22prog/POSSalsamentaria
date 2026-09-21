using Microsoft.EntityFrameworkCore;
using POS.Domain.Entities.Security;
using POS.Domain.Enums;
using POS.Domain.Interfaces.Repositories;
using POS.Infrastructure.Data;

namespace POS.Infrastructure.Repositories
{
    public class RolRepository : Repository<Rol>, IRolRepository
    {
        public RolRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Rol?> GetByNombreAsync(string nombre)
        {
            return await _dbSet
                .FirstOrDefaultAsync(r => r.Nombre == nombre);
        }

        public async Task<IEnumerable<Rol>> GetRolesActivosAsync()
        {
            return await _dbSet
                .Where(r => r.Estado == EstadoRegistro.Activo)
                .OrderBy(r => r.Nombre)
                .ToListAsync();
        }
    }
}