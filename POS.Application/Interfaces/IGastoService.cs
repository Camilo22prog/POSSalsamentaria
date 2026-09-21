using POS.Application.DTOs.Expenses;
using POS.Domain.Enums;

namespace POS.Application.Interfaces
{
    public interface IGastoService
    {
        Task<IEnumerable<GastoOperativoDto>> ObtenerTodosAsync();
        Task<IEnumerable<GastoOperativoDto>> ObtenerPorFechaAsync(DateTime inicio, DateTime fin);
        Task<IEnumerable<GastoOperativoDto>> ObtenerPorTipoAsync(TipoGasto tipo);
        Task<GastoOperativoDto?> ObtenerPorIdAsync(int id);
        Task<GastoOperativoDto> CrearAsync(CrearGastoOperativoDto dto);
        Task<GastoOperativoDto> ActualizarAsync(int id, CrearGastoOperativoDto dto);
        Task EliminarAsync(int id);
        Task<decimal> ObtenerTotalPorFechaAsync(DateTime inicio, DateTime fin);
    }
}
