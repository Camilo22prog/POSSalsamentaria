using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using POS.UI.ViewModels.Sales;

namespace POS.UI.Views.Sales
{
    public partial class ProcesarPagoView : Window
    {
        private readonly ProcesarPagoViewModel _viewModel;

        public ProcesarPagoView(ProcesarPagoViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            _viewModel = viewModel;

            // Auto-focus en efectivo
            Loaded += (s, e) => TxtEfectivo.Focus();

            // Capturar teclas globales en la ventana
            PreviewKeyDown += ProcesarPagoView_PreviewKeyDown;
        }

        private void ProcesarPagoView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                // Enter → Completar venta (desde cualquier campo)
                case Key.Enter:
                    if (_viewModel.CompletarVentaCommand.CanExecute(null))
                    {
                        _viewModel.CompletarVentaCommand.Execute(null);
                        e.Handled = true;
                    }
                    break;

                // Escape → Cerrar ventana y volver a agregar productos
                case Key.Escape:
                    Close();
                    e.Handled = true;
                    break;

                // Tab → Moverse entre campos (comportamiento natural, no interferir)
                case Key.Tab:
                    break;
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                // Seleccionar todo al hacer focus para facilitar reemplazo
                textBox.SelectAll();
            }
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}