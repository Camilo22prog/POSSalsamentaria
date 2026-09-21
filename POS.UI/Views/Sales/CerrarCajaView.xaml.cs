using System.Windows;
using POS.UI.ViewModels.Sales;

namespace POS.UI.Views.Sales
{
    public partial class CerrarCajaView : Window
    {
        public CerrarCajaView(CerrarCajaViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}