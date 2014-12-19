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
    /// Logica di interazione per OptionFragment.xaml
    /// </summary>
    public partial class OptionFragment : UserControl, ISettingsProvider
    {
        public OptionFragment()
        {
            InitializeComponent();
            InternalInit();
        }

        private void InternalInit()
        {

        }

        private void slRefreshTmo_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0) { SlRefreshTmo.Value += SlRefreshTmo.SmallChange; }
            else { SlRefreshTmo.Value -= SlRefreshTmo.SmallChange; }
        }


        private void calnameTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            //todo: add summary customization here
        }


        public void Apply()
        {
            
        }
    }
}
