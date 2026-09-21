using POS.Application.DTOs.Security;

namespace POS.Application.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
        Task<bool> CambiarPasswordAsync(CambiarPasswordDto request);
        Task RegistrarAuditoriaAsync(int usuarioId, string accion, string? motivo = null);
        Task<(bool Success, string? NombreSupervisor, string? Error)> ValidarSupervisorAsync(string nombreUsuario, string password);
    }
}