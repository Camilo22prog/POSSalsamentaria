using POS.Domain.Enums;

namespace POS.Domain.Entities.Security
{
    public class Rol
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public EstadoRegistro Estado { get; set; } = EstadoRegistro.Activo;
        
        // Permisos
        public bool PuedeGestionarUsuarios { get; set; }
        public bool PuedeGestionarProductos { get; set; }
        public bool PuedeAjustarInventario { get; set; }
        public bool PuedeAnularVentas { get; set; }
        public bool PuedeVerReportes { get; set; }
        public bool PuedeVerCostos { get; set; }
        public bool PuedeAplicarDescuentos { get; set; }
        public decimal? LimiteDescuento { get; set; } // % máximo de descuento
        
        // Auditoría
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaModificacion { get; set; }
        
        // Navegación
        public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    }
}