using Microsoft.EntityFrameworkCore.Storage;
using POS.Domain.Interfaces.Repositories;
using POS.Domain.Entities.Expenses;
using POS.Domain.Entities.Inventory;
using POS.Domain.Entities.Settings;
using POS.Domain.Entities.Purchases;
using POS.Infrastructure.Data;

namespace POS.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;

        // Seguridad
        public IUsuarioRepository Usuarios { get; }
        public IRolRepository Roles { get; }
        public IAuditoriaRepository Auditorias { get; }
        
        // Catálogo
        public IProductoRepository Productos { get; }
        public ICategoriaRepository Categorias { get; }
        
        // Inventario
        public IRepository<MovimientoInventario> MovimientosInventario { get; }
        public IRepository<Merma> Mermas { get; }
        public IRepository<Lote> Lotes { get; }

        // Compras
        public ICompraRepository Compras { get; }

        // Gastos Operativos
        public IRepository<GastoOperativo> Gastos { get; }

        // Configuración
        public IRepository<Configuracion> Configuracion { get; }

        // Ventas - AGREGAR
        public IVentaRepository Ventas { get; }
        public ICajaRepository Cajas { get; }
        public IDetalleDenominacionRepository DetallesDenominaciones { get; }
        public IRetiroCajaRepository RetirosCaja { get; }

        public IClienteRepository Clientes { get; }
        public IProveedorRepository Proveedores { get; }

        public UnitOfWork(ApplicationDbContext context)

        {
            _context = context;
            
            // Seguridad
            Usuarios = new UsuarioRepository(_context);
            Roles = new RolRepository(_context);
            Auditorias = new AuditoriaRepository(_context);
            
            // Catálogo
            Productos = new ProductoRepository(_context);
            Categorias = new CategoriaRepository(_context);
            
            // Inventario
            MovimientosInventario = new Repository<MovimientoInventario>(_context);
            Mermas = new Repository<Merma>(_context);
            Lotes = new Repository<Lote>(_context);

            // Compras
            Compras = new CompraRepository(_context);

            // Gastos Operativos
            Gastos = new Repository<GastoOperativo>(_context);

            // Configuración
            Configuracion = new Repository<Configuracion>(_context);

            // Ventas - AGREGAR
            Ventas = new VentaRepository(_context);
            Cajas = new CajaRepository(_context);
            DetallesDenominaciones = new DetalleDenominacionRepository(context);
            RetirosCaja = new RetiroCajaRepository(context);
            Clientes = new ClienteRepository(context);
            Proveedores = new ProveedorRepository(context);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
        

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                if (_transaction != null)
                {
                    await _transaction.CommitAsync();
                }
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }



        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}