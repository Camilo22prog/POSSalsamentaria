namespace POS.Application.DTOs.Customers
{
    public class ActualizarClienteDto : CrearClienteDto
    {
        public int Id { get; set; }
        public bool Activo { get; set; }
    }
}