using POS.Domain.Enums;

namespace POS.Application.DTOs.Catalog
{
    public class ProductoDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int CategoriaId { get; set; }
        public string CategoriaNombre { get; set; } = string.Empty;
        public string? CategoriaColor { get; set; }
        public TipoUnidad TipoUnidad { get; set; }
        public string TipoUnidadTexto { get; set; } = string.Empty;
        public decimal PrecioCompra { get; set; }
        public decimal PrecioVenta { get; set; }
        public TipoIVA TipoIVA { get; set; }
        public string TipoIVATexto { get; set; } = string.Empty;
        public decimal PorcentajeIVA { get; set; }
        public decimal StockActual { get; set; }
        public decimal StockMinimo { get; set; }
        public string? UnidadMedida { get; set; }
        public bool TieneStockBajo { get; set; }
        public bool ControlaVencimiento { get; set; }
        public bool PermiteFraccionado { get; set; }
        public EstadoRegistro Estado { get; set; }
        public string? ImagenUrl { get; set; }
        
        // ✅ NUEVOS CAMPOS PARA VENTA POR PESO
        public bool VentaPorPeso { get; set; }
        public CategoriaProductoPeso? CategoriaPeso { get; set; }
        public decimal PrecioPorKilo { get; set; }

        // Proveedor
        public int? ProveedorId { get; set; }
        public string? ProveedorNombre { get; set; }
    }
}