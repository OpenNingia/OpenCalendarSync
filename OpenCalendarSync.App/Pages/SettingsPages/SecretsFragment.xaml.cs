using OpenCalendarSync.App.Tray.Properties;
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
    /// Logica di interazione per SecretsFragment.xaml
    /// </summary>
    public partial class SecretsFragment : UserControl, ISettingsProvider
    {
        private string _clientId;
        private string _clientSecret;

        public SecretsFragment()
        {
            InitializeComponent();
            InternalInit();
        }

        private void InternalInit()
        {
            _clientId = Settings.Default.ClientID;
            _clientSecret = Settings.Default.ClientSecret;
            if (string.IsNullOrEmpty(_clientId))
                _clientId = GoogleToken.ClientId;
            if (string.IsNullOrEmpty(_clientSecret))
                _clientSecret = GoogleToken.ClientSecret;

            ClientIdPwdBox.Password = _clientId;
            ClientSecretPwdBox.Password = _clientSecret;
        }

        public void Apply()
        {
            
        }
    }
}
