using System.Windows.Controls;
using POS.UI.ViewModels.Sales;

namespace POS.UI.Views.Sales
{
    public partial class HistorialVentasView : UserControl
    {
        public HistorialVentasView(HistorialVentasViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}