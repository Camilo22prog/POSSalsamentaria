namespace POS.Application.DTOs.Security
{
    public class LoginRequestDto
    {
        public string NombreUsuario { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}