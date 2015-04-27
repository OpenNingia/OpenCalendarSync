using OpenCalendarSync.App.Tray.Properties;
using OpenCalendarSync.Lib.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Logica di interazione per ColorFragment.xaml
    /// </summary>
    public partial class ColorFragment : UserControl, ISettingsProvider
    {
        private bool[] _colorChanged;

        public ColorFragment()
        {
            InitializeComponent();
            InternalInit();
        }

        private void InternalInit()
        {
            _colorChanged = new bool[2];

            TextColorComboBox.SelectedColorChanged += textColorComboBox_SelectedColorChanged;
            BackgroundColorComboBox.SelectedColorChanged += backgroundColorComboBox_SelectedColorChanged;
        }

        private void textColorComboBox_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color> e)
        {
            if (e.NewValue != e.OldValue)
            {
                _colorChanged[0] = true;
            }
        }

        private void backgroundColorComboBox_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color> e)
        {
            if (e.NewValue != e.OldValue)
            {
                _colorChanged[1] = true;
            }
        }

        /*private void textColorComboBox_Unloaded(object sender, RoutedEventArgs e)
        {
            ChangeCalendarColor(TextColorComboBox.SelectedColor, BackgroundColorComboBox.SelectedColor);
        }

        private void backgroundColorComboBox_Unloaded(object sender, RoutedEventArgs e)
        {
            ChangeCalendarColor(TextColorComboBox.SelectedColor, BackgroundColorComboBox.SelectedColor);
        }*/

        private async void ChangeCalendarColor(System.Windows.Media.Color fg, System.Windows.Media.Color bg)
        {
            if (_colorChanged[0] || _colorChanged[1])
            {
                _colorChanged[0] = false;
                _colorChanged[1] = false;
                var foregroundColor = "#" + fg.ToString().Substring(3); // remove alpha channel, we don't want that shit
                var backgroundColor = "#" + bg.ToString().Substring(3);
                if (!GoogleCalendarManager.Instance.LoggedIn)
                {
                    var login = await GoogleCalendarManager.Instance.Login(Settings.Default.ClientID, Settings.Default.ClientSecret);
                }
                var calendarId = await GoogleCalendarManager.Instance.Initialize(Settings.Default.CalendarID, Settings.Default.CalendarName);
                if (Settings.Default.CalendarID != null && Settings.Default.CalendarID != calendarId)
                {
                    Settings.Default.CalendarID = calendarId;
                }
                if (GoogleCalendarManager.Instance.LoggedIn)
                {
                    await GoogleCalendarManager.Instance.SetCalendarColor(foregroundColor.ToLower(), backgroundColor.ToLower());
                }

                NotificationManager.AppendNotifyMessage("Colore Calendario", "Il colore del tuo calendario su google e' stato cambiato correttamente");
            }
        }

        public void Apply()
        {
            ChangeCalendarColor(TextColorComboBox.SelectedColor, BackgroundColorComboBox.SelectedColor);
        }
    }
}
