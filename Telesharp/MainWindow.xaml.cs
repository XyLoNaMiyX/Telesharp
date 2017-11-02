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
using TLSharp.Core;
using TLSharp.Core.MTProto;

namespace Telesharp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {   
        public MainWindow()
        {
            InitializeComponent();
            Loaded += loaded;
        }

        async void loaded(object sender, RoutedEventArgs e)
        {
            if (!await Telegram.Connect())
            {
                Application.Current.Shutdown();
                return;
            }
            if (Telegram.IsUserAuthorized())
            {
                foreach (var cardInfo in await Telegram.GetDialogs(50))
                    dialogsPanel.Children.Add(new DialogCard(cardInfo));
            }
        }
        
    }
}
