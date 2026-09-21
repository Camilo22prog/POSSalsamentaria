
using POS.Domain.Enums;
using POS.Domain.Entities.Sales;
namespace POS.Domain.Entities.Customers
{
    public class Cliente
    {
        public int Id { get; set; }
        
        // Tipo de cliente
        public TipoCliente TipoCliente { get; set; }
        
        // Identificación
        public TipoDocumento TipoDocumento { get; set; }
        public string NumeroDocumento { get; set; } = string.Empty;
        
        // Datos básicos
        public string NombreCompleto { get; set; } = string.Empty;  // Persona Natural
        public string? RazonSocial { get; set; }                     // Persona Jurídica
        public string? NombreComercial { get; set; }
        
        // Ubicación (OBLIGATORIO para facturación)
        public string Pais { get; set; } = "Colombia";
        public string Departamento { get; set; } = string.Empty;
        public string Ciudad { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        
        // Contacto (OBLIGATORIO)
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string? EmailSecundario { get; set; }
        public string? TelefonoSecundario { get; set; }
        
        // Datos tributarios
        public ResponsabilidadFiscal ResponsabilidadFiscal { get; set; }
        public RegimenTributario Regimen { get; set; }
        public string? ActividadEconomica { get; set; }  // Código CIIU
        
        // Datos adicionales
        public string? Observaciones { get; set; }
        public bool Activo { get; set; } = true;
        
        // Auditoría
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        
        // Navegación
        public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
    }
}