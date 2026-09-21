using POS.Application.DTOs.Catalog;

namespace POS.Application.Interfaces
{
    public interface IProductoService
    {
        Task<IEnumerable<ProductoDto>> ObtenerTodosAsync();
        Task<IEnumerable<ProductoDto>> ObtenerActivosAsync();
        Task<IEnumerable<ProductoDto>> ObtenerPorCategoriaAsync(int categoriaId);
        Task<IEnumerable<ProductoDto>> ObtenerPorProveedorAsync(int proveedorId);
        Task<IEnumerable<ProductoDto>> ObtenerConStockBajoAsync();
        Task<IEnumerable<ProductoDto>> BuscarAsync(string termino);
        Task<ProductoDto?> ObtenerPorIdAsync(int id);
        Task<ProductoDto?> ObtenerPorCodigoAsync(string codigo);
        Task<ProductoDto> CrearAsync(CrearProductoDto dto, int creadoPorId);
        Task<ProductoDto> ActualizarAsync(int id, CrearProductoDto dto, int modificadoPorId);
        Task DesactivarAsync(int id, int modificadoPorId);
        Task ActivarAsync(int id, int modificadoPorId);
    }
}