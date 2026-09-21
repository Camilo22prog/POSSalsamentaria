using System.Windows.Controls;
using POS.UI.ViewModels.Reports;

namespace POS.UI.Views.Reports
{
    public partial class VentasPorProductoView : UserControl
    {
        public VentasPorProductoView(VentasPorProductoViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}