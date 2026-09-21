using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Domain.Entities.Catalog;

namespace POS.Infrastructure.Data.Configurations.Catalog
{
    public class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
    {
        public void Configure(EntityTypeBuilder<Categoria> builder)
        {
            builder.ToTable("Categorias");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Nombre)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.Descripcion)
                .HasMaxLength(250);

            builder.Property(c => c.Color)
                .HasMaxLength(20);

            // Índices
            builder.HasIndex(c => c.Nombre).IsUnique();

            // Datos semilla
            builder.HasData(
                new Categoria { Id = 1, Nombre = "Embutidos", Color = "#FF5722", FechaCreacion = DateTime.Now },
                new Categoria { Id = 2, Nombre = "Lácteos", Color = "#2196F3", FechaCreacion = DateTime.Now },
                new Categoria { Id = 3, Nombre = "Bebidas", Color = "#4CAF50", FechaCreacion = DateTime.Now },
                new Categoria { Id = 4, Nombre = "Snacks", Color = "#FFC107", FechaCreacion = DateTime.Now },
                new Categoria { Id = 5, Nombre = "Granos", Color = "#795548", FechaCreacion = DateTime.Now }
            );
        }
    }
}