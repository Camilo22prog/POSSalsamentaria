using POS.Domain.Enums;

namespace POS.Application.DTOs.Inventory
{
    public class MovimientoInventarioDto
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public string ProductoNombre { get; set; } = string.Empty;
        public string ProductoCodigo { get; set; } = string.Empty;
        public TipoMovimientoInventario Tipo { get; set; }
        public string TipoTexto { get; set; } = string.Empty;
        public decimal Cantidad { get; set; }
        public decimal StockAnterior { get; set; }
        public decimal StockNuevo { get; set; }
        public decimal? CostoUnitario { get; set; }
        public decimal? CostoTotal { get; set; }
        public string? Referencia { get; set; }
        public string? Motivo { get; set; }
        public int UsuarioId { get; set; }
        public string UsuarioNombre { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
    }
}