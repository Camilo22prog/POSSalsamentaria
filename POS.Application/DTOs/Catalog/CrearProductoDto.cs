using POS.Domain.Enums;

namespace POS.Application.DTOs.Catalog
{
    public class CrearProductoDto
    {
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        
        public int CategoriaId { get; set; }
        public TipoUnidad TipoUnidad { get; set; }
        
        public decimal PrecioCompra { get; set; }
        public decimal PrecioVenta { get; set; }
        // Ventas por peso
        public bool VentaPorPeso { get; set; }
        public CategoriaProductoPeso? CategoriaPeso { get; set; }
        public decimal PrecioPorKilo { get; set; }
        
        // IVA
        public TipoIVA TipoIVA { get; set; } = TipoIVA.Diecinueve;
        
        public decimal StockMinimo { get; set; } = 0;
        public string? UnidadMedida { get; set; }
        
        public bool ControlaVencimiento { get; set; } = false;
        public int? DiasAlertaVencimiento { get; set; } = 7;
        public bool PermiteFraccionado { get; set; } = false;
        
        public string? ImagenUrl { get; set; }

        // Proveedor
        public int? ProveedorId { get; set; }
    }
}