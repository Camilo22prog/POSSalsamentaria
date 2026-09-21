using Microsoft.EntityFrameworkCore;
using POS.Domain.Entities.Sales;
using POS.Domain.Interfaces.Repositories;
using POS.Infrastructure.Data;

namespace POS.Infrastructure.Repositories
{
    public class CajaRepository : Repository<Caja>, ICajaRepository
    {
        public CajaRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Caja?> GetCajaAbiertaAsync(int usuarioId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId && c.Abierta);
        }

        public async Task<Caja?> GetCajaActivaAsync()
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => c.Abierta);
        }

        public async Task<bool> TieneCajaAbiertaAsync(int usuarioId)
        {
            return await _dbSet
                .AnyAsync(c => c.UsuarioId == usuarioId && c.Abierta);
        }
        public async Task<IEnumerable<Caja>> ObtenerCajasPorRangoFechaAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            return await _context.Cajas
                .Include(c => c.Usuario)
                .Include(c => c.Ventas)
                .Include(c => c.Retiros)
                .Where(c => c.FechaApertura.Date >= fechaInicio.Date && 
                            c.FechaApertura.Date <= fechaFin.Date)
                .OrderByDescending(c => c.FechaApertura)
                .ToListAsync();
        }
        public async Task<IEnumerable<Caja>> GetCajasConVentasAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            return await _context.Cajas
                .Include(c => c.Usuario)
                .Include(c => c.Ventas.Where(v => !v.Anulada))  // Solo ventas no anuladas
                .Where(c => c.FechaApertura.Date >= fechaInicio.Date 
                         && c.FechaApertura.Date <= fechaFin.Date)
                .OrderByDescending(c => c.FechaApertura)
                .AsNoTracking()  // Mejor rendimiento para reportes
                .ToListAsync();
        }
    }
}