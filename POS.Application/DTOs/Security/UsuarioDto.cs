using POS.Domain.Enums;

namespace POS.Application.DTOs.Security
{
    public class UsuarioDto
    {
        public int Id { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        
        // ✅ Usar TipoRol en lugar de RolId
        public TipoRol Rol { get; set; }
        public string RolNombre { get; set; } = string.Empty;
        
        public EstadoRegistro Estado { get; set; }
        public DateTime? UltimoAcceso { get; set; }
        public bool Bloqueado { get; set; }
        
        // Permisos calculados según el rol
        public bool PuedeGestionarUsuarios { get; set; }
        public bool PuedeGestionarProductos { get; set; }
        public bool PuedeAjustarInventario { get; set; }
        public bool PuedeAnularVentas { get; set; }
        public bool PuedeVerReportes { get; set; }
        public bool PuedeVerCostos { get; set; }
        public bool PuedeAplicarDescuentos { get; set; }
        public decimal? LimiteDescuento { get; set; }
    }
}