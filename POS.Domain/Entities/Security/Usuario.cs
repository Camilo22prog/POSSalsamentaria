using POS.Domain.Enums;
using POS.Domain.Entities.Sales;  

namespace POS.Domain.Entities.Security
{
    public class Usuario
    {
        public int Id { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        
        // ✅ NUEVO: Rol como Enum
        public TipoRol Rol { get; set; } = TipoRol.Cajero;
        
        // Control
        public EstadoRegistro Estado { get; set; } = EstadoRegistro.Activo;
        public DateTime? UltimoAcceso { get; set; }
        public int IntentosFailidos { get; set; } = 0;
        public bool Bloqueado { get; set; } = false;
        
        // Auditoría
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaModificacion { get; set; }
        public int? CreadoPorId { get; set; }
        public int? ModificadoPorId { get; set; }
        
        // Navegación
        public virtual ICollection<Venta> Ventas { get; set; } = new List<Venta>();
        public virtual ICollection<Caja> Cajas { get; set; } = new List<Caja>();
        public virtual ICollection<Auditoria> Auditorias { get; set; } = new List<Auditoria>();
    }
}