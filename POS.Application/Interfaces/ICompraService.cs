using POS.Application.DTOs.Purchases;

namespace POS.Application.Interfaces
{
    public interface ICompraService
    {
        Task<CompraDto> RegistrarCompraAsync(CrearCompraDto dto, int usuarioId);
        Task<IEnumerable<CompraDto>> ObtenerPorRangoAsync(DateTime inicio, DateTime fin);
        Task<CompraDto?> ObtenerPorIdAsync(int id);
        Task AnularCompraAsync(int id, string motivo, int usuarioId);
        Task MarcarPagadaAsync(int id);
    }
}
