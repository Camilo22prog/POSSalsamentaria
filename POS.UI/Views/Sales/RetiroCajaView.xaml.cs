using System.Windows;
using System.Windows.Controls;
using POS.UI.ViewModels.Sales;

namespace POS.UI.Views.Sales
{
    public partial class RetiroCajaView : Window
    {
        public RetiroCajaView(RetiroCajaViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void PbPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is RetiroCajaViewModel vm)
                vm.PasswordAutorizador = ((PasswordBox)sender).Password;
        }
    }
}
