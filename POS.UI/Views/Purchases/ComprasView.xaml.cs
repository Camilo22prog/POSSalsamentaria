using POS.UI.ViewModels.Purchases;
using System.Windows.Controls;

namespace POS.UI.Views.Purchases
{
    public partial class ComprasView : UserControl
    {
        public ComprasView(ComprasViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
