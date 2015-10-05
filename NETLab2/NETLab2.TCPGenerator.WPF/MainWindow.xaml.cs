using NETLab2.TCPGenerator.Shared;
using System;
using System.Net;
using System.Net.Sockets;
using System.Windows;

namespace NETLab2.TCPGenerator.WPF
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TCPHeader _header;
        private Socket _socket;
        private EndPoint _endPoint;
        public MainWindow()
        {
            InitializeComponent();

            App.Current.Exit += Current_Exit;
            SenderAddressBox.Text = Dns.GetHostAddresses(Dns.GetHostName())[0].MapToIPv4().ToString();
        }

        private void Current_Exit(object sender, ExitEventArgs e)
        {
            if (_socket != null)
                _socket.Close();
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP);
                _endPoint = new IPEndPoint(IPAddress.Parse(ReceiverAddressBox.Text), int.Parse(ReceiverPortBox.Text));
                _header = new TCPHeader(SenderPortBox.Text, ReceiverPortBox.Text, AckOut.IsChecked,
                    PshOut.IsChecked, RstOut.IsChecked, SynOut.IsChecked, FinOut.IsChecked, Message.Text);
                SendButton.IsEnabled = true;
            }
            catch (Exception ex) {
                MessageBox.Show(ex.ToString(), "Некорректные аргументы", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
