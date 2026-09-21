using POS.UI.ViewModels.Purchases;
using System.Windows;

namespace POS.UI.Views.Purchases
{
    public partial class NuevaCompraView : Window
    {
        public NuevaCompraView(NuevaCompraViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

        }

        private void TextBox_GotKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            if (sender is System.Windows.Controls.TextBox textBox)
            {
                textBox.SelectAll();
            }
        }

        private void TextBox_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is System.Windows.Controls.TextBox textBox)
            {
                if (!textBox.IsKeyboardFocusWithin)
                {
                    e.Handled = true;
                    textBox.Focus();
                }
            }
        }
    }
}
