using System.Windows.Controls;
using POS.UI.ViewModels.Inventory;

namespace POS.UI.Views.Inventory
{
    public partial class InventarioView : UserControl
    {
        public InventarioView(InventarioViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            
            Loaded += async (s, e) => await viewModel.CargarAsync();
        }
    }
}