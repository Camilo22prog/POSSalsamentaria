using System.Windows.Controls;
using POS.UI.ViewModels.Catalog;

namespace POS.UI.Views.Catalog
{
    public partial class CategoriasView : UserControl
    {
        public CategoriasView(CategoriasViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
