using System.Windows;
using POS.UI.ViewModels.Sales;

namespace POS.UI.Views.Sales
{
    public partial class AnularVentaView : Window
    {
        public AnularVentaView(AnularVentaViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}