using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Domain.Entities.Sales;

namespace POS.Infrastructure.Data.Configurations.Sales
{
    public class DetalleDenominacionConfiguration : IEntityTypeConfiguration<DetalleDenominacion>
    {
        public void Configure(EntityTypeBuilder<DetalleDenominacion> builder)
        {
            builder.ToTable("DetallesDenominaciones");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.Total)
                .HasColumnType("decimal(18,2)");

            builder.Property(d => d.Fecha)
                .IsRequired();

            // Relación con Caja
            builder.HasOne(d => d.Caja)
                .WithMany(c => c.DetallesDenominaciones)
                .HasForeignKey(d => d.CajaId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}