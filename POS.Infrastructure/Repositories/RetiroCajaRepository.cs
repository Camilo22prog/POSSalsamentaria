using Microsoft.EntityFrameworkCore;
using POS.Domain.Entities.Sales;
using POS.Domain.Interfaces.Repositories;
using POS.Infrastructure.Data;

namespace POS.Infrastructure.Repositories
{
    public class RetiroCajaRepository : Repository<RetiroCaja>, IRetiroCajaRepository
    {
        public RetiroCajaRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<RetiroCaja>> ObtenerPorCajaAsync(int cajaId)
        {
            return await _dbSet
                .Where(r => r.CajaId == cajaId)
                .OrderByDescending(r => r.Fecha)
                .ToListAsync();
        }
    }
}