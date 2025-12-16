using System.ComponentModel;
using System.Globalization;
using System.Resources;
using System.Windows.Data;

namespace Irvuewin.Helpers
{
    public class Localization : INotifyPropertyChanged
    {
        private static readonly Localization _instance = new();
        public static Localization Instance => _instance;

        private readonly ResourceManager _resourceManager = Properties.Resources.ResourceManager;

        public string this[string key]
        {
            get
            {
                try
                {
                    return _resourceManager.GetString(key, Properties.Resources.Culture) ?? $"[{key}]";
                }
                catch
                {
                    return $"[{key}]";
                }
            }
        }

        public void SetCulture(string cultureCode)
        {
            if (string.IsNullOrEmpty(cultureCode) || cultureCode == "Auto")
            {
                Properties.Resources.Culture = null; // Use OS default
                // Optionally reset thread culture to OS default if needed, or leave as is if "Auto" means "System"
                // But for hot reload "Auto" usually means "match system", so checking InstalledUICulture.
                // However, Properties.Resources.Culture being null makes ResourceManager use CurrentUICulture.
                // So we should set CurrentUICulture to InstalledUICulture or similar if "Auto".
                // Simple approach: Set to InstalledUICulture if Auto.
                 Thread.CurrentThread.CurrentUICulture = CultureInfo.InstalledUICulture;
                 Thread.CurrentThread.CurrentCulture = CultureInfo.InstalledUICulture;
            }
            else
            {
                try 
                {
                    var culture = new CultureInfo(cultureCode);
                    Properties.Resources.Culture = culture;
                    Thread.CurrentThread.CurrentUICulture = culture;
                    Thread.CurrentThread.CurrentCulture = culture;
                }
                catch { /* Ignore invalid culture */ }
            }

            // Notify that all indexed properties have changed
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Binding.IndexerName));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
