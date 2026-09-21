namespace POS.Application.DTOs.Reports
{
    public class FiltroReporteDto
    {
        public DateTime FechaInicio { get; set; } = DateTime.Now.Date;
        public DateTime FechaFin { get; set; } = DateTime.Now.Date;
        public int? CategoriaId { get; set; }
        public int? ProductoId { get; set; }
        public int? ClienteId { get; set; }
        public int? UsuarioId { get; set; }
        public int? CajaId { get; set; }
    }
}