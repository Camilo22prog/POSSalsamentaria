using POS.UI.ViewModels;
using System.Windows;

namespace POS.UI.Views
{
    public partial class UpdateView : Window
    {
        public UpdateView(UpdateViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}