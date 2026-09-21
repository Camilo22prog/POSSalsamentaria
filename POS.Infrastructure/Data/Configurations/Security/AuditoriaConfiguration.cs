using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Domain.Entities.Security;

namespace POS.Infrastructure.Data.Configurations.Security
{
    public class AuditoriaConfiguration : IEntityTypeConfiguration<Auditoria>
    {
        public void Configure(EntityTypeBuilder<Auditoria> builder)
        {
            builder.ToTable("Auditorias");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.Accion)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(a => a.Tabla)
                .HasMaxLength(50);

            builder.Property(a => a.DireccionIP)
                .HasMaxLength(45);

            // Relaciones
            builder.HasOne(a => a.Usuario)
                .WithMany(u => u.Auditorias)
                .HasForeignKey(a => a.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            // Índices
            builder.HasIndex(a => a.Fecha);
            builder.HasIndex(a => a.UsuarioId);
        }
    }
}