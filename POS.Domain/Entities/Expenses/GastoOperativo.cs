using POS.Domain.Entities.Security;
using POS.Domain.Enums;

namespace POS.Domain.Entities.Expenses
{
    public class GastoOperativo
    {
        public int Id { get; set; }

        public TipoGasto Tipo { get; set; }

        public string Descripcion { get; set; } = string.Empty;

        public decimal Monto { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        public string? Comprobante { get; set; }

        public string? Observaciones { get; set; }

        public int UsuarioId { get; set; }
        public virtual Usuario Usuario { get; set; } = null!;

        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }
}
