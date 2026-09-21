using POS.Application.DTOs.Settings;

namespace POS.Application.Interfaces
{
    public interface IConfiguracionService
    {
        Task<ConfiguracionDto> ObtenerAsync();
        Task GuardarAsync(ConfiguracionDto dto);
    }
}
