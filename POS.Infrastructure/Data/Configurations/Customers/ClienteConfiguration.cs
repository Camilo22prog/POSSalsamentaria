using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Domain.Entities.Customers;

namespace POS.Infrastructure.Data.Configurations.Customers
{
    public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
    {
        public void Configure(EntityTypeBuilder<Cliente> builder)
        {
            builder.ToTable("Clientes");

            builder.HasKey(c => c.Id);

            // Identificación
            builder.Property(c => c.NumeroDocumento)
                .HasMaxLength(50)
                .IsRequired();

            builder.HasIndex(c => c.NumeroDocumento)
                .IsUnique();

            // Datos básicos
            builder.Property(c => c.NombreCompleto)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(c => c.RazonSocial)
                .HasMaxLength(200);

            builder.Property(c => c.NombreComercial)
                .HasMaxLength(200);

            // Ubicación
            builder.Property(c => c.Pais)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(c => c.Departamento)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(c => c.Ciudad)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(c => c.Direccion)
                .HasMaxLength(300)
                .IsRequired();

            // Contacto
            builder.Property(c => c.Email)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(c => c.Telefono)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(c => c.EmailSecundario)
                .HasMaxLength(200);

            builder.Property(c => c.TelefonoSecundario)
                .HasMaxLength(50);

            // Datos tributarios
            builder.Property(c => c.ActividadEconomica)
                .HasMaxLength(10);

            // Observaciones
            builder.Property(c => c.Observaciones)
                .HasMaxLength(500);

            // Auditoría
            builder.Property(c => c.FechaCreacion)
                .IsRequired();
        }
    }
}