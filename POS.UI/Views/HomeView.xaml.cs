using System.Windows.Controls;
using POS.UI.ViewModels;

namespace POS.UI.Views
{
    public partial class HomeView : UserControl
    {
        private readonly HomeViewModel _viewModel;

        public HomeView(HomeViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = viewModel;
        }

        private async void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            await _viewModel.InicializarAsync();
        }
    }
}
