using POS.Domain.Enums; 

namespace POS.Domain.Entities.Sales
{
    public class DetalleDenominacion
    {
        public int Id { get; set; }
        public int CajaId { get; set; }
        public TipoDenominacion Tipo { get; set; }
        public int Valor { get; set; }
        public int Cantidad { get; set; }
        public decimal Total { get; set; }
        public TipoArqueo TipoArqueo { get; set; }
        public DateTime Fecha { get; set; }

        // Navegación
        public Caja Caja { get; set; } = null!;
    }
}