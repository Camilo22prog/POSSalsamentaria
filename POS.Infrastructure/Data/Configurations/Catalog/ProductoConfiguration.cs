using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Domain.Entities.Catalog;

namespace POS.Infrastructure.Data.Configurations.Catalog
{
    public class ProductoConfiguration : IEntityTypeConfiguration<Producto>
    {
        public void Configure(EntityTypeBuilder<Producto> builder)
        {
            builder.ToTable("Productos");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Codigo)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(p => p.Nombre)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.Descripcion)
                .HasMaxLength(500);

            builder.Property(p => p.UnidadMedida)
                .HasMaxLength(20);

            builder.Property(p => p.ImagenUrl)
                .HasMaxLength(500);

            // Decimales
            builder.Property(p => p.PrecioCompra)
                .HasColumnType("decimal(18,2)");

            builder.Property(p => p.PrecioVenta)
                .HasColumnType("decimal(18,2)");

            builder.Property(p => p.StockActual)
                .HasColumnType("decimal(18,3)");

            builder.Property(p => p.StockMinimo)
                .HasColumnType("decimal(18,3)");

            builder.Property(p => p.PesoPromedio)
                .HasColumnType("decimal(18,3)");

            // Relaciones
            builder.HasOne(p => p.Categoria)
                .WithMany(c => c.Productos)
                .HasForeignKey(p => p.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Índices
            builder.HasIndex(p => p.Codigo).IsUnique();
            builder.HasIndex(p => p.Nombre);
        }
    }
}