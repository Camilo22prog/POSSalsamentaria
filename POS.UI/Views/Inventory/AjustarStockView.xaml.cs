using System.Windows;
using POS.UI.ViewModels.Inventory;

namespace POS.UI.Views.Inventory
{
    public partial class AjustarStockView : Window
    {
        public AjustarStockView(AjustarStockViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            
            // Auto-focus al campo de Stock Nuevo al abrir
            Loaded += (s, e) => TxtStockNuevo.Focus();
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}