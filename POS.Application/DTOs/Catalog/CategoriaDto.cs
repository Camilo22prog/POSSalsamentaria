using POS.Domain.Enums;

namespace POS.Application.DTOs.Catalog
{
    public class CategoriaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string? Color { get; set; }
        public EstadoRegistro Estado { get; set; }
    }
}