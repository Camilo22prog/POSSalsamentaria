using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Application.DTOs.Catalog;
using POS.Application.Interfaces;
using POS.UI.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace POS.UI.ViewModels.Catalog
{
    public partial class CategoriasViewModel : ObservableObject
    {
        private readonly ICategoriaService _categoriaService;

        [ObservableProperty]
        private ObservableCollection<CategoriaDto> _categorias = new();

        [ObservableProperty]
        private CategoriaDto? _categoriaSeleccionada;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _mostrandoFormulario;

        [ObservableProperty]
        private string _nombreNueva = string.Empty;

        [ObservableProperty]
        private string _descripcionNueva = string.Empty;

        [ObservableProperty]
        private string _colorNueva = "#673AB7";

        [ObservableProperty]
        private string _mensajeError = string.Empty;

        [ObservableProperty]
        private bool _editando;

        private int? _editandoId;

        public CategoriasViewModel(ICategoriaService categoriaService)
        {
            _categoriaService = categoriaService;
            _ = CargarAsync();
        }

        [RelayCommand]
        private async Task CargarAsync()
        {
            IsLoading = true;
            try
            {
                var lista = await _categoriaService.ObtenerTodasAsync();
                Categorias = new ObservableCollection<CategoriaDto>(lista);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar categorías: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally { IsLoading = false; }
        }

        [RelayCommand]
        private void NuevaCategoria()
        {
            _editandoId = null;
            Editando = false;
            NombreNueva = string.Empty;
            DescripcionNueva = string.Empty;
            ColorNueva = "#673AB7";
            MensajeError = string.Empty;
            MostrandoFormulario = true;
        }

        [RelayCommand]
        private void EditarCategoria()
        {
            if (CategoriaSeleccionada == null)
            {
                MessageBox.Show("Seleccione una categoría para editar.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            _editandoId = CategoriaSeleccionada.Id;
            Editando = true;
            NombreNueva = CategoriaSeleccionada.Nombre;
            DescripcionNueva = CategoriaSeleccionada.Descripcion ?? string.Empty;
            ColorNueva = CategoriaSeleccionada.Color ?? "#673AB7";
            MensajeError = string.Empty;
            MostrandoFormulario = true;
        }

        [RelayCommand]
        private async Task GuardarAsync()
        {
            if (string.IsNullOrWhiteSpace(NombreNueva))
            {
                MensajeError = "El nombre es requerido.";
                return;
            }

            IsLoading = true;
            MensajeError = string.Empty;
            try
            {
                var dto = new CrearCategoriaDto
                {
                    Nombre = NombreNueva.Trim(),
                    Descripcion = string.IsNullOrWhiteSpace(DescripcionNueva) ? null : DescripcionNueva.Trim(),
                    Color = ColorNueva
                };
                var usuarioId = SessionService.Instance.UsuarioActual?.Id ?? 0;

                if (_editandoId.HasValue)
                    await _categoriaService.ActualizarAsync(_editandoId.Value, dto, usuarioId);
                else
                    await _categoriaService.CrearAsync(dto, usuarioId);

                MostrandoFormulario = false;
                await CargarAsync();
            }
            catch (Exception ex)
            {
                MensajeError = ex.Message;
            }
            finally { IsLoading = false; }
        }

        [RelayCommand]
        private void CancelarFormulario()
        {
            MostrandoFormulario = false;
            MensajeError = string.Empty;
        }

        [RelayCommand]
        private async Task DesactivarAsync()
        {
            if (CategoriaSeleccionada == null)
            {
                MessageBox.Show("Seleccione una categoría.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"¿Desactivar la categoría '{CategoriaSeleccionada.Nombre}'?",
                "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                var usuarioId = SessionService.Instance.UsuarioActual?.Id ?? 0;
                await _categoriaService.DesactivarAsync(CategoriaSeleccionada.Id, usuarioId);
                await CargarAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task ActivarAsync()
        {
            if (CategoriaSeleccionada == null)
            {
                MessageBox.Show("Seleccione una categoría.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var usuarioId = SessionService.Instance.UsuarioActual?.Id ?? 0;
                await _categoriaService.ActivarAsync(CategoriaSeleccionada.Id, usuarioId);
                await CargarAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
