using System.Windows.Controls;
using POS.UI.ViewModels.Reports;

namespace POS.UI.Views.Reports
{
    public partial class EstadoResultadosView : UserControl
    {
        public EstadoResultadosView(EstadoResultadosViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}