using System.Reflection;
using System.Windows;
using System.Windows.Shell;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace OpenCalendarSync.App.Tray
{

    /// <summary>
    /// Logica di interazione per App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            if (e.Args.Length > 0)
            {
                HandleCommands(e.Args);
                Shutdown();
            }
            else
            {
                BuildJumplist();
            }
        }

        private void HandleCommands(string[] args)
        {

        }

        private void BuildJumplist()
        {
            JumpTask tskCheckUpdate = new JumpTask
            {
                Title = "Controlla aggiornamenti",
                Arguments = "/update",
                Description = "Controlla la presenza di una nuova versione sul repository",
                CustomCategory = "Actions",
                IconResourcePath = Assembly.GetEntryAssembly().Location,
                ApplicationPath = Assembly.GetEntryAssembly().Location
            };

            JumpTask tskSyncNow = new JumpTask
            {
                Title = "Sincronizza ora",
                Arguments = "/sync",
                Description = "Effettua immediatamente una nuova sincronizzazione",
                CustomCategory = "Actions",
                IconResourcePath = Assembly.GetEntryAssembly().Location,
                ApplicationPath = Assembly.GetEntryAssembly().Location
            };

            JumpList jumpList = new JumpList();            

            jumpList.JumpItems.Add(tskCheckUpdate);
            jumpList.JumpItems.Add(tskSyncNow);
            jumpList.ShowFrequentCategory = false;
            jumpList.ShowRecentCategory = false;

            JumpList.SetJumpList(Application.Current, jumpList);

            jumpList.Apply();
        }
    }
}
