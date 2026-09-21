using POS.Domain.Enums;

namespace POS.Application.DTOs.Customers
{
    public class ClienteDto
    {
        public int Id { get; set; }
        public TipoCliente TipoCliente { get; set; }
        public TipoDocumento TipoDocumento { get; set; }
        public string NumeroDocumento { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string? RazonSocial { get; set; }
        public string? NombreComercial { get; set; }
        public string Pais { get; set; } = "Colombia";
        public string Departamento { get; set; } = string.Empty;
        public string Ciudad { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string? EmailSecundario { get; set; }
        public string? TelefonoSecundario { get; set; }
        public ResponsabilidadFiscal ResponsabilidadFiscal { get; set; }
        public RegimenTributario Regimen { get; set; }
        public string? ActividadEconomica { get; set; }
        public string? Observaciones { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
        
        // Datos calculados
        public int TotalCompras { get; set; }
        public decimal MontoTotalCompras { get; set; }
    }
}