using POS.Domain.Entities.Security;

namespace POS.Domain.Interfaces.Repositories
{
    public interface IAuditoriaRepository : IRepository<Auditoria>
    {
        Task<IEnumerable<Auditoria>> GetByUsuarioAsync(int usuarioId, DateTime? desde = null, DateTime? hasta = null);
        Task<IEnumerable<Auditoria>> GetByAccionAsync(string accion, DateTime? desde = null, DateTime? hasta = null);
        Task<IEnumerable<Auditoria>> GetByFechaAsync(DateTime desde, DateTime hasta);
    }
}