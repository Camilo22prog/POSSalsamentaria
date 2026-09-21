using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Application.DTOs.Security;
using POS.Application.Interfaces;
using POS.Domain.Enums;
using POS.UI.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;


namespace POS.UI.ViewModels.Security
{
    public partial class UsuarioFormViewModel : ObservableObject
    {
        private readonly IUsuarioService _usuarioService;
        private readonly int? _usuarioId;

        [ObservableProperty]
        private string _titulo = "Nuevo Usuario";

        [ObservableProperty]
        private string _nombreUsuario = string.Empty;

        [ObservableProperty]
        private string _nombreCompleto = string.Empty;

        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string _telefono = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private string _confirmarPassword = string.Empty;

        [ObservableProperty]
        private TipoRol _rolSeleccionado = TipoRol.Cajero;

        [ObservableProperty]
        private bool _esEdicion = false;

        public ObservableCollection<RolItem> Roles { get; set; }

        public UsuarioFormViewModel(IUsuarioService usuarioService, int? usuarioId = null)
        {
            _usuarioService = usuarioService;
            _usuarioId = usuarioId;

            // Configurar roles disponibles
            Roles = new ObservableCollection<RolItem>
            {
                new RolItem { Rol = TipoRol.Administrador, Nombre = "Administrador", Descripcion = "Acceso total al sistema" },
                new RolItem { Rol = TipoRol.Supervisor, Nombre = "Supervisor", Descripcion = "Puede anular ventas y ver reportes" },
                new RolItem { Rol = TipoRol.Cajero, Nombre = "Cajero", Descripcion = "Solo puede realizar ventas" }
            };

            if (_usuarioId.HasValue)
            {
                EsEdicion = true;
                Titulo = "Editar Usuario";
                _ = CargarUsuarioAsync();
            }
        }

        private async Task CargarUsuarioAsync()
        {
            try
            {
                var usuario = await _usuarioService.ObtenerPorIdAsync(_usuarioId!.Value);
                if (usuario != null)
                {
                    NombreUsuario = usuario.NombreUsuario;
                    NombreCompleto = usuario.NombreCompleto;
                    Email = usuario.Email;
                    Telefono = usuario.Telefono ?? string.Empty;
                    RolSeleccionado = usuario.Rol;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar usuario: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task GuardarAsync()
        {
            // Validaciones
            if (string.IsNullOrWhiteSpace(NombreUsuario))
            {
                MessageBox.Show("El nombre de usuario es requerido", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(NombreCompleto))
            {
                MessageBox.Show("El nombre completo es requerido", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(Email) || !Email.Contains("@"))
            {
                MessageBox.Show("Email inválido", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!EsEdicion)
            {
                if (string.IsNullOrWhiteSpace(Password))
                {
                    MessageBox.Show("La contraseña es requerida", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (Password != ConfirmarPassword)
                {
                    MessageBox.Show("Las contraseñas no coinciden", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (Password.Length < 4)
                {
                    MessageBox.Show("La contraseña debe tener al menos 4 caracteres", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            else
            {
                // En edición, solo validar password si se proporcionó uno nuevo
                if (!string.IsNullOrWhiteSpace(Password))
                {
                    if (Password != ConfirmarPassword)
                    {
                        MessageBox.Show("Las contraseñas no coinciden", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    if (Password.Length < 4)
                    {
                        MessageBox.Show("La contraseña debe tener al menos 4 caracteres", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
            }

            try
            {
                var dto = new CrearUsuarioDto
                {
                    NombreUsuario = NombreUsuario.Trim(),
                    NombreCompleto = NombreCompleto.Trim(),
                    Email = Email.Trim(),
                    Telefono = string.IsNullOrWhiteSpace(Telefono) ? null : Telefono.Trim(),
                    Password = Password,
                    Rol = RolSeleccionado
                };

                var usuarioActualId = SessionService.Instance.UsuarioActual?.Id ?? 0;

                if (EsEdicion)
                {
                    await _usuarioService.ActualizarAsync(_usuarioId!.Value, dto, usuarioActualId);
                    MessageBox.Show("Usuario actualizado correctamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    await _usuarioService.CrearAsync(dto, usuarioActualId);
                    MessageBox.Show("Usuario creado correctamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                // Cerrar ventana
                System.Windows.Application.Current.Windows.OfType<Window>()
                    .FirstOrDefault(w => w.DataContext == this)?.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void Cancelar()
        {
            System.Windows.Application.Current.Windows.OfType<Window>()
                .FirstOrDefault(w => w.DataContext == this)?.Close();
        }
    }

    public class RolItem
    {
        public TipoRol Rol { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }
}