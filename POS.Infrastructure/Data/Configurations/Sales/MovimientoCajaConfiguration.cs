using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Domain.Entities.Sales;

namespace POS.Infrastructure.Data.Configurations.Sales
{
    public class MovimientoCajaConfiguration : IEntityTypeConfiguration<MovimientoCaja>
    {
        public void Configure(EntityTypeBuilder<MovimientoCaja> builder)
        {
            builder.ToTable("MovimientosCaja");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.Monto)
                .HasPrecision(18, 2);

            builder.Property(m => m.Concepto)
                .HasMaxLength(500);

            builder.Property(m => m.Referencia)
                .HasMaxLength(100);

            // Relaciones
            builder.HasOne(m => m.Caja)
                .WithMany()
                .HasForeignKey(m => m.CajaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(m => m.Usuario)
                .WithMany()
                .HasForeignKey(m => m.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}