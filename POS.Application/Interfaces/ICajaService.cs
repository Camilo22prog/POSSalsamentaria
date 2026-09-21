using POS.Application.DTOs.Sales;

namespace POS.Application.Interfaces
{
    public interface ICajaService
    {
        // Abrir/Cerrar caja
        Task<CajaDto> AbrirCajaAsync(AbrirCajaDto dto, int usuarioId);
        Task<CajaDto> CerrarCajaAsync(int cajaId, CerrarCajaDto dto);
        
        // Consultar
        Task<CajaDto?> ObtenerCajaActivaAsync();
        Task<CajaDto?> ObtenerCajaAbiertaPorUsuarioAsync(int usuarioId);
        Task<bool> TieneCajaAbiertaAsync(int usuarioId);
        
        // Estadísticas
        Task<CajaDto> ObtenerResumenCajaAsync(int cajaId);
        Task<CajaDto> AbrirCajaConDenominacionesAsync(AbrirCajaConDenominacionesDto dto, int usuarioId);
        Task<CajaDto> CerrarCajaConArqueoAsync(int cajaId, CerrarCajaConArqueoDto dto);
        Task<RetiroCajaDto> RegistrarRetiroAsync(int cajaId, CrearRetiroDto dto, int usuarioId);
        Task<List<RetiroCajaDto>> ObtenerRetirosCajaAsync(int cajaId);
        Task<List<DetalleDenominacionDto>> ObtenerDenominacionesAperturaAsync(int cajaId);
    }
}