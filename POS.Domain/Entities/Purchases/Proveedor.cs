using POS.Domain.Enums;

namespace POS.Domain.Entities.Purchases
{
    public class Proveedor
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? NombreComercial { get; set; }
        public string? RUC { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public string? Direccion { get; set; }
        public string? PersonaContacto { get; set; }
        
        // Control
        public EstadoRegistro Estado { get; set; } = EstadoRegistro.Activo;
        
        // Auditoría
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaModificacion { get; set; }
        
        // Navegación
        public virtual ICollection<Compra> Compras { get; set; } = new List<Compra>();
    }
}