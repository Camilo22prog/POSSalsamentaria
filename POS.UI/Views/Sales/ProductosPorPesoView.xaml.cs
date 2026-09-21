using POS.UI.ViewModels.Sales;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace POS.UI.Views.Sales
{
    public partial class ProductosPorPesoView : UserControl
    {
        // ── Acceso rápido al VM ───────────────────────────────────────────────
        private ProductosPorPesoViewModel VM => (ProductosPorPesoViewModel)DataContext;

        public ProductosPorPesoView(ProductosPorPesoViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        // ─────────────────────────────────────────────────────────────────────
        // LOADED: inicializar VM, suscribirse a cambios y tomar el foco
        // ─────────────────────────────────────────────────────────────────────
        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ProductosPorPesoViewModel vm)
            {
                // Inicializar (verifica estado de balanza, etc.)
                await vm.InicializarAsync();

                // Suscribirse a cambios de propiedades del VM
                vm.PropertyChanged += (s, args) =>
                {
                    // Actualizar resaltado naranja cuando el índice de categoría cambia
                    if (args.PropertyName == nameof(vm.IndiceCategoriaFocused))
                        ActualizarResaltadoCategoria(vm.IndiceCategoriaFocused);

                    // Cuando se cambia de pantalla, devolver el foco al UserControl
                    if (args.PropertyName == nameof(vm.MostrandoProductos))
                        Focus();
                };

                // Dibujar resaltado inicial en la primera categoría
                ActualizarResaltadoCategoria(vm.IndiceCategoriaFocused);
            }

            // Tomar el foco para recibir eventos de teclado
            Focus();
        }

        // ─────────────────────────────────────────────────────────────────────
        // KEYDOWN — único punto de entrada para todo el teclado
        //
        // Funciona porque en el XAML pusimos Focusable="False" en el ListBox,
        // el TextBox del peso y los botones, así el evento siempre sube hasta
        // el UserControl sin ser consumido por los hijos.
        // ─────────────────────────────────────────────────────────────────────
        private async void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            var vm = VM;

            // ── Pantalla de CATEGORÍAS ────────────────────────────────────────
            if (!vm.MostrandoProductos)
            {
                switch (e.Key)
                {
                    case Key.Up:
                        vm.NavCategoriaArriba();
                        e.Handled = true;
                        break;

                    case Key.Down:
                        vm.NavCategoriaAbajo();
                        e.Handled = true;
                        break;

                    case Key.Enter:
                        await vm.ConfirmarCategoriaFocusedAsync();
                        e.Handled = true;
                        break;

                    case Key.Escape:
                        vm.CerrarModulo();  // cierra el popup/ventana del módulo
                        e.Handled = true;
                        break;
                }
                return;
            }

            // ── Pantalla de PRODUCTOS ─────────────────────────────────────────
            switch (e.Key)
            {
                case Key.Up:
                    vm.NavProductoArriba();
                    HacerScrollAlSeleccionado();
                    e.Handled = true;
                    break;

                case Key.Down:
                    vm.NavProductoAbajo();
                    HacerScrollAlSeleccionado();
                    e.Handled = true;
                    break;

                case Key.P:
                    if (vm.LeerPesoBalanzaCommand.CanExecute(null))
                        await vm.LeerPesoBalanzaCommand.ExecuteAsync(null);
                    e.Handled = true;
                    break;

                case Key.Enter:
                    if (vm.AgregarAlCarritoCommand.CanExecute(null))
                        vm.AgregarAlCarritoCommand.Execute(null);
                    e.Handled = true;
                    break;

                case Key.Escape:
                    vm.VolverCommand.Execute(null);  // vuelve a pantalla de categorías
                    Focus();                          // devuelve el foco al UserControl
                    e.Handled = true;
                    break;
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // Scroll automático al producto seleccionado al navegar con teclado
        // ─────────────────────────────────────────────────────────────────────
        private void HacerScrollAlSeleccionado()
        {
            if (VM.ProductoSeleccionado == null) return;
            var item = ListaProductos.ItemContainerGenerator
                           .ContainerFromItem(VM.ProductoSeleccionado) as ListBoxItem;
            item?.BringIntoView();
        }

        // ─────────────────────────────────────────────────────────────────────
        // Resaltado visual (borde naranja) sobre la categoría activa por teclado
        // ─────────────────────────────────────────────────────────────────────
        private void ActualizarResaltadoCategoria(int indiceFocused)
        {
            for (int i = 0; i < ListaCategorias.Items.Count; i++)
            {
                var container = ListaCategorias.ItemContainerGenerator
                                    .ContainerFromIndex(i) as ContentPresenter;
                if (container == null) continue;

                container.ApplyTemplate();
                var border = VisualTreeHelper.GetChild(container, 0) as Border;
                if (border == null) continue;

                if (i == indiceFocused)
                {
                    border.BorderBrush     = new SolidColorBrush(Color.FromRgb(0xFF, 0x98, 0x00));
                    border.BorderThickness = new Thickness(3);
                    border.Effect = new DropShadowEffect
                    {
                        Color       = Colors.Black,
                        Opacity     = 0.30,
                        BlurRadius  = 14,
                        ShadowDepth = 3
                    };
                }
                else
                {
                    border.BorderBrush     = Brushes.Transparent;
                    border.BorderThickness = new Thickness(3);
                    border.Effect          = null;
                }
            }
        }
    }
}
