using NETLab2.Sniffer.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

namespace NETLab2.Sniffer.WPF
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SnifferSocket socket;
        public MainWindow()
        {
            InitializeComponent();

            string strIP = null;
            IPHostEntry HostEntry = Dns.GetHostEntry((Dns.GetHostName()));
            if (HostEntry.AddressList.Length > 0)
            {
                foreach (IPAddress ip in HostEntry.AddressList)
                {
                    strIP = ip.ToString();
                    AdapterComboBox.Items.Add(strIP);
                }
            }
        }

        private void Current_Exit(object sender, ExitEventArgs e)
        {
            if (socket != null)
                socket.Close();
        }

        private void StartStopButton_Click(object sender, RoutedEventArgs e)
        {
            socket = new SnifferSocket(new IPEndPoint(IPAddress.Parse(AdapterComboBox.SelectedValue.ToString()), 0));
            App.Current.Exit += Current_Exit;
        }
    }
}
