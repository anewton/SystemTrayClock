using System;
using System.Windows;

namespace SystemTrayClock
{
    /// <summary>
    /// Use this Window for testing the MoonDisk control, change StartupUri in App.xaml
    /// </summary>
    public partial class MoonWindow : Window
    {
        public MoonWindow()
        {
            InitializeComponent();
            Loaded += MoonWindow_Loaded;
        }

        private void MoonWindow_Loaded(object sender, RoutedEventArgs e)
        {
            moonDisk.CurrentDate = DateTime.Now;
        }
    }
}
