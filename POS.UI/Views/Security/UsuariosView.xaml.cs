using POS.UI.ViewModels.Security;
using System.Windows.Controls;

namespace POS.UI.Views.Security
{
    public partial class UsuariosView : UserControl
    {
        public UsuariosView(UsuariosViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}