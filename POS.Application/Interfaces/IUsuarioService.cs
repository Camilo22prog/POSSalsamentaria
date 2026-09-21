using POS.Application.DTOs.Security;

namespace POS.Application.Interfaces
{
    public interface IUsuarioService
    {
        Task<IEnumerable<UsuarioDto>> ObtenerTodosAsync();
        Task<IEnumerable<UsuarioDto>> ObtenerActivosAsync();
        Task<UsuarioDto?> ObtenerPorIdAsync(int id);
        Task<UsuarioDto> CrearAsync(CrearUsuarioDto dto, int creadoPorId);
        Task<UsuarioDto> ActualizarAsync(int id, CrearUsuarioDto dto, int modificadoPorId);
        Task DesactivarAsync(int id, int modificadoPorId);
        Task ActivarAsync(int id, int modificadoPorId);
        Task BloquearAsync(int id, int modificadoPorId);
        Task DesbloquearAsync(int id, int modificadoPorId);
        Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario, int? excluirId = null);
    }
}