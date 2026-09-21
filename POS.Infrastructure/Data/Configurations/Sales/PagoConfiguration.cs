using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Domain.Entities.Sales;

namespace POS.Infrastructure.Data.Configurations.Sales
{
    public class PagoConfiguration : IEntityTypeConfiguration<Pago>
    {
        public void Configure(EntityTypeBuilder<Pago> builder)
        {
            builder.ToTable("Pagos");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Referencia)
                .HasMaxLength(100);

            builder.Property(p => p.NumeroAutorizacion)
                .HasMaxLength(100);

            // Relaciones
            builder.HasOne(p => p.Venta)
                .WithMany(v => v.Pagos)
                .HasForeignKey(p => p.VentaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Índices
            builder.HasIndex(p => p.Fecha);
        }
    }
}