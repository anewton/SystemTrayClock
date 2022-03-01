using System;
using System.Threading;
using System.Windows;

namespace SystemTrayClock
{
    public partial class App : Application
    {
        private const string _uniqueEventName = "{18B16C61-D921-4D44-98D4-5F0FEDCD9FCC}";
        private const string _uniqueMutexName = "{86A1841E-C5F7-457B-9470-679663AE4BC6}";
        private readonly ThemeHelper _themeHelper = new ThemeHelper();
        internal static Action<object, ThemeChangedArgument> WindowsThemeChanged;
        private EventWaitHandle _eventWaitHandle;
        private Mutex _mutex;

        private void AppOnStartup(object sender, StartupEventArgs e)
        {
            _themeHelper.StartThemeWatching();
            _mutex = new Mutex(true, _uniqueMutexName, out bool isOwned);
            _eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, _uniqueEventName);

            // So, R# would not give a warning that this variable is not used.
            GC.KeepAlive(_mutex);

            if (isOwned)
            {
                // Spawn a thread which will be waiting for our event
                var thread = new Thread(
                    () =>
                    {
                        while (_eventWaitHandle.WaitOne())
                        {
                            Current.Dispatcher.BeginInvoke(
                                () => ((MainWindow)Current.MainWindow).BringToForeground());
                        }
                    })
                {
                    // It is important mark it as background otherwise it will prevent app from exiting.
                    IsBackground = true
                };
                thread.Start();
                return;
            }

            // Notify other instance so it could bring itself to foreground.
            _eventWaitHandle.Set();

            // Terminate this instance.
            Shutdown();
        }
    }
}