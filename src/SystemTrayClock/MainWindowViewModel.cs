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
        }

        public ICommand LoadedCommand { get; }
        public ICommand ClosingCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand NotifyCommand { get; }
        public ICommand NotifyIconOpenCommand { get; }
        public ICommand NotifyIconExitCommand { get; }

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
            ShowCurrentTime();
            InitTimer();
        }

        private void Open()
        {
            InitTimer();
            WindowState = WindowState.Normal;
        }

        private void Close()
        {
            WindowState = WindowState.Minimized;
            StopTimer();
        }

        private void Closing(CancelEventArgs e)
        {
            if (e == null)
                return;
            e.Cancel = true;
            WindowState = WindowState.Minimized;
            StopTimer();
        }

        private void InitTimer()
        {
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

        private void ShowCurrentTime()
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
