using System.Windows.Controls;
using POS.UI.ViewModels.Dashboards;

namespace POS.UI.Views.Dashboards
{
    public partial class DashboardInventarioView : UserControl
    {
        private readonly DashboardInventarioViewModel _viewModel;

        public DashboardInventarioView(DashboardInventarioViewModel viewModel)
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
