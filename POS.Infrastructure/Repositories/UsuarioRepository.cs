using Microsoft.EntityFrameworkCore;
using POS.Domain.Entities.Security;
using POS.Domain.Enums;
using POS.Domain.Interfaces.Repositories;
using POS.Infrastructure.Data;

namespace POS.Infrastructure.Repositories
{
    public class UsuarioRepository : Repository<Usuario>, IUsuarioRepository
    {
        public UsuarioRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Usuario?> GetByNombreUsuarioAsync(string nombreUsuario)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario);
        }

        public async Task<Usuario?> GetByEmailAsync(string email)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<Usuario?> GetUsuarioConRolAsync(int usuarioId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.Id == usuarioId);
        }

        public async Task<IEnumerable<Usuario>> GetUsuariosActivosAsync()
        {
            return await _dbSet
                .Where(u => u.Estado == EstadoRegistro.Activo)
                .OrderBy(u => u.NombreCompleto)
                .ToListAsync();
        }

        public async Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario, int? excludeId = null)
        {
            var query = _dbSet.Where(u => u.NombreUsuario == nombreUsuario);
            
            if (excludeId.HasValue)
                query = query.Where(u => u.Id != excludeId.Value);
            
            return await query.AnyAsync();
        }

        public async Task<bool> ExisteEmailAsync(string email, int? excludeId = null)
        {
            var query = _dbSet.Where(u => u.Email == email);
            
            if (excludeId.HasValue)
                query = query.Where(u => u.Id != excludeId.Value);
            
            return await query.AnyAsync();
        }
    }
}