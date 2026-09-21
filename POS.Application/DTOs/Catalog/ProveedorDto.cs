using POS.Domain.Enums;

namespace POS.Application.DTOs.Catalog
{
    public class ProveedorDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? NombreComercial { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public string? Direccion { get; set; }
        public string? PersonaContacto { get; set; }
        public EstadoRegistro Estado { get; set; }
    }

    public class CrearProveedorDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string? NombreComercial { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public string? Direccion { get; set; }
        public string? PersonaContacto { get; set; }
    }
}
