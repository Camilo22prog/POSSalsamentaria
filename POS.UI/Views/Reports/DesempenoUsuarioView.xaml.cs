using System.Windows.Controls;
using POS.UI.ViewModels.Reports;

namespace POS.UI.Views.Reports
{
    public partial class DesempenoUsuarioView : UserControl
    {
        public DesempenoUsuarioView(DesempenoUsuarioViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}