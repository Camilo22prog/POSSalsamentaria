using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Application.Interfaces;
using POS.UI.Helpers;
using POS.UI.Services;
using System.Windows;

namespace POS.UI.ViewModels.Sales
{
    public partial class AbrirCajonManualViewModel : ObservableObject
    {
        private readonly IAuthService _authService;

        [ObservableProperty]
        private string _usuarioAutorizador = string.Empty;

        // Gestionada desde el code-behind del PasswordBox
        public string PasswordAutorizador { get; set; } = string.Empty;

        [ObservableProperty]
        private bool _isLoading;

        public AbrirCajonManualViewModel(IAuthService authService)
        {
            _authService = authService;
        }

        [RelayCommand]
        private async Task ValidarYAbrirAsync()
        {
            if (string.IsNullOrWhiteSpace(UsuarioAutorizador) || string.IsNullOrWhiteSpace(PasswordAutorizador))
            {
                MessageBox.Show("Ingrese el usuario y la contraseña del administrador o supervisor.",
                    "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsLoading = true;
            try
            {
                var (success, nombre, error) = await _authService.ValidarSupervisorAsync(UsuarioAutorizador, PasswordAutorizador);

                if (!success)
                {
                    MessageBox.Show(error ?? "Credenciales inválidas o sin permisos suficientes.",
                        "Acceso Denegado", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    new TicketPrinter().AbrirCajonSolo();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"No se pudo abrir el cajón: {ex.Message}",
                        "Error de Hardware", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Registro de auditoría del cajero que solicitó la apertura
                var usuarioId = SessionService.Instance.UsuarioActual?.Id ?? 0;
                await _authService.RegistrarAuditoriaAsync(
                    usuarioId,
                    "ABRIR_CAJON_MANUAL",
                    $"Autorizado por: {nombre ?? UsuarioAutorizador}");

                System.Windows.Application.Current.Windows
                    .OfType<Window>()
                    .FirstOrDefault(w => w.DataContext == this)
                    ?.Close();
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void Cancelar()
        {
            System.Windows.Application.Current.Windows
                .OfType<Window>()
                .FirstOrDefault(w => w.DataContext == this)
                ?.Close();
        }
    }
}
