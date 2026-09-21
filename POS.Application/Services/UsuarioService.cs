using POS.Application.DTOs.Security;
using POS.Application.Interfaces;
using POS.Domain.Entities.Security;
using POS.Domain.Enums;
using POS.Domain.Interfaces.Repositories;
using System.Security.Cryptography;
using System.Text;

namespace POS.Application.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UsuarioService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<UsuarioDto>> ObtenerTodosAsync()
        {
            var usuarios = await _unitOfWork.Usuarios.GetAllAsync();
            return usuarios.Select(MapToDto);
        }

        public async Task<IEnumerable<UsuarioDto>> ObtenerActivosAsync()
        {
            var usuarios = await _unitOfWork.Usuarios.GetAllAsync();
            return usuarios
                .Where(u => u.Estado == EstadoRegistro.Activo && !u.Bloqueado)
                .Select(MapToDto);
        }

        public async Task<UsuarioDto?> ObtenerPorIdAsync(int id)
        {
            var usuario = await _unitOfWork.Usuarios.GetByIdAsync(id);
            return usuario != null ? MapToDto(usuario) : null;
        }

        public async Task<UsuarioDto> CrearAsync(CrearUsuarioDto dto, int creadoPorId)
        {
            // Validaciones
            if (await ExisteNombreUsuarioAsync(dto.NombreUsuario))
            {
                throw new Exception($"El nombre de usuario '{dto.NombreUsuario}' ya existe");
            }

            if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 4)
            {
                throw new Exception("La contraseña debe tener al menos 4 caracteres");
            }

            if (string.IsNullOrWhiteSpace(dto.Email) || !dto.Email.Contains("@"))
            {
                throw new Exception("Email inválido");
            }

            var usuario = new Usuario
            {
                NombreUsuario = dto.NombreUsuario.Trim().ToLower(),
                NombreCompleto = dto.NombreCompleto.Trim(),
                Email = dto.Email.Trim().ToLower(),
                Telefono = dto.Telefono?.Trim(),
                PasswordHash = HashPassword(dto.Password),
                Rol = dto.Rol,
                Estado = EstadoRegistro.Activo,
                Bloqueado = false,
                IntentosFailidos = 0,
                FechaCreacion = DateTime.Now,
                CreadoPorId = creadoPorId
            };

            await _unitOfWork.Usuarios.AddAsync(usuario);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(usuario);
        }

        public async Task<UsuarioDto> ActualizarAsync(int id, CrearUsuarioDto dto, int modificadoPorId)
        {
            var usuario = await _unitOfWork.Usuarios.GetByIdAsync(id);
            if (usuario == null)
            {
                throw new Exception("Usuario no encontrado");
            }

            // Validar username único (excepto el mismo usuario)
            if (await ExisteNombreUsuarioAsync(dto.NombreUsuario, id))
            {
                throw new Exception($"El nombre de usuario '{dto.NombreUsuario}' ya existe");
            }

            if (string.IsNullOrWhiteSpace(dto.Email) || !dto.Email.Contains("@"))
            {
                throw new Exception("Email inválido");
            }

            usuario.NombreUsuario = dto.NombreUsuario.Trim().ToLower();
            usuario.NombreCompleto = dto.NombreCompleto.Trim();
            usuario.Email = dto.Email.Trim().ToLower();
            usuario.Telefono = dto.Telefono?.Trim();
            usuario.Rol = dto.Rol;
            usuario.FechaModificacion = DateTime.Now;
            usuario.ModificadoPorId = modificadoPorId;

            // Solo actualizar password si se proporciona uno nuevo
            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                if (dto.Password.Length < 4)
                {
                    throw new Exception("La contraseña debe tener al menos 4 caracteres");
                }
                usuario.PasswordHash = HashPassword(dto.Password);
            }

            _unitOfWork.Usuarios.Update(usuario);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(usuario);
        }

        public async Task DesactivarAsync(int id, int modificadoPorId)
        {
            var usuario = await _unitOfWork.Usuarios.GetByIdAsync(id);
            if (usuario == null)
            {
                throw new Exception("Usuario no encontrado");
            }

            usuario.Estado = EstadoRegistro.Inactivo;
            usuario.FechaModificacion = DateTime.Now;
            usuario.ModificadoPorId = modificadoPorId;

            _unitOfWork.Usuarios.Update(usuario);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task ActivarAsync(int id, int modificadoPorId)
        {
            var usuario = await _unitOfWork.Usuarios.GetByIdAsync(id);
            if (usuario == null)
            {
                throw new Exception("Usuario no encontrado");
            }

            usuario.Estado = EstadoRegistro.Activo;
            usuario.FechaModificacion = DateTime.Now;
            usuario.ModificadoPorId = modificadoPorId;

            _unitOfWork.Usuarios.Update(usuario);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task BloquearAsync(int id, int modificadoPorId)
        {
            var usuario = await _unitOfWork.Usuarios.GetByIdAsync(id);
            if (usuario == null)
            {
                throw new Exception("Usuario no encontrado");
            }

            usuario.Bloqueado = true;
            usuario.FechaModificacion = DateTime.Now;
            usuario.ModificadoPorId = modificadoPorId;

            _unitOfWork.Usuarios.Update(usuario);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DesbloquearAsync(int id, int modificadoPorId)
        {
            var usuario = await _unitOfWork.Usuarios.GetByIdAsync(id);
            if (usuario == null)
            {
                throw new Exception("Usuario no encontrado");
            }

            usuario.Bloqueado = false;
            usuario.IntentosFailidos = 0;
            usuario.FechaModificacion = DateTime.Now;
            usuario.ModificadoPorId = modificadoPorId;

            _unitOfWork.Usuarios.Update(usuario);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario, int? excluirId = null)
        {
            var usuarios = await _unitOfWork.Usuarios.GetAllAsync();
            return usuarios.Any(u => 
                u.NombreUsuario.ToLower() == nombreUsuario.ToLower().Trim() && 
                (!excluirId.HasValue || u.Id != excluirId.Value));
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

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}