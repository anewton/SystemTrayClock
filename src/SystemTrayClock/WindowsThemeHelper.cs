using Microsoft.Win32;
using System;
using System.Globalization;
using System.Management;
using System.Security.Principal;
using System.Windows;

namespace SystemTrayClock
{
    public enum WindowsTheme
    {
        Default = 0,
        Light = 1,
        Dark = 2,
        HighContrast = 3
    }

    public class ThemeHelper
    {
        private const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
        private const string RegistryValueName = "AppsUseLightTheme";

        private static WindowsTheme _windowsTheme;

        public WindowsTheme WindowsTheme
        {
            get { return _windowsTheme; }
            set { _windowsTheme = value; }
        }

        public void StartThemeWatching()
        {
            var currentUser = WindowsIdentity.GetCurrent();
            string query = string.Format(
                CultureInfo.InvariantCulture,
                @"SELECT * FROM RegistryValueChangeEvent WHERE Hive = 'HKEY_USERS' AND KeyPath = '{0}\\{1}' AND ValueName = '{2}'",
                currentUser.User.Value,
                RegistryKeyPath.Replace(@"\", @"\\"),
                RegistryValueName);

            try
            {
                _windowsTheme = GetWindowsTheme();
                MergeThemeDictionaries(_windowsTheme);
                var watcher = new ManagementEventWatcher(query);
                watcher.EventArrived += Watcher_EventArrived;
                SystemParameters.StaticPropertyChanged += SystemParameters_StaticPropertyChanged;
                // Start listening for events
                watcher.Start();
            }
            catch
            {
                // This can fail on Windows 7
                _windowsTheme = WindowsTheme.Default;
            }
        }

        private void MergeThemeDictionaries(WindowsTheme windowsTheme)
        {
            string appTheme = "LightTheme";
            switch (windowsTheme)
            {
                case WindowsTheme.Light:
                    appTheme = "LightTheme";
                    break;
                case WindowsTheme.Dark:
                    appTheme = "DarkTheme";
                    break;
                case WindowsTheme.HighContrast:
                    appTheme = "HighContrastTheme";
                    break;
                case WindowsTheme.Default:
                    appTheme = "LightTheme";
                    break;
            }
            App.Current.Resources.MergedDictionaries[0].Source = new Uri($"/Themes/{appTheme}.xaml", UriKind.Relative);
        }

        private void SystemParameters_StaticPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            _windowsTheme = GetWindowsTheme();
            MergeThemeDictionaries(_windowsTheme);
            ThemeChangedArgument themeChangedArgument = new ThemeChangedArgument();
            themeChangedArgument.WindowsTheme = _windowsTheme;
            App.WindowsThemeChanged?.Invoke(this, themeChangedArgument);
        }

        private void Watcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            _windowsTheme = GetWindowsTheme();
            MergeThemeDictionaries(_windowsTheme);
            ThemeChangedArgument themeChangedArgument = new();
            themeChangedArgument.WindowsTheme = _windowsTheme;
            App.WindowsThemeChanged?.Invoke(this, themeChangedArgument);
        }

        public WindowsTheme GetWindowsTheme()
        {
            WindowsTheme theme = WindowsTheme.Light;

            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath))
                {
                    object registryValueObject = key?.GetValue(RegistryValueName);
                    if (registryValueObject == null)
                    {
                        return WindowsTheme.Light;
                    }

                    int registryValue = (int)registryValueObject;

                    if (SystemParameters.HighContrast)
                        theme = WindowsTheme.HighContrast;

                    theme = registryValue > 0 ? WindowsTheme.Light : WindowsTheme.Dark;
                }

                return theme;
            }
            catch
            {
                return theme;
            }
        }
    }
}
