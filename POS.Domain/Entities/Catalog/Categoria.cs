using POS.Domain.Enums;

namespace POS.Domain.Entities.Catalog
{
    public class Categoria
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string? Color { get; set; } // Para UI
        public EstadoRegistro Estado { get; set; } = EstadoRegistro.Activo;
        
        // Auditoría
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaModificacion { get; set; }
        
        // Navegación
        public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
    }
}