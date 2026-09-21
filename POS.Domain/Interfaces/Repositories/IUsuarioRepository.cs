using POS.Domain.Entities.Security;

namespace POS.Domain.Interfaces.Repositories
{
    public interface IUsuarioRepository : IRepository<Usuario>
    {
        Task<Usuario?> GetByNombreUsuarioAsync(string nombreUsuario);
        Task<Usuario?> GetByEmailAsync(string email);
        Task<Usuario?> GetUsuarioConRolAsync(int usuarioId);
        Task<IEnumerable<Usuario>> GetUsuariosActivosAsync();
        Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario, int? excludeId = null);
        Task<bool> ExisteEmailAsync(string email, int? excludeId = null);
    }
}