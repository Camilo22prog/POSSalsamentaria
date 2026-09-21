namespace POS.Application.Interfaces
{
    public interface IUpdateService
    {
        void ConfigurarUrl(string? url);
        Task<bool> CheckForUpdatesAsync();
        Task<string> GetLatestVersionAsync();
        Task<string> GetCurrentVersionAsync();
        Task<bool> DownloadUpdateAsync(IProgress<int> progress);
        Task ApplyUpdateAsync();
        string GetUpdateNotes();
    }
}