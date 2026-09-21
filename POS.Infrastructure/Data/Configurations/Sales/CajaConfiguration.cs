using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Domain.Entities.Sales;

namespace POS.Infrastructure.Data.Configurations.Sales
{
    public class CajaConfiguration : IEntityTypeConfiguration<Caja>
    {
        public void Configure(EntityTypeBuilder<Caja> builder)
        {
            builder.ToTable("Cajas");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.MontoInicial)
                .HasPrecision(18, 2);

            builder.Property(c => c.MontoFinal)
                .HasPrecision(18, 2);

            builder.Property(c => c.Diferencia)
                .HasPrecision(18, 2);

            builder.Property(c => c.TotalEfectivo)
                .HasPrecision(18, 2);

            builder.Property(c => c.TotalTarjeta)
                .HasPrecision(18, 2);

            builder.Property(c => c.TotalNequi)
                .HasPrecision(18, 2);

            builder.Property(c => c.TotalDaviplata)
                .HasPrecision(18, 2);

            builder.Property(c => c.TotalTransferencia)
                .HasPrecision(18, 2);

            builder.Property(c => c.TotalQR)
                .HasPrecision(18, 2);

            // Relaciones
            builder.HasOne(c => c.Usuario)
                .WithMany(u => u.Cajas)
                .HasForeignKey(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(c => c.Ventas)
                .WithOne(v => v.Caja)
                .HasForeignKey(v => v.CajaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(c => c.EfectivoEsperado)
                .HasColumnType("decimal(18,2)");

            builder.Property(c => c.EfectivoContado)
                .HasColumnType("decimal(18,2)");

            builder.Property(c => c.DiferenciaEfectivo)
                .HasColumnType("decimal(18,2)");

            builder.Property(c => c.TotalRetiros)
                .HasColumnType("decimal(18,2)");
        }
    }
}