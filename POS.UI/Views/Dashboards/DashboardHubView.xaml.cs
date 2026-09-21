using System.Windows.Controls;
using POS.UI.ViewModels.Dashboards;

namespace POS.UI.Views.Dashboards
{
    public partial class DashboardHubView : UserControl
    {
        private readonly DashboardHubViewModel _viewModel;

        public DashboardHubView(DashboardHubViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = viewModel;
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            _viewModel.Initialize();
        }
    }
}
