using POS.Application.DTOs.Settings;
using POS.Application.Interfaces;
using POS.Domain.Entities.Settings;
using POS.Domain.Interfaces.Repositories;

namespace POS.Application.Services
{
    public class ConfiguracionService : IConfiguracionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ConfiguracionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ConfiguracionDto> ObtenerAsync()
        {
            var config = await _unitOfWork.Configuracion.GetByIdAsync(1);

            // Si no existe, crear con valores por defecto
            if (config == null)
            {
                config = new Configuracion { Id = 1 };
                await _unitOfWork.Configuracion.AddAsync(config);
                await _unitOfWork.SaveChangesAsync();
            }

            return MapToDto(config);
        }

        public async Task GuardarAsync(ConfiguracionDto dto)
        {
            var config = await _unitOfWork.Configuracion.GetByIdAsync(1);

            if (config == null)
            {
                config = new Configuracion { Id = 1 };
                await _unitOfWork.Configuracion.AddAsync(config);
            }

            config.NombreNegocio       = dto.NombreNegocio.Trim();
            config.Nit                 = Limpio(dto.Nit);
            config.Direccion           = Limpio(dto.Direccion);
            config.Telefono            = Limpio(dto.Telefono);
            config.MensajePieTicket    = dto.MensajePieTicket.Trim();
            config.NombreImpresora     = Limpio(dto.NombreImpresora);
            config.AbrirCajonAutomatico = dto.AbrirCajonAutomatico;
            config.PuertoBalanza       = Limpio(dto.PuertoBalanza);
            config.BaudRateBalanza     = dto.BaudRateBalanza;
            config.UrlActualizaciones  = Limpio(dto.UrlActualizaciones);
            config.FechaModificacion   = DateTime.Now;

            await _unitOfWork.SaveChangesAsync();
        }

        private static ConfiguracionDto MapToDto(Configuracion c) => new()
        {
            NombreNegocio       = c.NombreNegocio,
            Nit                 = c.Nit,
            Direccion           = c.Direccion,
            Telefono            = c.Telefono,
            MensajePieTicket    = c.MensajePieTicket,
            NombreImpresora     = c.NombreImpresora,
            AbrirCajonAutomatico = c.AbrirCajonAutomatico,
            PuertoBalanza       = c.PuertoBalanza,
            BaudRateBalanza     = c.BaudRateBalanza,
            UrlActualizaciones  = c.UrlActualizaciones,
        };

        private static string? Limpio(string? s) =>
            string.IsNullOrWhiteSpace(s) ? null : s.Trim();
    }
}
