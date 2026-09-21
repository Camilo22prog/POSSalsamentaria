namespace POS.Domain.Entities.Security
{
    public class Auditoria
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string Accion { get; set; } = string.Empty; // LOGIN, VENTA, ANULACION, etc.
        public string? Tabla { get; set; }
        public int? RegistroId { get; set; }
        public string? ValoresAnteriores { get; set; } // JSON
        public string? ValoresNuevos { get; set; } // JSON
        public string? Motivo { get; set; } // Para autorizaciones
        public string? DireccionIP { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;
        
        // Navegación
        public virtual Usuario Usuario { get; set; } = null!;
    }
}