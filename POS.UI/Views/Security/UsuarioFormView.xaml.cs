using Microsoft.Extensions.DependencyInjection;
using POS.UI.ViewModels.Security;
using System.Windows;
using System.Windows.Controls;

namespace POS.UI.Views.Security
{
    public partial class UsuarioFormView : Window
    {
        private readonly UsuarioFormViewModel _viewModel;

        public UsuarioFormView(int? usuarioId = null)
        {
            InitializeComponent();

            var usuarioService = App.Services.GetRequiredService<Application.Interfaces.IUsuarioService>();
            _viewModel = new UsuarioFormViewModel(usuarioService, usuarioId);
            DataContext = _viewModel;
        }

        private void TxtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                _viewModel.Password = passwordBox.Password;
            }
        }

        private void TxtConfirmarPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                _viewModel.ConfirmarPassword = passwordBox.Password;
            }
        }
    }
}