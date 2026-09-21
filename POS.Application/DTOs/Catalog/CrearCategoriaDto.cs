namespace POS.Application.DTOs.Catalog
{
    public class CrearCategoriaDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string? Color { get; set; }
    }
}