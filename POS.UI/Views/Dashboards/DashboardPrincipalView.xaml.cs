using System.Windows.Controls;
using POS.UI.ViewModels.Dashboards;

namespace POS.UI.Views.Dashboards
{
    public partial class DashboardPrincipalView : UserControl
    {
        public DashboardPrincipalView(DashboardPrincipalViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private async void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is DashboardPrincipalViewModel viewModel)
            {
                await viewModel.InicializarAsync();
            }
        }
    }
}