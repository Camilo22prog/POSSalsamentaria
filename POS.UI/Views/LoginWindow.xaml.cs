using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using POS.UI.ViewModels;

namespace POS.UI.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow(LoginViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel vm && sender is PasswordBox pb)
            {
                vm.Password = pb.Password;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && DataContext is LoginViewModel vm && !vm.IsLoading)
            {
                vm.LoginCommand.Execute(null);
                e.Handled = true;
            }
        }
    }
}