using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Irvuewin.Models;
using Serilog;
using Irvuewin.Helpers;
using Irvuewin.Helpers.Events;

namespace Irvuewin.ViewModels
{
    public class UpdateWindowViewModel : INotifyPropertyChanged
    {
        private static readonly ILogger Logger = Log.ForContext<UpdateWindowViewModel>();

        public event PropertyChangedEventHandler? PropertyChanged;

        private readonly GitHubRelease _release;
        private readonly Window _window;

        public string VersionName => _release.name;
        public string ReleaseNotes => _release.body;

        private bool _isDownloading;
        public bool IsDownloading
        {
            get => _isDownloading;
            set
            {
                if (_isDownloading == value) return;
                _isDownloading = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotDownloading));
            }
        }

        public bool IsNotDownloading => !IsDownloading;

        private double _progressValue;
        public double ProgressValue
        {
            get => _progressValue;
            set
            {
                if (Math.Abs(_progressValue - value) < 0.1) return;
                _progressValue = value;
                OnPropertyChanged();
            }
        }

        private string _progressText = "";
        public string ProgressText
        {
            get => _progressText;
            set
            {
                if (_progressText == value) return;
                _progressText = value;
                OnPropertyChanged();
            }
        }

        public ICommand CloseCommand { get; }
        public ICommand DownloadCommand { get; }

        public UpdateWindowViewModel(GitHubRelease release, Window window)
        {
            _release = release;
            _window = window;

            CloseCommand = new RelayCommand((_) => OnClose());
            DownloadCommand = new AsyncRelayCommand(OnDownloadAsync);
        }

        private void OnClose()
        {
            _window.Close();
        }

        private async Task OnDownloadAsync()
        {
            var exeAsset = _release.assets?.FirstOrDefault(a => a.name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase));
            if (exeAsset == null)
            {
                // Fallback to browser
                ICommonCommands.OpenUrl(_release.html_url);
                _window.Close();
                return;
            }

            IsDownloading = true;
            ProgressValue = 0;
            ProgressText = Helpers.Localization.Instance["UpdateWindow_Starting"] ?? "Starting download...";

            var tempFile = Path.Combine(Path.GetTempPath(), exeAsset.name);

            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "Irvue-win");

                using var response = await client.GetAsync(exeAsset.browser_download_url, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                var canReportProgress = totalBytes != -1;

                using var stream = await response.Content.ReadAsStreamAsync();
                using var fileStream = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

                var buffer = new byte[8192];
                var totalRead = 0L;
                int read;

                while ((read = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, read);
                    totalRead += read;

                    if (canReportProgress)
                    {
                        var percentage = (double)totalRead / totalBytes * 100;
                        if (percentage - ProgressValue >= 1 || percentage == 100)
                        {
                            ProgressValue = percentage;
                            ProgressText = string.Format(Helpers.Localization.Instance["UpdateWindow_Downloading"] ?? "Downloading: {0}%", ProgressValue.ToString("F0"));
                        }
                    }
                    else
                    {
                        ProgressText = $"Downloaded {totalRead / 1024} KB";
                    }
                }

                fileStream.Close(); // Explicitly close to release lock

                Process.Start(new ProcessStartInfo(tempFile) { UseShellExecute = true });
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to download update");
                MessageBox.Show(Helpers.Localization.Instance["UpdateWindow_Error"] ?? "Error downloading update.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                IsDownloading = false;
            }
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
