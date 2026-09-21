using System.Windows.Controls;
using POS.UI.ViewModels.Reports;

namespace POS.UI.Views.Reports
{
    public partial class VentasPorMetodoPagoView : UserControl
    {
        public VentasPorMetodoPagoView(VentasPorMetodoPagoViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}