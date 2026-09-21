using POS.Domain.Entities.Expenses;
using POS.Domain.Entities.Inventory;
using POS.Domain.Entities.Settings;
using POS.Domain.Entities.Purchases;
using POS.Domain.Entities.Sales;

namespace POS.Domain.Interfaces.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        // Seguridad
        IUsuarioRepository Usuarios { get; }
        IRolRepository Roles { get; }
        IAuditoriaRepository Auditorias { get; }

        // Catálogo
        IProductoRepository Productos { get; }
        ICategoriaRepository Categorias { get; }

        // Inventario
        IRepository<MovimientoInventario> MovimientosInventario { get; }
        IRepository<Merma> Mermas { get; }
        IRepository<Lote> Lotes { get; }

        // Compras
        ICompraRepository Compras { get; }

        // Gastos Operativos
        IRepository<GastoOperativo> Gastos { get; }

        // Configuración
        IRepository<Configuracion> Configuracion { get; }

        // Ventas - AGREGAR
        IVentaRepository Ventas { get; }
        ICajaRepository Cajas { get; }

        IDetalleDenominacionRepository DetallesDenominaciones { get; }
        IRetiroCajaRepository RetirosCaja { get; }

        IClienteRepository Clientes { get; }
        IProveedorRepository Proveedores { get; }

        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}