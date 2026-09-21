using Microsoft.EntityFrameworkCore;
using POS.Domain.Entities.Sales;
using POS.Domain.Interfaces.Repositories;
using POS.Infrastructure.Data;

namespace POS.Infrastructure.Repositories
{
    public class VentaRepository : Repository<Venta>, IVentaRepository
    {
        public VentaRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Venta?> GetVentaCompletaAsync(int ventaId)
        {
            return await _dbSet
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                .Include(v => v.Pagos)
                .Include(v => v.Usuario)
                .Include(v => v.Cliente)
                .FirstOrDefaultAsync(v => v.Id == ventaId);
        }

        public async Task<IEnumerable<Venta>> GetVentasPorCajaAsync(int cajaId)
        {
            return await _dbSet
                .Include(v => v.Detalles)
                .Include(v => v.Pagos)
                .Where(v => v.CajaId == cajaId && !v.Anulada)
                .OrderBy(v => v.Fecha)
                .ToListAsync();
        }

        public async Task<IEnumerable<Venta>> GetVentasPorFechaAsync(DateTime fecha)
        {
            var inicio = fecha.Date;
            var fin = fecha.Date.AddDays(1);

            return await _dbSet
                .Include(v => v.Detalles)
                .Include(v => v.Pagos)
                .Where(v => v.Fecha >= inicio && v.Fecha < fin && !v.Anulada)
                .OrderBy(v => v.Fecha)
                .ToListAsync();
        }

        public async Task<string> GenerarNumeroVentaAsync()
        {
            var hoy = DateTime.Now.Date;
            var contador = await ContarVentasDelDiaAsync() + 1;
            
            return $"VTA-{hoy:yyyyMMdd}-{contador:D3}";
        }

        public async Task<int> ContarVentasDelDiaAsync()
        {
            var hoy = DateTime.Now.Date;
            var manana = hoy.AddDays(1);

            return await _dbSet
                .CountAsync(v => v.Fecha >= hoy && v.Fecha < manana);
        }

        public async Task<IEnumerable<Venta>> GetVentasPorFechaRangoAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            return await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.Usuario)
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                        .ThenInclude(p => p.Categoria)
                .Include(v => v.Pagos)
                .Where(v => v.Fecha.Date >= fechaInicio.Date && v.Fecha.Date <= fechaFin.Date)
                .OrderByDescending(v => v.Fecha)
                .ToListAsync();
        }
    }
}