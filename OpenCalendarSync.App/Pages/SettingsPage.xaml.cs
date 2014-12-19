using Ookii.Dialogs.Wpf;
using OpenCalendarSync.App.Tray.Pages.SettingsPages;
using OpenCalendarSync.App.Tray.Properties;
using OpenCalendarSync.Lib.Database;
using OpenCalendarSync.Lib.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OpenCalendarSync.App.Tray.Pages
{
    /// <summary>
    /// Logica di interazione per SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        

        public SettingsPage()
        {
            InitializeComponent();
            InternalInit();
        }

        private void InternalInit()
        {
            LibraryVersionLabel.Content = "Lib v" + Lib.Utilities.VersionHelper.LibraryVersion() +
                                            ", built " + Lib.Utilities.VersionHelper.LibraryBuildTime().ToString("s");
            ExecutingAssemblyVersionLabel.Content = "App v" + Lib.Utilities.VersionHelper.ExecutingAssemblyVersion() +
                                                    ", built " + Lib.Utilities.VersionHelper.ExecutingAssemblyBuildTime().ToString("s");
        }

        private void btSave_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Save();
            SettingsContentLoader.DoApply();
        }


    }
}
