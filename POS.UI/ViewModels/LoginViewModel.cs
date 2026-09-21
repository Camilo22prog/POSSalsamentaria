using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Application.DTOs.Security;
using POS.Application.Interfaces;
using POS.UI.Services;
using POS.UI.Views;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace POS.UI.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IAuthService _authService;

        [ObservableProperty]
        private string _nombreUsuario = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private string _mensajeError = string.Empty;

        [ObservableProperty]
        private bool _isLoading = false;

        public LoginViewModel(IAuthService authService)
        {
            _authService = authService;
        }

        [RelayCommand]
        private async Task LoginAsync()
        {
            MensajeError = string.Empty;

            if (string.IsNullOrWhiteSpace(NombreUsuario))
            {
                MensajeError = "Ingrese el nombre de usuario";
                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                MensajeError = "Ingrese la contraseña";
                return;
            }

            IsLoading = true;

            try
            {
                var request = new LoginRequestDto
                {
                    NombreUsuario = NombreUsuario.Trim(),
                    Password = Password
                };

                var result = await _authService.LoginAsync(request);

                if (result.Success && result.Usuario != null)
                {
                    SessionService.Instance.IniciarSesion(result.Usuario);

                    var mainWindow = App.Services.GetRequiredService<MainWindow>();
                    mainWindow.Show();

                    foreach (Window window in System.Windows.Application.Current.Windows)
                    {
                        if (window is LoginWindow)
                        {
                            window.Close();
                            break;
                        }
                    }
                }
                else
                {
                    MensajeError = result.Message ?? "Error al iniciar sesión";
                    Password = string.Empty;
                }
            }
            catch (Exception ex)
            {
                MensajeError = $"Error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }


        }

        [RelayCommand]
        private void Cancelar()
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}