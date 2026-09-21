using POS.Application.DTOs.Security;
using POS.Domain.Enums;

namespace POS.UI.Services
{
    public class SessionService
    {
        private static SessionService? _instance;
        public static SessionService Instance => _instance ??= new SessionService();

        public UsuarioDto? UsuarioActual { get; private set; }
        public bool EstaLogueado => UsuarioActual != null;

        private SessionService() { }

        public void IniciarSesion(UsuarioDto usuario)
        {
            UsuarioActual = usuario;
        }

        public void CerrarSesion()
        {
            UsuarioActual = null;
        }

        // ✅ Métodos de permisos usando UsuarioDto
        public bool EsAdministrador()
        {
            return UsuarioActual?.Rol == TipoRol.Administrador;
        }

        public bool EsSupervisor()
        {
            return UsuarioActual?.Rol == TipoRol.Supervisor;
        }

        public bool EsCajero()
        {
            return UsuarioActual?.Rol == TipoRol.Cajero;
        }

        public bool PuedeAnularVentas()
        {
            return UsuarioActual?.PuedeAnularVentas ?? false;
        }

        public bool PuedeVerReportes()
        {
            return UsuarioActual?.PuedeVerReportes ?? false;
        }

        public bool PuedeGestionarInventario()
        {
            return UsuarioActual?.PuedeAjustarInventario ?? false;
        }

        public bool PuedeGestionarUsuarios()
        {
            return UsuarioActual?.PuedeGestionarUsuarios ?? false;
        }

        public bool PuedeCerrarCaja()
        {
            return UsuarioActual?.Rol == TipoRol.Administrador || 
                   UsuarioActual?.Rol == TipoRol.Supervisor;
        }

        public bool PuedeGestionarProductos()
        {
            return UsuarioActual?.PuedeGestionarProductos ?? false;
        }

        public string ObtenerNombreRol()
        {
            return UsuarioActual?.RolNombre ?? "Sin rol";
        }
    }
}