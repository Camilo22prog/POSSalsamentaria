using POS.Domain.Enums;

namespace POS.Application.DTOs.Expenses
{
    public class GastoOperativoDto
    {
        public int Id { get; set; }
        public TipoGasto Tipo { get; set; }
        public string TipoNombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; }
        public string? Comprobante { get; set; }
        public string? Observaciones { get; set; }
        public string UsuarioNombre { get; set; } = string.Empty;
    }
}
