namespace POS.Application.DTOs.Sales
{
    public class RetiroCajaDto
    {
        public int Id { get; set; }
        public int CajaId { get; set; }
        public decimal Monto { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public string? Observaciones { get; set; }
        public int UsuarioId { get; set; }
        public string UsuarioNombre { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
    }
}