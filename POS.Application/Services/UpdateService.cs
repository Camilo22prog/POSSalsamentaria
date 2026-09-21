using POS.Application.Interfaces;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;

namespace POS.Application.Services
{
    public class UpdateService : IUpdateService
    {
        private readonly HttpClient _httpClient;
        private string _updateUrl;
        private readonly string _currentVersion;
        private readonly string _updateFolder;
        private string _latestVersion = string.Empty;
        private string _updateNotes = string.Empty;
        private string _downloadUrl = string.Empty;

        public void ConfigurarUrl(string? url)
        {
            _updateUrl = url ?? string.Empty;
        }

        public UpdateService()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(15)
            };
            
            _updateUrl = string.Empty; // Se configura desde Configuración del sistema
            
            // Obtener versión actual de la aplicación (del ejecutable de entrada, e.g., POS.UI)
            _currentVersion = (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly())
                .GetName()
                .Version?
                .ToString() ?? "1.0.0";
            
            // Carpeta temporal para descargas
            _updateFolder = Path.Combine(Path.GetTempPath(), "POSUpdates");
            Directory.CreateDirectory(_updateFolder);
        }

        public async Task<string> GetCurrentVersionAsync()
        {
            return await Task.FromResult(_currentVersion);
        }

        public async Task<string> GetLatestVersionAsync()
        {
            try
            {
                // Descargar archivo version.json desde el servidor
                var response = await _httpClient.GetStringAsync($"{_updateUrl}/version.json");
                var versionInfo = JsonSerializer.Deserialize<VersionInfo>(response);
                
                _latestVersion = versionInfo?.Version ?? _currentVersion;
                _updateNotes = versionInfo?.Notes ?? "Sin notas de actualización";
                _downloadUrl = versionInfo?.DownloadUrl ?? string.Empty;
                
                return _latestVersion;
            }
            catch
            {
                return _currentVersion;
            }
        }

        public async Task<bool> CheckForUpdatesAsync()
        {
            var latestVersion = await GetLatestVersionAsync();
            var currentVersion = await GetCurrentVersionAsync();
            
            return CompareVersions(latestVersion, currentVersion) > 0;
        }

        public async Task<bool> DownloadUpdateAsync(IProgress<int> progress)
        {
            try
            {
                var updateFileName = $"POS_Update_{_latestVersion}.zip";
                var downloadUrl = !string.IsNullOrWhiteSpace(_downloadUrl) 
                    ? _downloadUrl 
                    : $"{_updateUrl}/{updateFileName}";
                var localFilePath = Path.Combine(_updateFolder, updateFileName);

                // Eliminar archivo anterior si existe
                if (File.Exists(localFilePath))
                    File.Delete(localFilePath);

                using var response = await _httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                var canReportProgress = totalBytes != -1 && progress != null;

                using var contentStream = await response.Content.ReadAsStreamAsync();
                using var fileStream = new FileStream(localFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

                var buffer = new byte[8192];
                long totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead);
                    totalBytesRead += bytesRead;

                    if (canReportProgress)
                    {
                        var progressPercentage = (int)((totalBytesRead * 100) / totalBytes);
                        progress?.Report(progressPercentage);
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task ApplyUpdateAsync()
        {
            await Task.Run(() =>
            {
                var updateFileName = $"POS_Update_{_latestVersion}.zip";
                var zipPath = Path.Combine(_updateFolder, updateFileName);
                var extractPath = Path.Combine(_updateFolder, "extracted");

                // Extraer archivos
                if (Directory.Exists(extractPath))
                    Directory.Delete(extractPath, true);
                
                System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, extractPath);

                // Obtener ruta de la aplicación
                var appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? AppDomain.CurrentDomain.BaseDirectory;

                // Ejecutar updater
                var updaterPath = Path.Combine(appPath, "POS.Updater.exe");
                
                if (File.Exists(updaterPath))
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = updaterPath,
                        Arguments = $"\"{extractPath}\" \"{appPath}\"",
                        UseShellExecute = true,
                        Verb = "runas"
                    };

                    Process.Start(startInfo);
                    
                    // Cerrar aplicación actual
                    Environment.Exit(0);
                }
            });
        }

        public string GetUpdateNotes()
        {
            return _updateNotes;
        }

        private int CompareVersions(string version1, string version2)
        {
            var v1 = new Version(version1);
            var v2 = new Version(version2);
            return v1.CompareTo(v2);
        }

        private class VersionInfo
        {
            public string Version { get; set; } = string.Empty;
            public string Notes { get; set; } = string.Empty;
            public string DownloadUrl { get; set; } = string.Empty;
            public DateTime ReleaseDate { get; set; }
        }
    }
}