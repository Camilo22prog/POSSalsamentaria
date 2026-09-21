using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using POS.Application.Interfaces;
using POS.UI.ViewModels.Sales;

namespace POS.UI.Views.Sales
{
    public partial class POSView : UserControl
    {
        public POSView(POSViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            Loaded += async (s, e) =>
            {
                await viewModel.InicializarAsync();
            };

            // Capturar teclas a nivel de UserControl
            PreviewKeyDown += POSView_PreviewKeyDown;
        }

        private void TxtBusqueda_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                textBox.Focus();
                textBox.PreviewKeyDown += TxtBusqueda_PreviewKeyDown;
            }
        }

        private void TxtBusqueda_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var vm = DataContext as POSViewModel;
            var pestana = vm?.PestanaActiva;
            if (pestana == null) return;

            switch (e.Key)
            {
                case Key.F12:
                    pestana.ProcesarPagoCommand.Execute(null);
                    e.Handled = true;
                    break;

                case Key.F5:
                    pestana.CancelarVentaCommand.Execute(null);
                    e.Handled = true;
                    break;

                case Key.Escape:
                    pestana.CodigoBusqueda = string.Empty;
                    e.Handled = true;
                    break;

                // + desde búsqueda → Aumentar cantidad último producto
                case Key.Add:
                case Key.OemPlus:
                    if (string.IsNullOrWhiteSpace(pestana.CodigoBusqueda))
                    {
                        pestana.AumentarCantidadCommand.Execute(null);
                        e.Handled = true;
                    }
                    break;

                // - desde búsqueda → Disminuir cantidad último producto
                case Key.Subtract:
                case Key.OemMinus:
                    if (string.IsNullOrWhiteSpace(pestana.CodigoBusqueda))
                    {
                        pestana.DisminuirCantidadCommand.Execute(null);
                        e.Handled = true;
                    }
                    break;
            }
        }

        private void POSView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var vm = DataContext as POSViewModel;
            var pestana = vm?.PestanaActiva;
            if (pestana == null) return;

            bool enTextBox = Keyboard.FocusedElement is TextBox;

            switch (e.Key)
            {
                // F1 - Enfocar búsqueda
                case Key.F1:
                    EnfocarBusqueda();
                    e.Handled = true;
                    break;

                // F12 - Procesar pago
                case Key.F12:
                    pestana.ProcesarPagoCommand.Execute(null);
                    e.Handled = true;
                    break;

                // F5 - Cancelar venta
                case Key.F5:
                    pestana.CancelarVentaCommand.Execute(null);
                    e.Handled = true;
                    break;

                // F7 - Reimprimir última venta
                case Key.F7:
                    vm!.ImprimirUltimaVentaCommand.Execute(null);
                    e.Handled = true;
                    break;

                // F4 - Quitar item seleccionado
                case Key.F4:
                    pestana.QuitarItemCommand.Execute(null);
                    e.Handled = true;
                    break;

                // Ctrl+N - Nueva pestaña
                case Key.N when Keyboard.Modifiers == ModifierKeys.Control:
                    vm!.NuevaPestanaCommand.Execute(null);
                    e.Handled = true;
                    break;

                // Ctrl+W - Cerrar pestaña
                case Key.W when Keyboard.Modifiers == ModifierKeys.Control:
                    pestana.CancelarVentaCommand.Execute(null);
                    e.Handled = true;
                    break;

                // Ctrl+C - Buscar cliente (solo fuera de textbox para no romper copiar)
                case Key.C when Keyboard.Modifiers == ModifierKeys.Control:
                    if (!enTextBox)
                    {
                        AbrirDialogoCliente(pestana);
                        e.Handled = true;
                    }
                    break;

                // + Aumentar cantidad
                case Key.Add:
                case Key.OemPlus:
                    if (!enTextBox)
                    {
                        pestana.AumentarCantidadCommand.Execute(null);
                        e.Handled = true;
                    }
                    break;

                // - Disminuir cantidad
                case Key.Subtract:
                case Key.OemMinus:
                    if (!enTextBox)
                    {
                        pestana.DisminuirCantidadCommand.Execute(null);
                        e.Handled = true;
                    }
                    break;

                // Delete - Eliminar item
                case Key.Delete:
                    if (!enTextBox)
                    {
                        pestana.QuitarItemCommand.Execute(null);
                        e.Handled = true;
                    }
                    break;

                // Flechas ↑↓ - Navegar carrito
                case Key.Up:
                    if (!enTextBox)
                    {
                        pestana.SeleccionarItemAnteriorCommand.Execute(null);
                        e.Handled = true;
                    }
                    break;

                case Key.Down:
                    if (!enTextBox)
                    {
                        pestana.SeleccionarItemSiguienteCommand.Execute(null);
                        e.Handled = true;
                    }
                    break;
            }
        }

        private void AbrirDialogoCliente(VentaPestanaViewModel pestana)
        {
            var clienteService = App.Services.GetRequiredService<IClienteService>();
            var dialogVm = new BuscarClienteDialogViewModel(clienteService, pestana.ClienteSeleccionado);
            var dialog = new BuscarClienteDialog(dialogVm)
            {
                Owner = Window.GetWindow(this)
            };

            if (dialog.ShowDialog() == true)
            {
                if (dialogVm.Limpiado)
                    pestana.LimpiarClienteCommand.Execute(null);
                else if (dialogVm.ClienteElegido != null)
                    pestana.SeleccionarClienteCommand.Execute(dialogVm.ClienteElegido);
            }
        }

        private void EnfocarBusqueda()
        {
            var txtBusqueda = FindVisualChild<TextBox>(this);
            txtBusqueda?.Focus();
        }

        private static T? FindVisualChild<T>(System.Windows.DependencyObject parent) where T : System.Windows.DependencyObject
        {
            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                if (child is T result)
                    return result;

                var descendant = FindVisualChild<T>(child);
                if (descendant != null)
                    return descendant;
            }
            return null;
        }
    }
}