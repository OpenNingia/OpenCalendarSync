using OpenCalendarSync.App.Tray.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shell;
using System.Windows.Threading;

namespace OpenCalendarSync.App.Tray
{
    static class NotificationManager
    {
        // instantiate Type => page dictionary
        static Dictionary<Type, IMessageAppender> Appenders;
        static TaskbarItemInfo taskBar;
        static DispatcherTimer blinkTimer;
        static bool errorBlink;

        static NotificationManager()
        {
            Appenders = new Dictionary<Type, IMessageAppender>();
            blinkTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle);
            blinkTimer.Interval = new TimeSpan(0, 0, 1);
            blinkTimer.Tick += blinkTimer_Tick;

        }

        private static void blinkTimer_Tick(object sender, EventArgs e)
        {
            if (taskBar == null)
                return;

            if (taskBar.ProgressState != TaskbarItemProgressState.Normal)
                taskBar.ProgressState = TaskbarItemProgressState.Normal;
            else if (errorBlink)
                taskBar.ProgressState = TaskbarItemProgressState.Error;
            else
                taskBar.ProgressState = TaskbarItemProgressState.Paused;
        }

        public static void RegisterAppender(Type appenderType, IMessageAppender instance)
        {
            Appenders.Add(appenderType, instance);
        }

        public static void RegisterTaskBar(TaskbarItemInfo tb)
        {
            taskBar = tb;
        }

        public static bool AppendMessage(Type appenderType, string title, string message)
        {
            if (!Appenders.ContainsKey(appenderType))
                return false;

            var header = string.Format("{0} {1} {2}", DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString(), title);
            Appenders[appenderType].AppendMessage(header, message);
            return true;
        }

        public static void AppendNotifyMessage(string title, string message)
        {
            AppendMessage(typeof(OpenCalendarSync.App.Tray.Pages.Notifies), title, message);
            //StartBlink(false);
        }

        public static void AppendErrorMessage(string title, string message)
        {
            AppendMessage(typeof(OpenCalendarSync.App.Tray.Pages.Errors), title, message);
            //StartBlink(true);
        }

        public static void StopBlink()
        {
            blinkTimer.Stop();
        }

        public static void StartBlink(bool error)
        {
            errorBlink = error;
            blinkTimer.Start();

            taskBar.ProgressValue = 0.9;
        }
    }
}
