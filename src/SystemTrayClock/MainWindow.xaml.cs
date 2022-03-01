using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Animation;

namespace SystemTrayClock
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel mainWindowVM)
            {
                mainWindowVM.ToggleMainWindowState -= ToggleMainWindowState;
                mainWindowVM.ToggleMainWindowState += ToggleMainWindowState;
            }

            //To hide the window in the alt-tab apps when minimized
            WindowInteropHelper wndHelper = new(this);
            int exStyle = (int)GetWindowLong(wndHelper.Handle, (int)GetWindowLongFields.GWL_EXSTYLE);
            exStyle |= (int)ExtendedWindowStyles.WS_EX_TOOLWINDOW;
            SetWindowLong(wndHelper.Handle, (int)GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);
        }

        public void ToggleMainWindowState()
        {
            if (DataContext is MainWindowViewModel mainWindowVM)
            {
                switch (mainWindowVM.MainWindowState)
                {
                    case MainWindowState.Closed:
                        OnOpened();
                        mainWindowVM.MainWindowState = MainWindowState.Open;
                        break;
                    case MainWindowState.Open:
                        OnClosed();
                        mainWindowVM.MainWindowState = MainWindowState.Closed;
                        break;
                }
            }
        }

        public void OnOpened()
        {
            var margins = 2;
            var desktopWorkingArea = SystemParameters.WorkArea;
            Top = (desktopWorkingArea.Bottom - ActualHeight) - margins;
            var showMainWindowStoryboard = FindResource("showMainWindowStoryboard") as Storyboard;
            if (showMainWindowStoryboard.Children[0] is DoubleAnimation doubleAnimation)
            {
                doubleAnimation.From = desktopWorkingArea.Right;
                doubleAnimation.To = (desktopWorkingArea.Right - ActualWidth) - margins;
            }
            showMainWindowStoryboard.Begin();

            
        }

        public void OnClosed()
        {
            var margins = 2;
            var desktopWorkingArea = SystemParameters.WorkArea;
            Top = (desktopWorkingArea.Bottom - ActualHeight) - margins;
            var hideMainWindowStoryboard = FindResource("hideMainWindowStoryboard") as Storyboard;
            if (hideMainWindowStoryboard.Children[0] is DoubleAnimation doubleAnimation)
            {
                doubleAnimation.From = Left;
                doubleAnimation.To = desktopWorkingArea.Right;
            }
            hideMainWindowStoryboard.Begin();
        }

        public void BringToForeground()
        {
            if (WindowState == WindowState.Minimized || Visibility == Visibility.Hidden)
            {
                Show();
                WindowState = WindowState.Normal;
                OnOpened();
                if (DataContext is MainWindowViewModel mainWindowVM)
                {
                    mainWindowVM.ShowCurrentTime();
                    mainWindowVM.InitTimer();
                }
            }
            Activate();
            Topmost = true;
            Topmost = false;
            Focus();
        }

        [Flags]
        public enum ExtendedWindowStyles
        {
            WS_EX_TOOLWINDOW = 0x00000080,
        }

        public enum GetWindowLongFields
        {
            GWL_EXSTYLE = (-20),
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

        public static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            int error = 0;
            IntPtr result = IntPtr.Zero;
            // Win32 SetWindowLong doesn't clear error on success
            SetLastError(0);
            if (IntPtr.Size == 4)
            {
                // use SetWindowLong
                Int32 tempResult = IntSetWindowLong(hWnd, nIndex, IntPtrToInt32(dwNewLong));
                error = Marshal.GetLastWin32Error();
                result = new IntPtr(tempResult);
            }
            else
            {
                // use SetWindowLongPtr
                result = IntSetWindowLongPtr(hWnd, nIndex, dwNewLong);
                error = Marshal.GetLastWin32Error();
            }
            if ((result == IntPtr.Zero) && (error != 0))
            {
                throw new System.ComponentModel.Win32Exception(error);
            }
            return result;
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr IntSetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
        private static extern Int32 IntSetWindowLong(IntPtr hWnd, int nIndex, Int32 dwNewLong);

        private static int IntPtrToInt32(IntPtr intPtr)
        {
            return unchecked((int)intPtr.ToInt64());
        }

        [DllImport("kernel32.dll", EntryPoint = "SetLastError")]
        public static extern void SetLastError(int dwErrorCode);
    }
}
