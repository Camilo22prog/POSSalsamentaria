using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Microsoft.Extensions.DependencyInjection;
using POS.UI.Services;
using POS.UI.ViewModels;
using POS.UI.ViewModels.Sales;
using POS.UI.Views.Sales;

namespace POS.UI.Views
{
    public partial class MainWindow : Window
    {
        private const double SidebarCollapsed = 64;
        private const double SidebarExpanded  = 230;

        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            PreviewKeyDown += MainWindow_PreviewKeyDown;
        }

        // ── Sidebar hover ────────────────────────────────────────────────────

        private void Sidebar_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
            => AnimateSidebar(SidebarExpanded);

        private void Sidebar_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
            => AnimateSidebar(SidebarCollapsed);

        private void AnimateSidebar(double toWidth)
        {
            var anim = new DoubleAnimation(toWidth, TimeSpan.FromMilliseconds(180))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            SidebarBorder.BeginAnimation(WidthProperty, anim);
        }

        // ── Ctrl+P: abrir cajón manual ───────────────────────────────────────

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.P && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (!SessionService.Instance.EstaLogueado) return;

                e.Handled = true;
                var vm = App.Services.GetRequiredService<AbrirCajonManualViewModel>();
                var view = new AbrirCajonManualView(vm) { Owner = this };
                view.ShowDialog();
            }
        }
    }
}