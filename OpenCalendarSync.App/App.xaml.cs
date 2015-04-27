using Microsoft.Shell;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Shell;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace OpenCalendarSync.App.Tray
{

    /// <summary>
    /// Logica di interazione per App.xaml
    /// </summary>
    public partial class App : Application, ISingleInstanceApp
    {
        /*protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            // Allow single instance code to perform cleanup operations
            SingleInstance<App>.Cleanup();
        }*/

        [STAThread]
        public static void Main()
        {
            if (SingleInstance<App>.InitializeAsFirstInstance("OpenCalendarSync.App"))
            {
                var app = new App();
                app.Init();
                app.Run();

                // Allow single instance code to perform cleanup operations
                SingleInstance<App>.Cleanup();
            }
        }

        private void Init()
        {            
            InitializeComponent();
            BuildJumplist();
        }

        protected override void OnStartup(StartupEventArgs e)
        {

        }

        private async void HandleCommands(IList<string> args)
        {
            var mw = MainWindow as MainWindow;
            if (mw == null)
                return;

            if (args.Contains("/update"))
            {
                await mw.CheckUpdates();
            }

            if (args.Contains("/sync") )
            {
                mw.StartSync();
            }

            if (args.Contains("/settings"))
            {
                mw.ShowSettingsDialog();
            }   
        }

        private void BuildJumplist()
        {
            JumpTask tskSyncNow = new JumpTask
            {
                Title = "Sincronizza ora",
                Arguments = "/sync",
                Description = "Effettua immediatamente una nuova sincronizzazione",
                CustomCategory = "Actions",
                ApplicationPath = Assembly.GetEntryAssembly().Location,
                IconResourcePath = Assembly.GetEntryAssembly().Location,
                IconResourceIndex = 1
            };

            JumpTask tskCheckUpdate = new JumpTask
            {
                Title = "Controlla aggiornamenti",
                Arguments = "/update",
                Description = "Controlla la presenza di una nuova versione sul repository",
                CustomCategory = "Actions",
                ApplicationPath = Assembly.GetEntryAssembly().Location,
                IconResourcePath = Assembly.GetEntryAssembly().Location,
                IconResourceIndex = 2
            };

            JumpTask tskSettings = new JumpTask
            {
                Title = "Cambia impostazioni",
                Arguments = "/settings",
                Description = "Personalizza le impostazioni dell'applicazione",
                CustomCategory = "Actions",
                ApplicationPath = Assembly.GetEntryAssembly().Location,
                IconResourcePath = Assembly.GetEntryAssembly().Location,
                IconResourceIndex = 3
            };

            JumpList jumpList = new JumpList();            

            jumpList.JumpItems.Add(tskSyncNow);
            jumpList.JumpItems.Add(tskCheckUpdate);
            jumpList.JumpItems.Add(tskSettings);
            jumpList.ShowFrequentCategory = false;
            jumpList.ShowRecentCategory = false;

            JumpList.SetJumpList(Application.Current, jumpList);

            jumpList.Apply();
        }

        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            HandleCommands(args);
            return true;
        }
    }
}
