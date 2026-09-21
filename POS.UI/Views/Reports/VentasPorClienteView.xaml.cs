using System.Windows.Controls;
using POS.UI.ViewModels.Reports;

namespace POS.UI.Views.Reports
{
    public partial class VentasPorClienteView : UserControl
    {
        public VentasPorClienteView(VentasPorClienteViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}