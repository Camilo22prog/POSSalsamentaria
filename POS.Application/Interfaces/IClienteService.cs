using POS.Application.DTOs.Customers;

namespace POS.Application.Interfaces
{
    public interface IClienteService
    {
        Task<ClienteDto> CrearAsync(CrearClienteDto dto);
        Task<ClienteDto> ActualizarAsync(ActualizarClienteDto dto);
        Task<bool> EliminarAsync(int id);
        Task<ClienteDto?> ObtenerPorIdAsync(int id);
        Task<ClienteDto?> ObtenerPorDocumentoAsync(string numeroDocumento);
        Task<IEnumerable<ClienteDto>> ObtenerTodosAsync();
        Task<IEnumerable<ClienteDto>> BuscarAsync(string termino);
        Task<bool> ExisteDocumentoAsync(string numeroDocumento, int? clienteIdExcluir = null);
    }
}