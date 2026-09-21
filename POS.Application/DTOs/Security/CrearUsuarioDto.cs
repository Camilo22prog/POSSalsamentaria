using POS.Domain.Enums;

namespace POS.Application.DTOs.Security
{
    public class CrearUsuarioDto
    {
        public string NombreUsuario { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        
        // ✅ Usar TipoRol en lugar de RolId
        public TipoRol Rol { get; set; } = TipoRol.Cajero;
    }
}