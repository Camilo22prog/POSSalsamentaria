using POS.Domain.Entities.Security;
namespace POS.Domain.Entities.Sales

{

    public class RetiroCaja
    {
        public int Id { get; set; }
        public int CajaId { get; set; }
        public decimal Monto { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public string? Observaciones { get; set; }
        public int UsuarioId { get; set; }
        public DateTime Fecha { get; set; }

        // Navegación
        public Caja Caja { get; set; } = null!;
        public Usuario Usuario { get; set; } = null!;
    }
}