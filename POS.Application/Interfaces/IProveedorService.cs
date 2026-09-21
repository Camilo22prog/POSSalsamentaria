using POS.Application.DTOs.Catalog;

namespace POS.Application.Interfaces
{
    public interface IProveedorService
    {
        Task<IEnumerable<ProveedorDto>> ObtenerTodosAsync();
        Task<IEnumerable<ProveedorDto>> ObtenerActivosAsync();
        Task<ProveedorDto?> ObtenerPorIdAsync(int id);
        Task<ProveedorDto> CrearAsync(CrearProveedorDto dto);
        Task<ProveedorDto> ActualizarAsync(int id, CrearProveedorDto dto);
        Task DesactivarAsync(int id);
        Task ActivarAsync(int id);
    }
}
