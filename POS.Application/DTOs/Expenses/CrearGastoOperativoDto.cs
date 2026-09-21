using POS.Domain.Enums;

namespace POS.Application.DTOs.Expenses
{
    public class CrearGastoOperativoDto
    {
        public TipoGasto Tipo { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;
        public string? Comprobante { get; set; }
        public string? Observaciones { get; set; }
        public int UsuarioId { get; set; }
    }
}
