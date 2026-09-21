using System.Windows;
using POS.UI.ViewModels.Customers;

namespace POS.UI.Views.Customers
{
    public partial class ClienteFormView : Window
    {
        public ClienteFormView(ClienteFormViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}