using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Domain.Entities.Sales;

namespace POS.Infrastructure.Data.Configurations.Sales
{
    public class RetiroCajaConfiguration : IEntityTypeConfiguration<RetiroCaja>
    {
        public void Configure(EntityTypeBuilder<RetiroCaja> builder)
        {
            builder.ToTable("RetirosCaja");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Monto)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(r => r.Motivo)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(r => r.Observaciones)
                .HasMaxLength(500);

            builder.Property(r => r.Fecha)
                .IsRequired();

            // Relación con Caja
            builder.HasOne(r => r.Caja)
                .WithMany(c => c.Retiros)
                .HasForeignKey(r => r.CajaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación con Usuario
            builder.HasOne(r => r.Usuario)
                .WithMany()
                .HasForeignKey(r => r.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}