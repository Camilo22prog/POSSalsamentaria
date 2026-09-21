using POS.Domain.Entities.Sales;
using POS.Domain.Enums;

namespace POS.Domain.Interfaces.Repositories
{
    public interface IDetalleDenominacionRepository : IRepository<DetalleDenominacion>
    {
        Task<List<DetalleDenominacion>> ObtenerPorCajaYTipoAsync(int cajaId, TipoArqueo tipoArqueo);
    }
}