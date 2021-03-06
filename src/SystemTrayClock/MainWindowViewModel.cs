using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace SystemTrayClock
{
    public class MainWindowViewModel : ObservableRecipient
    {
        private DispatcherTimer _timer;

        public MainWindowViewModel()
        {
            LoadedCommand = new RelayCommand(Loaded);
            ClosingCommand = new RelayCommand<CancelEventArgs>(Closing);
            CloseCommand = new RelayCommand(Close);
            NotifyIconOpenCommand = new RelayCommand(Open);
            NotifyIconExitCommand = new RelayCommand(() => { Application.Current.Shutdown(); });
            App.WindowsThemeChanged += WindowsThemeChanged;
        }

        public ICommand LoadedCommand { get; }
        public ICommand ClosingCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand NotifyCommand { get; }
        public ICommand NotifyIconOpenCommand { get; }
        public ICommand NotifyIconExitCommand { get; }

        public Action ToggleMainWindowState;

        public void WindowsThemeChanged(object sender, ThemeChangedArgument e)
        {

        }


        public MainWindowState MainWindowState
        {
            get => _mainWindowState;
            set
            {
                ShowInTaskbar = true;
                SetProperty(ref _mainWindowState, value);
                ShowInTaskbar = value != MainWindowState.Closed;
            }
        }
        private MainWindowState _mainWindowState;

        public WindowState WindowState
        {
            get => _windowState;
            set
            {
                ShowInTaskbar = true;
                SetProperty(ref _windowState, value);
                ShowInTaskbar = value != WindowState.Minimized;
            }
        }
        private WindowState _windowState;

        public bool ShowInTaskbar
        {
            get => _showInTaskbar;
            set => SetProperty(ref _showInTaskbar, value);
        }
        private bool _showInTaskbar;

        public string CurrentTime
        {
            get { return _currentTime; }
            set { SetProperty(ref _currentTime, value); }
        }
        private string _currentTime;

        public string CurrentTimeZone
        {
            get { return _currentTimeZone; }
            set { SetProperty(ref _currentTimeZone, value); }
        }
        private string _currentTimeZone;

        public DateTime CurrentDate
        {
            get { return _currentDate; }
            set { SetProperty(ref _currentDate, value); }
        }
        private DateTime _currentDate;
        

        private void Loaded()
        {
            WindowState = WindowState.Minimized;
            MainWindowState = MainWindowState.Closed;
            ShowCurrentTime();
            InitTimer();
        }

        private void Open()
        {
            InitTimer();
            WindowState = WindowState.Normal;
            ToggleMainWindowState?.Invoke();
        }

        private void Close()
        {
            WindowState = WindowState.Minimized;
            StopTimer();
            ToggleMainWindowState?.Invoke();
        }

        private void Closing(CancelEventArgs e)
        {
            if (e == null)
                return;
            e.Cancel = true;
            WindowState = WindowState.Minimized;
            MainWindowState = MainWindowState.Closed;
            StopTimer();
        }

        public void InitTimer()
        {
            if (_timer != null && _timer.IsEnabled)
            {
                _timer.Stop();
                _timer = null;
            }
            _timer = new();
            _timer.Tick += new EventHandler(TimerTick);
            _timer.Interval = new TimeSpan(0, 0, 1);
            _timer.Start();
        }

        private void StopTimer()
        {
            if (_timer != null && _timer.IsEnabled)
            {
                _timer.Stop();
                _timer = null;
            }
        }

        private void TimerTick(object sender, EventArgs e)
        {
            ShowCurrentTime();
        }

        public void ShowCurrentTime()
        {
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                CurrentDate = DateTime.Now;
                CurrentTime = CurrentDate.ToString("h:mm:ss tt");
                TimeZoneInfo localZone = TimeZoneInfo.Local;
                CurrentTimeZone = localZone.IsDaylightSavingTime(CurrentDate) ? localZone.DaylightName : localZone.StandardName;
            });
        }
    }
}
