using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Domain.Entities.Sales;

namespace POS.Infrastructure.Data.Configurations.Sales
{
    public class DetalleVentaConfiguration : IEntityTypeConfiguration<DetalleVenta>
    {
        public void Configure(EntityTypeBuilder<DetalleVenta> builder)
        {
            builder.ToTable("DetallesVenta");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.Cantidad)
                .HasPrecision(18, 3);

            builder.Property(d => d.PrecioUnitario)
                .HasPrecision(18, 2);

            builder.Property(d => d.Descuento)
                .HasPrecision(18, 2);

            builder.Property(d => d.PorcentajeIVA)
                .HasPrecision(5, 2);

            builder.Property(d => d.MontoIVA)
                .HasPrecision(18, 2);

            builder.Property(d => d.Subtotal)
                .HasPrecision(18, 2);

            builder.Property(d => d.Total)
                .HasPrecision(18, 2);

            builder.Property(d => d.CostoUnitario)
                .HasPrecision(18, 2);

            // Relaciones
            builder.HasOne(d => d.Venta)
                .WithMany(v => v.Detalles)
                .HasForeignKey(d => d.VentaId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(d => d.Producto)
                .WithMany()
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}