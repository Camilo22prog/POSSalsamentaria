using POS.Application.DTOs.Catalog;

namespace POS.Application.Interfaces
{
    public interface ICategoriaService
    {
        Task<IEnumerable<CategoriaDto>> ObtenerTodasAsync();
        Task<IEnumerable<CategoriaDto>> ObtenerActivasAsync();
        Task<CategoriaDto?> ObtenerPorIdAsync(int id);
        Task<CategoriaDto> CrearAsync(CrearCategoriaDto dto, int creadoPorId);
        Task<CategoriaDto> ActualizarAsync(int id, CrearCategoriaDto dto, int modificadoPorId);
        Task DesactivarAsync(int id, int modificadoPorId);
        Task ActivarAsync(int id, int modificadoPorId);
    }
}