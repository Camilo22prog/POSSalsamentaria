
using System.Windows;
using POS.UI.ViewModels.Sales;

namespace POS.UI.Views.Sales
{
    public partial class AbrirCajaView : Window
    {
        public AbrirCajaView(AbrirCajaViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}