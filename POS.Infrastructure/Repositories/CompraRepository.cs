using Microsoft.EntityFrameworkCore;
using POS.Domain.Entities.Purchases;
using POS.Domain.Interfaces.Repositories;
using POS.Infrastructure.Data;

namespace POS.Infrastructure.Repositories
{
    public class CompraRepository : Repository<Compra>, ICompraRepository
    {
        public CompraRepository(ApplicationDbContext context) : base(context) { }

        public async Task<string> GenerarNumeroCompraAsync()
        {
            var hoy = DateTime.Today;
            var count = await _context.Compras
                .CountAsync(c => c.FechaCompra.Date == hoy);
            return $"COMP-{hoy:yyyyMMdd}-{count + 1:D3}";
        }

        public async Task<IEnumerable<Compra>> GetComprasPorRangoAsync(DateTime inicio, DateTime fin)
        {
            return await _context.Compras
                .Include(c => c.Proveedor)
                .Include(c => c.Usuario)
                .Include(c => c.DetallesCompra)
                    .ThenInclude(d => d.Producto)
                .Where(c => c.FechaCompra >= inicio && c.FechaCompra <= fin)
                .OrderByDescending(c => c.FechaCompra)
                .ToListAsync();
        }

        public async Task<Compra?> GetCompraConDetallesAsync(int compraId)
        {
            return await _context.Compras
                .Include(c => c.Proveedor)
                .Include(c => c.Usuario)
                .Include(c => c.DetallesCompra)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(c => c.Id == compraId);
        }
    }
}
