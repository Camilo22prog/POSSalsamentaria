using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Domain.Entities.Security;

namespace POS.Infrastructure.Data.Configurations.Security
{
    public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            builder.ToTable("Usuarios");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.NombreUsuario)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.NombreCompleto)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(u => u.Telefono)
                .HasMaxLength(20);

            // ✅ Rol es un enum, se guarda como int automáticamente
            builder.Property(u => u.Rol)
                .IsRequired()
                .HasConversion<int>(); // Convertir enum a int en la BD

            builder.Property(u => u.Estado)
                .IsRequired()
                .HasConversion<int>();

            // Índices
            builder.HasIndex(u => u.NombreUsuario)
                .IsUnique();

            builder.HasIndex(u => u.Email)
                .IsUnique();

            // Relaciones (si las tienes)
            builder.HasMany(u => u.Ventas)
                .WithOne(v => v.Usuario)
                .HasForeignKey(v => v.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(u => u.Cajas)
                .WithOne(c => c.Usuario)
                .HasForeignKey(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(u => u.Auditorias)
                .WithOne(a => a.Usuario)
                .HasForeignKey(a => a.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}