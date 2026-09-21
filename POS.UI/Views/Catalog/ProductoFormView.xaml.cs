using System.Windows;
using POS.UI.ViewModels.Catalog;

namespace POS.UI.Views.Catalog
{
    public partial class ProductoFormView : Window
    {
        public ProductoFormView(ProductoFormViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}