using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Domain.Entities.Sales;
using POS.Domain.Entities.Customers;

namespace POS.Infrastructure.Data.Configurations.Sales
{
    public class VentaConfiguration : IEntityTypeConfiguration<Venta>
    {
        public void Configure(EntityTypeBuilder<Venta> builder)
        {
            builder.ToTable("Ventas");

            builder.HasKey(v => v.Id);

            builder.Property(v => v.NumeroVenta)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(v => v.MotivoAnulacion)
                .HasMaxLength(500);

            // Relaciones
            builder.HasOne(v => v.Cliente)
                .WithMany(c => c.Ventas)
                .HasForeignKey(v => v.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(v => v.Usuario)
                .WithMany(u => u.Ventas)
                .HasForeignKey(v => v.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(v => v.Caja)
                .WithMany(c => c.Ventas)
                .HasForeignKey(v => v.CajaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Índices
            builder.HasIndex(v => v.NumeroVenta).IsUnique();
            builder.HasIndex(v => v.Fecha);
        }
    }
}