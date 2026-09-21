using System.Windows;
using POS.UI.ViewModels.Inventory;

namespace POS.UI.Views.Inventory
{
    public partial class KardexView : Window
    {
        public KardexView(KardexViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}