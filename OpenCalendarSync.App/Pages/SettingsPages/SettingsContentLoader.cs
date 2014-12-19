using FirstFloor.ModernUI.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace OpenCalendarSync.App.Tray.Pages.SettingsPages
{
    public class SettingsContentLoader : DefaultContentLoader
    {
        private static Dictionary<Uri, ISettingsProvider> SettingsFragments;

        public SettingsContentLoader()
        {
/*
 * 
                <mui:Link DisplayName="Opzioni" Source="Pages\SettingsPages\OptionFragment.xaml"/>
                <mui:Link DisplayName="Colori" Source="Pages\SettingsPages\ColorFragment.xaml"/>
                <mui:Link DisplayName="Segreti" Source="Pages\SettingsPages\SecretsFragment.xaml"/>
                <mui:Link DisplayName="Altro" Source="Pages\SettingsPages\MiscFragment.xaml"/>
 
 */

            SettingsFragments = new Dictionary<Uri, ISettingsProvider>();
            SettingsFragments.Add(new Uri(@"Pages\SettingsPages\OptionFragment.xaml", UriKind.Relative), new OptionFragment());
            SettingsFragments.Add(new Uri(@"Pages\SettingsPages\ColorFragment.xaml", UriKind.Relative), new ColorFragment());
            SettingsFragments.Add(new Uri(@"Pages\SettingsPages\SecretsFragment.xaml", UriKind.Relative), new SecretsFragment());
            SettingsFragments.Add(new Uri(@"Pages\SettingsPages\MiscFragment.xaml", UriKind.Relative), new MiscFragment());
        }

        protected override object LoadContent(Uri uri)
        {
            if (SettingsFragments.ContainsKey(uri))
                return SettingsFragments[uri];
            return null;
        }

        public static void DoApply()
        {
            foreach (var sp in SettingsFragments.Values)
                sp.Apply();
        }
    }
}
