using System.Windows.Controls;
using POS.UI.ViewModels.Reports;

namespace POS.UI.Views.Reports
{
    public partial class AnalisisABCView : UserControl
    {
        public AnalisisABCView(AnalisisABCViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}