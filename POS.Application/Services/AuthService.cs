using POS.Application.DTOs.Security;
using POS.Application.Interfaces;
using POS.Domain.Entities.Security;
using POS.Domain.Enums;
using POS.Domain.Interfaces.Repositories;
using System.Security.Cryptography;
using System.Text;

namespace POS.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AuthService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
        {
            // Validar datos de entrada
            if (string.IsNullOrWhiteSpace(request.NombreUsuario) || 
                string.IsNullOrWhiteSpace(request.Password))
            {
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Usuario y contraseña son requeridos"
                };
            }

            // Buscar usuario
            var usuario = await _unitOfWork.Usuarios.GetByNombreUsuarioAsync(request.NombreUsuario);

            if (usuario == null)
            {
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Usuario o contraseña incorrectos"
                };
            }

            // Verificar si está bloqueado
            if (usuario.Bloqueado)
            {
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Usuario bloqueado. Contacte al administrador"
                };
            }

            // Verificar si está inactivo
            if (usuario.Estado != EstadoRegistro.Activo)
            {
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Usuario inactivo. Contacte al administrador"
                };
            }

            // Verificar contraseña
            var passwordHash = HashPassword(request.Password);
            if (usuario.PasswordHash != passwordHash)
            {
                // Incrementar intentos fallidos
                usuario.IntentosFailidos++;
                
                // Bloquear después de 3 intentos
                if (usuario.IntentosFailidos >= 3)
                {
                    usuario.Bloqueado = true;
                }
                
                await _unitOfWork.SaveChangesAsync();

                return new LoginResponseDto
                {
                    Success = false,
                    Message = usuario.Bloqueado 
                        ? "Usuario bloqueado por múltiples intentos fallidos" 
                        : "Usuario o contraseña incorrectos"
                };
            }

            // Login exitoso - resetear intentos fallidos
            usuario.IntentosFailidos = 0;
            usuario.UltimoAcceso = DateTime.Now;
            await _unitOfWork.SaveChangesAsync();

            // Registrar auditoría
            await RegistrarAuditoriaAsync(usuario.Id, "LOGIN");

            return new LoginResponseDto
            {
                Success = true,
                Message = "Login exitoso",
                Usuario = MapToDto(usuario)
            };
        }

        public async Task<bool> CambiarPasswordAsync(CambiarPasswordDto request)
        {
            var usuario = await _unitOfWork.Usuarios.GetByIdAsync(request.UsuarioId);
            
            if (usuario == null)
                return false;

            // Verificar password actual
            var passwordActualHash = HashPassword(request.PasswordActual);
            if (usuario.PasswordHash != passwordActualHash)
                return false;

            // Verificar que los passwords coincidan
            if (request.PasswordNuevo != request.ConfirmarPassword)
                return false;

            // Actualizar password
            usuario.PasswordHash = HashPassword(request.PasswordNuevo);
            usuario.FechaModificacion = DateTime.Now;
            
            await _unitOfWork.SaveChangesAsync();

            // Registrar auditoría
            await RegistrarAuditoriaAsync(usuario.Id, "CAMBIO_PASSWORD");

            return true;
        }

        public async Task<(bool Success, string? NombreSupervisor, string? Error)> ValidarSupervisorAsync(string nombreUsuario, string password)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario) || string.IsNullOrWhiteSpace(password))
                return (false, null, "Ingrese usuario y contraseña del supervisor.");

            var usuario = await _unitOfWork.Usuarios.GetByNombreUsuarioAsync(nombreUsuario);
            if (usuario == null)
                return (false, null, "Usuario no encontrado.");

            if (usuario.Bloqueado || usuario.Estado != Domain.Enums.EstadoRegistro.Activo)
                return (false, null, "Usuario bloqueado o inactivo.");

            var hash = HashPassword(password);
            if (usuario.PasswordHash != hash)
                return (false, null, "Contraseña incorrecta.");

            if (usuario.Rol != Domain.Enums.TipoRol.Administrador && usuario.Rol != Domain.Enums.TipoRol.Supervisor)
                return (false, null, "El usuario no tiene permisos de supervisor o administrador.");

            await RegistrarAuditoriaAsync(usuario.Id, "AUTORIZA_RETIRO_CAJA");
            return (true, usuario.NombreCompleto, null);
        }

        public async Task RegistrarAuditoriaAsync(int usuarioId, string accion, string? motivo = null)
        {
            var auditoria = new Auditoria
            {
                UsuarioId = usuarioId,
                Accion = accion,
                Motivo = motivo,
                Fecha = DateTime.Now
            };

            await _unitOfWork.Auditorias.AddAsync(auditoria);
            await _unitOfWork.SaveChangesAsync();
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        private UsuarioDto MapToDto(Usuario usuario)
        {
            var permisos = ObtenerPermisosPorRol(usuario.Rol);

            return new UsuarioDto
            {
                Id = usuario.Id,
                NombreUsuario = usuario.NombreUsuario,
                NombreCompleto = usuario.NombreCompleto,
                Email = usuario.Email,
                Telefono = usuario.Telefono,
                Rol = usuario.Rol,
                RolNombre = ObtenerNombreRol(usuario.Rol),
                Estado = usuario.Estado,
                UltimoAcceso = usuario.UltimoAcceso,
                Bloqueado = usuario.Bloqueado,
                PuedeGestionarUsuarios = permisos.PuedeGestionarUsuarios,
                PuedeGestionarProductos = permisos.PuedeGestionarProductos,
                PuedeAjustarInventario = permisos.PuedeAjustarInventario,
                PuedeAnularVentas = permisos.PuedeAnularVentas,
                PuedeVerReportes = permisos.PuedeVerReportes,
                PuedeVerCostos = permisos.PuedeVerCostos,
                PuedeAplicarDescuentos = permisos.PuedeAplicarDescuentos,
                LimiteDescuento = permisos.LimiteDescuento
            };
        }

        private string ObtenerNombreRol(TipoRol rol)
        {
            return rol switch
            {
                TipoRol.Administrador => "Administrador",
                TipoRol.Supervisor => "Supervisor",
                TipoRol.Cajero => "Cajero",
                _ => "Sin rol"
            };
        }

        private (bool PuedeGestionarUsuarios, bool PuedeGestionarProductos, bool PuedeAjustarInventario, 
                bool PuedeAnularVentas, bool PuedeVerReportes, bool PuedeVerCostos, 
                bool PuedeAplicarDescuentos, decimal? LimiteDescuento) ObtenerPermisosPorRol(TipoRol rol)
        {
            return rol switch
            {
                TipoRol.Administrador => (true, true, true, true, true, true, true, 100m),
                TipoRol.Supervisor => (false, false, false, true, true, true, true, 20m),
                TipoRol.Cajero => (false, false, false, false, false, false, true, 5m),
                _ => (false, false, false, false, false, false, false, null)
            };
        }
    }
}