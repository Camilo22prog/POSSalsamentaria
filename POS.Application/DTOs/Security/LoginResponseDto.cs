namespace POS.Application.DTOs.Security
{
    public class LoginResponseDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public UsuarioDto? Usuario { get; set; }
    }
}