using System.Windows.Controls;
using POS.UI.ViewModels.Reports;

namespace POS.UI.Views.Reports
{
    public partial class ReporteArqueosView : UserControl
    {
        public ReporteArqueosView(ReporteArqueosViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}