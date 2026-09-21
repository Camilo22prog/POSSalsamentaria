using POS.Domain.Entities.Security;
using POS.Domain.Enums;

namespace POS.Domain.Entities.Sales
{
    public class MovimientoCaja
    {
        public int Id { get; set; }
        public int CajaId { get; set; }
        public Caja Caja { get; set; } = null!;
        
        public TipoMovimientoCaja Tipo { get; set; }
        public decimal Monto { get; set; }
        public string Concepto { get; set; } = string.Empty;
        public string? Referencia { get; set; }  // AGREGAR
        
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;
        
        public DateTime Fecha { get; set; }
    }
}