using System.Windows.Controls;
using POS.UI.ViewModels.Settings;

namespace POS.UI.Views.Settings
{
    public partial class ConfiguracionView : UserControl
    {
        public ConfiguracionView(ConfiguracionViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
