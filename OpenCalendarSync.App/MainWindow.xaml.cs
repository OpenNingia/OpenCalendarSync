using System.Linq;
using MongoDB.Driver.Builders;
using OpenCalendarSync.App.Tray.Properties;
using OpenCalendarSync.Lib;
using OpenCalendarSync.Lib.Event;
using OpenCalendarSync.Lib.Manager;
using Hardcodet.Wpf.TaskbarNotification;
using log4net.Config;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Collections.Generic;
using System.IO;
using Squirrel;
using Ookii.Dialogs.Wpf;
using FirstFloor.ModernUI.Windows.Controls;
using System.Windows.Shell;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OpenCalendarSync.App.Tray
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ModernWindow
    {
        static bool _showTheWelcomeWizard;
        static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        UpdateManager _mgr;
        TaskbarItemInfo taskBarItem;

        public MainWindow()
        {
            XmlConfigurator.Configure(); //only once
            Log.Info("Application is starting");

            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                Log.Error("Exception", ex);    
            }

            // instantiate the taskbar item
            taskBarItem = new TaskbarItemInfo();

            NotificationManager.RegisterTaskBar(taskBarItem);

            // Create a Timer with a Normal Priority
            var timer = new DispatcherTimer {Interval = TimeSpan.FromMinutes(Settings.Default.RefreshRate)};

            // Set the callback to just show the time ticking away
            // NOTE: We are using a control so this has to run on 
            // the UI thread
            timer.Tick += delegate {
                StartSync();
            };            

            timer.Start();                       
        }

        public void AppendNotifyMessage(string title, string message)
        {
            NotificationManager.AppendNotifyMessage(title, message);
        }

        public void AppendErrorMessage(string title, string message)
        {
            NotificationManager.AppendErrorMessage(title, message);
        }

        private System.Drawing.Icon GetAppIcon(string name, System.Drawing.Size sz)
        {
            // se esiste la icona "doge" usa quella :)
            var dogeFile = Path.Combine("doge", name + ".ico");
            if (File.Exists(dogeFile))
                return new System.Drawing.Icon(dogeFile, sz);
            return (System.Drawing.Icon)Properties.Resources.ResourceManager.GetObject(name, Properties.Resources.Culture);
        }

        private void miExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void miSync_Click(object sender, RoutedEventArgs e)
        {
            StartSync();
        }

        public async void StartSync()
        {
            AppendNotifyMessage("Inizio sincronizzazione", "");

            taskBarItem.ProgressState = TaskbarItemProgressState.Indeterminate;

            var conv = new ImageSourceConverter();
            taskBarItem.Overlay = (ImageSource)conv.ConvertFrom(Properties.Resources.sync);

            await OutlookToGoogle();
            EndSync();
        }

        private void EndSync()
        {
            AppendNotifyMessage("Sincronizzazione terminata", "");
        }

        private void ProgressError()
        {
            taskBarItem.ProgressState = TaskbarItemProgressState.Error;
        }

        private void ProgressDone()
        {
            taskBarItem.ProgressState = TaskbarItemProgressState.Paused;
        }

        private void ProgressIdle()
        {
            taskBarItem.ProgressState = TaskbarItemProgressState.None;
        }

        private async Task OutlookToGoogle()
        {
            // settings
            var clientId = Settings.Default.ClientID;
            var secret    = Settings.Default.ClientSecret;
            var calName  = Settings.Default.CalendarName;
            var calId    = Settings.Default.CalendarID;   // note: on first startup, this is null or empty.
            if ( string.IsNullOrEmpty(clientId) )
                clientId = GoogleToken.ClientId;
            if ( string.IsNullOrEmpty(secret) )
                secret    = GoogleToken.ClientSecret;
            if (string.IsNullOrEmpty(calName))
                calName = "GVR.Meetings";
            //
            ICalendar calendar;
            //
            try
            {
                // take events from outlook and push em to google
                calendar = await OutlookCalendarManager.Instance.PullAsync() as GenericCalendar;
            }
            catch (Exception ex)
            {
                SyncFailure(ex.Message);
                return;
            }
            //
            try
            {
                // if I'm not logged in
                if (!GoogleCalendarManager.Instance.LoggedIn)
                {
                    var login = await GoogleCalendarManager.Instance.Login(clientId, secret);
                }
                // initialize google calendar (i.e.: create it if it's not present, just get it if it's present)
                var googleCalId = await GoogleCalendarManager.Instance.Initialize(calId, calName);
                if (calId != googleCalId) // if the calendar ids differ
                {
                    // update the settings, so that the next time we start, we have a calendarId
                    Settings.Default.CalendarID = googleCalId;
                    Settings.Default.Save();
                }
                //logged in to google, go on!
                if (GoogleCalendarManager.Instance.LoggedIn) 
                {
                    try
                    {
                        var ret = await GoogleCalendarManager.Instance.PushAsync(calendar);
                        SyncSuccess(ret);
                    }
                    catch (PushException ex)
                    {
                        SyncFailure(ex.Message);
                    }
                    catch (Exception ex)
                    {
                        SyncFailure(ex.Message);
                    }
                }
            }
            catch(Exception ex)
            {
                Log.Error("Exception", ex);
                SyncFailure(ex.Message);
            }
        }

        private void SyncSuccess(IEnumerable<UpdateOutcome> pushedEvents)
        {
            const string title = "Risultato sincronizzazione";
            var text  = "La sincronizzazione e' terminata con successo";
            var events = pushedEvents as List<UpdateOutcome>;
            if (events != null && events.Count > 0)
            {
                if(events.Count(e => e.Successful && e.Event.Action == EventAction.Add) > 0)
                {
                    text += "\n" + String.Format("{0} eventi aggiunti al calendario", events.Count(e => e.Successful && e.Event.Action == EventAction.Add));
                }
                if (events.Count(e => e.Successful && e.Event.Action == EventAction.Update) > 0)
                {
                    text += "\n" + String.Format("{0} eventi aggiornati", events.Count(e => e.Successful && e.Event.Action == EventAction.Update));
                }
                if (events.Count(e => e.Event.Action == EventAction.Remove) > 0)
                {
                    text += "\n" + String.Format("{0} eventi rimossi", events.Count(e => e.Event.Action == EventAction.Remove));
                }

                AppendNotifyMessage(title, text);

                ProgressDone();
            } else { ProgressIdle(); }
        }

        private void SyncFailure(string message)
        {
            const string title = "Risultato sincronizzazione";
            var text = "La sincronizzazione e' fallita\n";
            var details = message;
            text += details;

            ProgressError();

            AppendErrorMessage(title, text);
        }

        private void miSettings_Click(object sender, RoutedEventArgs e)
        {
            ShowSettingsDialog();
        }

        public void ShowSettingsDialog()
        {
            ContentSource = new Uri("Pages/SettingsPage.xaml", UriKind.Relative);
        }        

        private async void Window_Initialized(object sender, EventArgs e)
        {
            Log.Debug(String.Format("Window_Initialized: sender[{0}], args [{1}]", sender, e));

            // todo: avoid to login to google if there's any squirrel event
            var clientId = Settings.Default.ClientID;
            var secret = Settings.Default.ClientSecret;
            if (string.IsNullOrEmpty(clientId))
                clientId = GoogleToken.ClientId;
            if (string.IsNullOrEmpty(secret))
                secret = GoogleToken.ClientSecret;

            if(!GoogleCalendarManager.Instance.LoggedIn)
            {
                var login = await GoogleCalendarManager.Instance.Login(clientId, secret);
            }

            var repo = Settings.Default.UpdateRepositoryPath;
            if (string.IsNullOrEmpty(Settings.Default.UpdateRepositoryPath))
                repo = "null";

            using (var manager = new UpdateManager(repo, "OpenCalendarSync", FrameworkVersion.Net45))
            {
                // Note, in most of these scenarios, the app exits after this method
                // completes!
                SquirrelAwareApp.HandleEvents(
                onInitialInstall: v =>
                {
                    Log.Info(String.Format("Application installed {0}", v.ToString()));
                    AppendNotifyMessage("Installazione", String.Format("OpenCalendarSync {0} installato.", v.ToString()));
                    manager.CreateShortcutForThisExe();
                },
                onAppUpdate: v =>
                {
                    Log.Info(String.Format("Application updated to {0}, trying to upgrade settings", v.ToString()));
                    try
                    {
                        Settings.Default.Upgrade();
                        Settings.Default.Save();
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Error while upgrading settings, you'll have to merge them manually", ex);
                    }

                    AppendNotifyMessage("Aggiornamento", String.Format("OpenCalendarSync aggiornato alla versione {0}", v.ToString()));                    
                    manager.CreateShortcutForThisExe();
                },
                onAppUninstall: v => manager.RemoveShortcutForThisExe(),
                onFirstRun: () => _showTheWelcomeWizard = true);
            }

            if (!_showTheWelcomeWizard)
            {
                ContentSource = new Uri("Pages/Notifies.xaml", UriKind.Relative);
                return;
            }
            
            using (var welcomeDialog = new TaskDialog())
            {
                welcomeDialog.Buttons.Add(new TaskDialogButton(ButtonType.Ok));
                welcomeDialog.WindowTitle = "Primo Avvio";
                welcomeDialog.MainIcon = TaskDialogIcon.Information;
                welcomeDialog.MainInstruction +=
                    "Benvenuto in OpenCalendarSync";
                welcomeDialog.Content +=
                    "Questo programma serve per importare il tuo calendario Microsoft Outlook nel servizio Google Calendar\n\n" +
                    "Per iniziare basta premere con qualsiasi tasto del mouse sull'icona che e' apparsa nella barra delle notifiche e\n" +
                    "e selezionare \"Sincronizza ora\", questo creera' un calendario su Google Calendar e procedera'\n" +
                    "con l'importazione del calendario attuale di Microsoft Outlook\n";

                welcomeDialog.ShowDialog();
            }
        }

        private ProgressDialog _updateDialog;
        private static bool _updateFinished;

        private async void MiUpdate_OnClick(object sender, RoutedEventArgs e)
        {
            await CheckUpdates();
        }

        public async Task CheckUpdates()
        {
            if (string.IsNullOrEmpty(Settings.Default.UpdateRepositoryPath))
            {
                Log.Info("No repository path is set, won't update");
                return;
            }

            try
            {
                _mgr = new UpdateManager(Settings.Default.UpdateRepositoryPath, "OpenCalendarSync", FrameworkVersion.Net45);
                var updateInfo = await _mgr.CheckForUpdate();
                if (!updateInfo.ReleasesToApply.Any())
                {
                    Log.Info("No newer version found");
                    return;
                }

                Log.Info(String.Format("Newer version found => [{0}]", updateInfo.FutureReleaseEntry.EntryAsString));

                var updateAvailableDialog = new TaskDialog
                {
                    MainIcon = TaskDialogIcon.Information,
                    WindowTitle = "Nuova versione disponibile",
                    MainInstruction = "Procedere con l'installazione?",
                    Content = String.Format("Versione {0} disponibile.\nSelezionare una delle due opzioni.", updateInfo.FutureReleaseEntry.EntryAsString)
                };
                updateAvailableDialog.Buttons.Add(new TaskDialogButton(ButtonType.Yes));
                updateAvailableDialog.Buttons.Add(new TaskDialogButton(ButtonType.No));
                var userDecision = updateAvailableDialog.Show();

                if (userDecision.ButtonType != ButtonType.Yes)
                {
                    Log.Info("User has selected to abort the update");
                    return;
                }

                Log.Info("User has selected to proceed with the update");

                _updateDialog = new ProgressDialog();
                _updateDialog.WindowTitle += "Aggiornamento a nuova versione";
                _updateDialog.Text += "Installazione in corso dell'ultima versione...";
                _updateDialog.DoWork += (o, args) =>
                {
                    _updateFinished = false;
                    var updateAppTask = _mgr.UpdateApp(i =>
                    {
                        if ((i >= 0 && i <= 100) && !_updateFinished)
                        {
                            _updateDialog.ReportProgress(i);
                        }
                    });
                    Log.Debug(String.Format("{0} applied.", updateAppTask.Result.EntryAsString));
                };
                _updateDialog.RunWorkerCompleted += (o, args) =>
                {
                    Log.Info("Finished updating app to the latest version.");
                    _updateFinished = true;
                    _mgr = null;
                };
                _updateDialog.Show();
            }
            catch (Exception ex)
            {
                Log.Error("Failed to check or apply update", ex);
                AppendErrorMessage("Ricerca aggiornamenti", "Ricerca aggiornamenti o applicazione aggiornamento fallita");
            }
        }

        private void ModernWindow_Loaded(object sender, RoutedEventArgs e)
        {
            AppendNotifyMessage("OpenCalendarSync pronto.", "");
        }

        private void ModernWindow_Activated(object sender, EventArgs e)
        {
            NotificationManager.StopBlink();
        }
    }
}
