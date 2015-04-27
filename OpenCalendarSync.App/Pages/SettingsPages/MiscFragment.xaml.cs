using Ookii.Dialogs.Wpf;
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

namespace OpenCalendarSync.App.Tray.Pages.SettingsPages
{
    /// <summary>
    /// Logica di interazione per MiscFragment.xaml
    /// </summary>
    public partial class MiscFragment : UserControl, ISettingsProvider
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public MiscFragment()
        {
            InitializeComponent();
            InternalInit();
        }

        private void InternalInit()
        {

        }

        private void UpdatesRepositoryTextBox_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog();
            if (!string.IsNullOrEmpty(Settings.Default.UpdateRepositoryPath))
                dialog.SelectedPath = Settings.Default.UpdateRepositoryPath;
            var result = dialog.ShowDialog();
            if (!result.HasValue) return;
            if (!result.Value) return;
            Settings.Default.UpdateRepositoryPath = dialog.SelectedPath;
        }

        private async void btReset_Click(object sender, RoutedEventArgs e)
        {
            var askForReset = MessageBox.Show("L'operazione di reset comporta:\n" +
                                                "\t1. Cancellazione database appuntamenti\n" +
                                                "\t2. Cancellazione calendario su google (opzionale)\n" +
                                                "Vuoi proseguire?",
                                                "Reset",
                                                MessageBoxButton.YesNo,
                                                MessageBoxImage.Exclamation,
                                                MessageBoxResult.No);

            if (askForReset != MessageBoxResult.Yes) return;
            if (!GoogleCalendarManager.Instance.LoggedIn)
            {
                var res = await GoogleCalendarManager.Instance.Login(Settings.Default.ClientID, Settings.Default.ClientSecret);
                if (!res)
                {
                    Log.Error("Couldn't log in to google services with the provided clientId and clientSecret");
                    NotificationManager.AppendErrorMessage("Login non riuscito", "Login a servizi google non effettuato.");
                    return;
                }
            }
            Reset();
        }

        private async void Reset()
        {
            const string title = "Risultato reset";
            var text = "";
            var drop = Storage.Instance.Appointments.Drop();
            
            if (drop.Ok)
            {
                text += "\tDatabase appuntamenti svuotato correttamente\n";
            }
            else
            {
                text += "\tDatabase appuntamenti *NON* svuotato!\n";
                Log.Error("Failed to delete drop appointments database");
            }
            try
            {
                var res = MessageBox.Show("Vuoi cancellare il calendario su google?",
                                           "Calendario Google",
                                           MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
                if (res == MessageBoxResult.Yes)
                {
                    var calendarDrop = await GoogleCalendarManager.Instance.DropCurrentCalendar();
                    text += "\tCalendario su google calendar cancellato correttamente\n";
                    if (!string.IsNullOrEmpty(calendarDrop))
                        text += "\t\tDettagli operazione: " + calendarDrop + "\n";
                }
            }
            catch (Exception ex)
            {
                text += "\tCalendario su google calendar *NON* cancellato\n";
                Log.Error("Failed to delete calendar from google", ex);
            }
            Settings.Default.CalendarID = "";
            text += "\tID del calendario resettato";

            NotificationManager.AppendNotifyMessage(title, text);
        }

        public void Apply()
        {
            
        }
    }
}
