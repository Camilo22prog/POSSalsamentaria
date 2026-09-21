using POS.Domain.Enums;
using POS.Domain.Entities.Inventory;
using POS.Domain.Entities.Purchases;

namespace POS.Domain.Entities.Catalog
{
    public class Producto
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        
        // Categoría
        public int CategoriaId { get; set; }
        public virtual Categoria Categoria { get; set; } = null!;

        // Proveedor (opcional)
        public int? ProveedorId { get; set; }
        public virtual Proveedor? Proveedor { get; set; }
        
        // Tipo de venta
        public TipoUnidad TipoUnidad { get; set; }
        
        // Precios
        public decimal PrecioCompra { get; set; }
        public decimal PrecioVenta { get; set; }
        
        // IVA - NUEVO CAMPO
        public TipoIVA TipoIVA { get; set; } = TipoIVA.Diecinueve;
        
        // Inventario
        public decimal StockActual { get; set; } = 0;
        public decimal StockMinimo { get; set; } = 0;
        public string? UnidadMedida { get; set; }
        
        // Peso (para productos que se venden por peso)
        public decimal? PesoPromedio { get; set; }
        
        // Control de vencimiento
        public bool ControlaVencimiento { get; set; } = false;
        public int? DiasAlertaVencimiento { get; set; } = 7;

        public bool VentaPorPeso { get; set; }
        public CategoriaProductoPeso? CategoriaPeso { get; set; }
        public decimal PrecioPorKilo { get; set; }
        
        // Imagen
        public string? ImagenUrl { get; set; }
        
        // Control
        public bool PermiteFraccionado { get; set; } = false;
        public EstadoRegistro Estado { get; set; } = EstadoRegistro.Activo;
        
        // Auditoría
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaModificacion { get; set; }
        
        // Navegación
        public virtual ICollection<Lote> Lotes { get; set; } = new List<Lote>();
        public virtual ICollection<MovimientoInventario> MovimientosInventario { get; set; } = new List<MovimientoInventario>();
    }
}