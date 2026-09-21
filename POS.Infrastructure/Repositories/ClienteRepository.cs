using Microsoft.EntityFrameworkCore;
using POS.Domain.Entities.Customers;
using POS.Domain.Interfaces.Repositories;
using POS.Infrastructure.Data;

namespace POS.Infrastructure.Repositories
{
    public class ClienteRepository : Repository<Cliente>, IClienteRepository
    {
        public ClienteRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Cliente?> ObtenerPorDocumentoAsync(string numeroDocumento)
        {
            return await _dbSet
                .Include(c => c.Ventas)
                .FirstOrDefaultAsync(c => c.NumeroDocumento == numeroDocumento);
        }

        public async Task<bool> ExisteDocumentoAsync(string numeroDocumento, int? clienteIdExcluir = null)
        {
            var query = _dbSet.Where(c => c.NumeroDocumento == numeroDocumento);
            
            if (clienteIdExcluir.HasValue)
            {
                query = query.Where(c => c.Id != clienteIdExcluir.Value);
            }
            
            return await query.AnyAsync();
        }

        public async Task<IEnumerable<Cliente>> BuscarAsync(string termino)
        {
            termino = termino.ToLower().Trim();
            
            return await _dbSet
                .Where(c => c.Activo &&
                    (c.NumeroDocumento.ToLower().Contains(termino) ||
                     c.NombreCompleto.ToLower().Contains(termino) ||
                     (c.RazonSocial != null && c.RazonSocial.ToLower().Contains(termino)) ||
                     (c.NombreComercial != null && c.NombreComercial.ToLower().Contains(termino)) ||
                     c.Email.ToLower().Contains(termino) ||
                     c.Telefono.Contains(termino)))
                .OrderBy(c => c.NombreCompleto)
                .Take(50)
                .ToListAsync();
        }
    }
}