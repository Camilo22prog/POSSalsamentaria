using POS.Domain.Entities.Customers;

namespace POS.Domain.Interfaces.Repositories
{
    public interface IClienteRepository : IRepository<Cliente>
    {
        Task<Cliente?> ObtenerPorDocumentoAsync(string numeroDocumento);
        Task<bool> ExisteDocumentoAsync(string numeroDocumento, int? clienteIdExcluir = null);
        Task<IEnumerable<Cliente>> BuscarAsync(string termino);
    }
}