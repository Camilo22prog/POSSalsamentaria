using System.Windows.Controls;
using POS.UI.ViewModels.Catalog;

namespace POS.UI.Views.Catalog
{
    public partial class ProductosView : UserControl
    {
        public ProductosView(ProductosViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            
            Loaded += async (s, e) => await viewModel.CargarAsync();
        }
    }
}