using System.Windows;
using POS.UI.ViewModels.Sales;

namespace POS.UI.Views.Sales
{
    public partial class BuscarClienteDialog : Window
    {
        public BuscarClienteDialog(BuscarClienteDialogViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            viewModel.SolicitarCierre += () =>
            {
                DialogResult = true;
                Close();
            };

            Loaded += (_, _) => TxtBusqueda.Focus();
        }
    }
}
