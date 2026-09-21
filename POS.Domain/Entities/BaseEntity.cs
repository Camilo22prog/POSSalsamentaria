using POS.Domain.Enums;

namespace POS.Domain.Entities
{
    public abstract class BaseEntity
    {
        public int Id { get; set; }
        public EstadoRegistro Estado { get; set; } = EstadoRegistro.Activo;
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaModificacion { get; set; }
    }
}