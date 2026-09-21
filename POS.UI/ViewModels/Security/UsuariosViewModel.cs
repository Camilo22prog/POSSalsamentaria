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
    public partial class UsuariosViewModel : ObservableObject
    {
        private readonly IUsuarioService _usuarioService;

        [ObservableProperty]
        private ObservableCollection<UsuarioDto> _usuarios = new();

        [ObservableProperty]
        private UsuarioDto? _usuarioSeleccionado;

        [ObservableProperty]
        private string _busqueda = string.Empty;

        [ObservableProperty]
        private bool _mostrarInactivos = false;

        public UsuariosViewModel(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
            _ = CargarUsuariosAsync();
        }

        [RelayCommand]
        private async Task CargarUsuariosAsync()
        {
            try
            {
                var usuarios = await _usuarioService.ObtenerTodosAsync();

                // Filtrar según búsqueda
                if (!string.IsNullOrWhiteSpace(Busqueda))
                {
                    usuarios = usuarios.Where(u =>
                        u.NombreUsuario.Contains(Busqueda, StringComparison.OrdinalIgnoreCase) ||
                        u.NombreCompleto.Contains(Busqueda, StringComparison.OrdinalIgnoreCase) ||
                        u.Email.Contains(Busqueda, StringComparison.OrdinalIgnoreCase)
                    );
                }

                // Filtrar inactivos
                if (!MostrarInactivos)
                {
                    usuarios = usuarios.Where(u => u.Estado == EstadoRegistro.Activo);
                }

                Usuarios = new ObservableCollection<UsuarioDto>(usuarios.OrderBy(u => u.NombreCompleto));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar usuarios: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void NuevoUsuario()
        {
            var window = new Views.Security.UsuarioFormView();
            if (window.ShowDialog() == true)
            {
                _ = CargarUsuariosAsync();
            }
        }

        [RelayCommand]
        private void EditarUsuario()
        {
            if (UsuarioSeleccionado == null)
            {
                MessageBox.Show("Seleccione un usuario", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var window = new Views.Security.UsuarioFormView(UsuarioSeleccionado.Id);
            if (window.ShowDialog() == true)
            {
                _ = CargarUsuariosAsync();
            }
        }

        [RelayCommand]
        private async Task ActivarDesactivarAsync()
        {
            if (UsuarioSeleccionado == null)
            {
                MessageBox.Show("Seleccione un usuario", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // No permitir desactivar al usuario actual
            if (UsuarioSeleccionado.Id == SessionService.Instance.UsuarioActual?.Id)
            {
                MessageBox.Show("No puede desactivar su propio usuario", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var accion = UsuarioSeleccionado.Estado == EstadoRegistro.Activo ? "desactivar" : "activar";
            var resultado = MessageBox.Show(
                $"¿Está seguro de {accion} al usuario '{UsuarioSeleccionado.NombreCompleto}'?",
                "Confirmación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resultado != MessageBoxResult.Yes)
                return;

            try
            {
                var usuarioActualId = SessionService.Instance.UsuarioActual?.Id ?? 0;

                if (UsuarioSeleccionado.Estado == EstadoRegistro.Activo)
                {
                    await _usuarioService.DesactivarAsync(UsuarioSeleccionado.Id, usuarioActualId);
                    MessageBox.Show("Usuario desactivado correctamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    await _usuarioService.ActivarAsync(UsuarioSeleccionado.Id, usuarioActualId);
                    MessageBox.Show("Usuario activado correctamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                await CargarUsuariosAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task BloquearDesbloquearAsync()
        {
            if (UsuarioSeleccionado == null)
            {
                MessageBox.Show("Seleccione un usuario", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // No permitir bloquear al usuario actual
            if (UsuarioSeleccionado.Id == SessionService.Instance.UsuarioActual?.Id)
            {
                MessageBox.Show("No puede bloquear su propio usuario", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var accion = UsuarioSeleccionado.Bloqueado ? "desbloquear" : "bloquear";
            var resultado = MessageBox.Show(
                $"¿Está seguro de {accion} al usuario '{UsuarioSeleccionado.NombreCompleto}'?",
                "Confirmación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resultado != MessageBoxResult.Yes)
                return;

            try
            {
                var usuarioActualId = SessionService.Instance.UsuarioActual?.Id ?? 0;

                if (UsuarioSeleccionado.Bloqueado)
                {
                    await _usuarioService.DesbloquearAsync(UsuarioSeleccionado.Id, usuarioActualId);
                    MessageBox.Show("Usuario desbloqueado correctamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    await _usuarioService.BloquearAsync(UsuarioSeleccionado.Id, usuarioActualId);
                    MessageBox.Show("Usuario bloqueado correctamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                await CargarUsuariosAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        partial void OnBusquedaChanged(string value)
        {
            _ = CargarUsuariosAsync();
        }

        partial void OnMostrarInactivosChanged(bool value)
        {
            _ = CargarUsuariosAsync();
        }
    }
}