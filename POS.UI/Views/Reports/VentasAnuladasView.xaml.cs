using System.Windows.Controls;
using POS.UI.ViewModels.Reports;

namespace POS.UI.Views.Reports
{
    public partial class VentasAnuladasView : UserControl
    {
        public VentasAnuladasView(VentasAnuladasViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}