using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Application.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace POS.UI.ViewModels
{
    public partial class UpdateViewModel : ObservableObject
    {
        private readonly IUpdateService _updateService;

        [ObservableProperty]
        private string _currentVersion = string.Empty;

        [ObservableProperty]
        private string _latestVersion = string.Empty;

        [ObservableProperty]
        private string _updateNotes = string.Empty;

        [ObservableProperty]
        private bool _isChecking = false;

        [ObservableProperty]
        private bool _isDownloading = false;

        [ObservableProperty]
        private bool _updateAvailable = false;

        [ObservableProperty]
        private int _downloadProgress = 0;

        [ObservableProperty]
        private string _statusMessage = "Verificando actualizaciones...";

        public UpdateViewModel(IUpdateService updateService)
        {
            _updateService = updateService;
            _ = CheckForUpdatesAsync();
        }

        [RelayCommand]
        private async Task CheckForUpdatesAsync()
        {
            IsChecking = true;
            StatusMessage = "Verificando actualizaciones...";

            try
            {
                CurrentVersion = await _updateService.GetCurrentVersionAsync();
                
                var hasUpdate = await _updateService.CheckForUpdatesAsync();

                if (hasUpdate)
                {
                    LatestVersion = await _updateService.GetLatestVersionAsync();
                    UpdateNotes = _updateService.GetUpdateNotes();
                    UpdateAvailable = true;
                    StatusMessage = $"Nueva versión disponible: {LatestVersion}";
                }
                else
                {
                    UpdateAvailable = false;
                    StatusMessage = "El sistema está actualizado";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al verificar actualizaciones: {ex.Message}";
                UpdateAvailable = false;
            }
            finally
            {
                IsChecking = false;
            }
        }

        [RelayCommand]
        private async Task DownloadAndInstallAsync()
        {
            var result = MessageBox.Show(
                $"Se descargará e instalará la versión {LatestVersion}.\n\n" +
                "La aplicación se cerrará durante el proceso.\n\n" +
                "¿Desea continuar?",
                "Confirmar Actualización",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            IsDownloading = true;
            StatusMessage = "Descargando actualización...";

            try
            {
                var progress = new Progress<int>(value =>
                {
                    DownloadProgress = value;
                    StatusMessage = $"Descargando actualización... {value}%";
                });

                var success = await _updateService.DownloadUpdateAsync(progress);

                if (success)
                {
                    StatusMessage = "Aplicando actualización...";
                    
                    // Aplicar actualización (esto cerrará la app)
                    await _updateService.ApplyUpdateAsync();
                }
                else
                {
                    StatusMessage = "Error al descargar la actualización";
                    MessageBox.Show(
                        "No se pudo descargar la actualización. Intente nuevamente más tarde.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                MessageBox.Show(
                    $"Error durante la actualización:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsDownloading = false;
            }
        }

        [RelayCommand]
        private void Close()
        {
            System.Windows.Application.Current.Windows
                .OfType<Window>()
                .FirstOrDefault(w => w.DataContext == this)
                ?.Close();
        }
    }
}