using Microsoft.EntityFrameworkCore;
using POS.Domain.Entities.Security;
using POS.Domain.Entities.Catalog;
using POS.Domain.Entities.Expenses;
using POS.Domain.Entities.Inventory;
using POS.Domain.Entities.Sales;
using POS.Domain.Entities.Purchases;
using POS.Domain.Entities.Settings;
using POS.Infrastructure.Data.Configurations.Sales;
using POS.Infrastructure.Data.Configurations.Customers;
using POS.Domain.Entities.Customers;

namespace POS.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets - Seguridad
        // ❌ ELIMINADO: public DbSet<Rol> Roles { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Auditoria> Auditorias { get; set; }

        // DbSets - Catálogo
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Producto> Productos { get; set; }

        // DbSets - Inventario
        public DbSet<Lote> Lotes { get; set; }
        public DbSet<MovimientoInventario> MovimientosInventario { get; set; }
        public DbSet<Merma> Mermas { get; set; }

        // DbSets - Ventas
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<DetalleVenta> DetallesVenta { get; set; }
        public DbSet<Pago> Pagos { get; set; }
        public DbSet<Caja> Cajas { get; set; }
        public DbSet<MovimientoCaja> MovimientosCaja { get; set; }

        // DbSets - Compras
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<Compra> Compras { get; set; }
        public DbSet<DetalleCompra> DetallesCompra { get; set; }

        // DbSets - Gastos Operativos
        public DbSet<GastoOperativo> GastosOperativos { get; set; }

        // DbSets - Configuración
        public DbSet<Configuracion> Configuraciones { get; set; }
        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración global para todos los decimales
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var properties = entityType.ClrType.GetProperties()
                    .Where(p => p.PropertyType == typeof(decimal) || p.PropertyType == typeof(decimal?));

                foreach (var property in properties)
                {
                    modelBuilder.Entity(entityType.Name)
                        .Property(property.Name)
                        .HasColumnType("decimal(18,2)");
                }
            }

            // Configuracion es una tabla de fila única — el Id nunca es auto-generado
            modelBuilder.Entity<Configuracion>()
                .Property(c => c.Id)
                .ValueGeneratedNever();

            // Aplicar configuraciones específicas
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
            modelBuilder.ApplyConfiguration(new DetalleDenominacionConfiguration());
            modelBuilder.ApplyConfiguration(new RetiroCajaConfiguration());
            modelBuilder.ApplyConfiguration(new ClienteConfiguration());
        }
    }
}