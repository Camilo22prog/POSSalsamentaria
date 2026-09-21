using System.Windows.Controls;
using POS.UI.ViewModels.Expenses;

namespace POS.UI.Views.Expenses
{
    public partial class GastosView : UserControl
    {
        public GastosView(GastosViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
