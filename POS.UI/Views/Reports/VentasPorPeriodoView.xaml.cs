using System.Windows.Controls;
using POS.UI.ViewModels.Reports;

namespace POS.UI.Views.Reports
{
    public partial class VentasPorPeriodoView : UserControl
    {
        public VentasPorPeriodoView(VentasPorPeriodoViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            
            // ✅ AGREGAR DEBUG
            System.Diagnostics.Debug.WriteLine($"✅ VentasPorPeriodoView inicializado con ViewModel: {viewModel != null}");
        }
    }
}