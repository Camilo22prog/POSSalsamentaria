using System.Windows.Controls;
using POS.UI.ViewModels.Customers;

namespace POS.UI.Views.Customers
{
    public partial class ClientesView : UserControl
    {
        public ClientesView(ClientesViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}