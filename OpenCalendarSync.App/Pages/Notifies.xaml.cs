using FirstFloor.ModernUI.Windows.Controls;
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

namespace OpenCalendarSync.App.Tray.Pages
{
    /// <summary>
    /// Logica di interazione per Notifies.xaml
    /// </summary>
    public partial class Notifies : IMessageAppender
    {
        NavigationService nav;

        public Notifies()
            : base()
        {
            InitializeComponent();

            NotificationManager.RegisterAppender(this.GetType(), this);
        }

        public void AppendMessage(string title, string message)
        {            
            var tr = new Run(title);
            tr.FontWeight = FontWeights.Bold;

            var mr = new Run(message);

            var pt = new Paragraph(tr);
            var pm = new Paragraph(mr);

            docNotifies.Blocks.AddRange(new object[] { pt, pm });
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

        }

    }
}
