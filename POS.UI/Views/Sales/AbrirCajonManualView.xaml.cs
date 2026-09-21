using System.Windows;
using System.Windows.Controls;
using POS.UI.ViewModels.Sales;

namespace POS.UI.Views.Sales
{
    public partial class AbrirCajonManualView : Window
    {
        public AbrirCajonManualView(AbrirCajonManualViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            Loaded += (_, _) => TxtUsuario.Focus();
        }

        private void PbPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is AbrirCajonManualViewModel vm)
                vm.PasswordAutorizador = ((PasswordBox)sender).Password;
        }
    }
}
