using Microsoft.EntityFrameworkCore;
using POS.Domain.Entities.Sales;
using POS.Domain.Enums;
using POS.Domain.Interfaces.Repositories;
using POS.Infrastructure.Data;

namespace POS.Infrastructure.Repositories
{
    public class DetalleDenominacionRepository : Repository<DetalleDenominacion>, IDetalleDenominacionRepository
    {
        public DetalleDenominacionRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<DetalleDenominacion>> ObtenerPorCajaYTipoAsync(int cajaId, TipoArqueo tipoArqueo)
        {
            return await _dbSet
                .Where(d => d.CajaId == cajaId && d.TipoArqueo == tipoArqueo)
                .ToListAsync();
        }
    }
}