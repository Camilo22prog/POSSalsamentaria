using System.Windows.Controls;
using POS.UI.ViewModels.Catalog;

namespace POS.UI.Views.Catalog
{
    public partial class ProveedoresView : UserControl
    {
        public ProveedoresView(ProveedoresViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
