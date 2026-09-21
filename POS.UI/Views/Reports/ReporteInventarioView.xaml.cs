using System.Windows.Controls;
using POS.UI.ViewModels.Reports;

namespace POS.UI.Views.Reports
{
    public partial class ReporteInventarioView : UserControl
    {
        public ReporteInventarioView(ReporteInventarioViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}