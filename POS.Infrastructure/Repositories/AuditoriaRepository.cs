using Microsoft.EntityFrameworkCore;
using POS.Domain.Entities.Security;
using POS.Domain.Interfaces.Repositories;
using POS.Infrastructure.Data;

namespace POS.Infrastructure.Repositories
{
    public class AuditoriaRepository : Repository<Auditoria>, IAuditoriaRepository
    {
        public AuditoriaRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Auditoria>> GetByUsuarioAsync(int usuarioId, DateTime? desde = null, DateTime? hasta = null)
        {
            var query = _dbSet
                .Include(a => a.Usuario)
                .Where(a => a.UsuarioId == usuarioId);

            if (desde.HasValue)
                query = query.Where(a => a.Fecha >= desde.Value);

            if (hasta.HasValue)
                query = query.Where(a => a.Fecha <= hasta.Value);

            return await query
                .OrderByDescending(a => a.Fecha)
                .ToListAsync();
        }

        public async Task<IEnumerable<Auditoria>> GetByAccionAsync(string accion, DateTime? desde = null, DateTime? hasta = null)
        {
            var query = _dbSet
                .Include(a => a.Usuario)
                .Where(a => a.Accion == accion);

            if (desde.HasValue)
                query = query.Where(a => a.Fecha >= desde.Value);

            if (hasta.HasValue)
                query = query.Where(a => a.Fecha <= hasta.Value);

            return await query
                .OrderByDescending(a => a.Fecha)
                .ToListAsync();
        }

        public async Task<IEnumerable<Auditoria>> GetByFechaAsync(DateTime desde, DateTime hasta)
        {
            return await _dbSet
                .Include(a => a.Usuario)
                .Where(a => a.Fecha >= desde && a.Fecha <= hasta)
                .OrderByDescending(a => a.Fecha)
                .ToListAsync();
        }
    }
}