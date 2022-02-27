using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;

namespace SystemTrayClock
{
    public class NotifyIconHelper : FrameworkElement, IDisposable
    {
        private readonly NotifyIcon _notifyIcon;

        public NotifyIconHelper()
        {
            if (DesignerProperties.GetIsInDesignMode(this))
                return;
            _notifyIcon = new NotifyIcon
            {
                Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location),
                Visible = true,
                ContextMenuStrip = CreateContextMenu()
            };
            _notifyIcon.Click += OpenItemOnClick;
            System.Windows.Application.Current.Exit += (obj, args) => { _notifyIcon.Dispose(); };
        }

        private static readonly RoutedEvent OpenSelectedEvent = EventManager.RegisterRoutedEvent("OpenSelected",
            RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(NotifyIconHelper));

        private static readonly RoutedEvent ExitSelectedEvent = EventManager.RegisterRoutedEvent("ExitSelected",
            RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(NotifyIconHelper));

        public void Dispose()
        {
            _notifyIcon.Dispose();
        }

        public event RoutedEventHandler OpenSelected
        {
            add => AddHandler(OpenSelectedEvent, value);
            remove => RemoveHandler(OpenSelectedEvent, value);
        }

        public event RoutedEventHandler ExitSelected
        {
            add => AddHandler(ExitSelectedEvent, value);
            remove => RemoveHandler(ExitSelectedEvent, value);
        }

        private ContextMenuStrip CreateContextMenu()
        {
            var exitItem = new ToolStripMenuItem("Exit");
            exitItem.Click += ExitItemOnClick;
            var contextMenu = new ContextMenuStrip { Items = { exitItem } };
            return contextMenu;
        }

        private void OpenItemOnClick(object sender, EventArgs e)
        {
            if (((MouseEventArgs)e).Button == MouseButtons.Left)
            {
                var args = new RoutedEventArgs(OpenSelectedEvent);
                RaiseEvent(args);
            }
        }

        private void ExitItemOnClick(object sender, EventArgs e)
        {
            var args = new RoutedEventArgs(ExitSelectedEvent);
            RaiseEvent(args);
        }
    }
}
