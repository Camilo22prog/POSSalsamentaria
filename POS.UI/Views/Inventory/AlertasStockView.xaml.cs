using POS.UI.ViewModels.Inventory;
using System.Windows;

namespace POS.UI.Views.Inventory
{
    public partial class AlertasStockView : Window
    {
        public AlertasStockView(AlertasStockViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e) => Close();
    }
}
