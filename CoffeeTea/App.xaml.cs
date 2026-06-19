using System.Windows;
using CoffeeTea.Services;

namespace CoffeeTea
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            ApplySavedTheme();
            base.OnStartup(e);
        }

        private static void ApplySavedTheme()
        {
            try
            {
                SettingsStorageService settingsStorageService = new SettingsStorageService();
                ThemeManager.ApplyTheme(settingsStorageService.Load().IsDarkTheme);
            }
            catch
            {
                ThemeManager.ApplyTheme(false);
            }
        }
    }
}

