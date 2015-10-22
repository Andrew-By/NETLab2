using NETLab2.TCPGenerator.Shared;
using System;
using System.Collections;
using System.Linq;
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
        Socket rawSocket = null;
        public MainWindow()
        {
            InitializeComponent();

            App.Current.Exit += Current_Exit;
            SenderAddressBox.Text = Dns.GetHostAddresses(Dns.GetHostName()).FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork).ToString();
        }

        private void Current_Exit(object sender, ExitEventArgs e)
        {
            if (rawSocket != null)
                rawSocket.Close();
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            byte[] builtPacket, payLoad;
            ProtocolHeader.TcpHeader tcpPacket;
            ArrayList headerList = new ArrayList();
            try
            {
                tcpPacket = new ProtocolHeader.TcpHeader(SenderPortBox.Text, ReceiverPortBox.Text, UrgOut.IsChecked, 
                    AckOut.IsChecked, PshOut.IsChecked, RstOut.IsChecked, SynOut.IsChecked, FinOut.IsChecked, Message.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Некорректные аргументы", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            SendButton.IsEnabled = true;
            SeqNOut.Text = tcpPacket.SeqN.ToString();
            AckNOut.Text = tcpPacket.AckN.ToString();
            WindowOut.Text = tcpPacket.Win.ToString();
            Console.WriteLine("Создан Tcp пакет:\n" + tcpPacket.ToString() + "\n");

            payLoad = new byte[Message.Text.Length * sizeof(char)];
            Buffer.BlockCopy(Message.Text.ToCharArray(), 0, payLoad, 0, payLoad.Length);

            ProtocolHeader.Ipv4Header ipv4Packet = new ProtocolHeader.Ipv4Header();
            try
            {
                Console.WriteLine("Построение заголовка пакета IPv4...");
                ipv4Packet.Version = 4;
                ipv4Packet.Protocol = (byte)ProtocolType.Tcp;
                ipv4Packet.Ttl = 128;
                ipv4Packet.Offset = 0;
                ipv4Packet.Id = (ushort)new Random().Next(1, 1000);
                ipv4Packet.Length = (byte)ProtocolHeader.Ipv4Header.Ipv4HeaderLength;
                ipv4Packet.TotalLength = (ushort)Convert.ToUInt16(ProtocolHeader.Ipv4Header.Ipv4HeaderLength + ProtocolHeader.Ipv4Header.Ipv4HeaderLength + Message.Text.Length);
                ipv4Packet.SourceAddress = IPAddress.Parse(SenderAddressBox.Text);
                ipv4Packet.DestinationAddress = IPAddress.Parse(ReceiverAddressBox.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Некорректные аргументы", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            Console.WriteLine("Подсчет контрольной суммы на основе псевдозаголовка IPv4...");
            tcpPacket.ipv4PacketHeader = ipv4Packet;

            Console.WriteLine("Сборка пакета...");
            headerList.Add(ipv4Packet);
            headerList.Add(tcpPacket);
            builtPacket = tcpPacket.BuildPacket(headerList, payLoad);
            CrcOut.Text = tcpPacket.Crc.ToString();

            rawSocket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Raw);
            Console.WriteLine((EndPoint)new IPEndPoint(IPAddress.Parse(ReceiverAddressBox.Text), UInt16.Parse(ReceiverPortBox.Text)));
            rawSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, 1);
            try
            {
                Console.WriteLine("Отправка пакета...");
                int rc = rawSocket.SendTo(builtPacket, (EndPoint)new IPEndPoint(IPAddress.Parse(ReceiverAddressBox.Text), UInt16.Parse(ReceiverPortBox.Text)));
                Console.WriteLine("Послано {0} байтов по адресу {1}", rc, ReceiverAddressBox.Text);
            }
            catch (SocketException err)
            {
                Console.WriteLine("Возникла ошибка сокета: {0}", err.Message);
            }
            finally
            {
                Console.WriteLine("Закрытие сокета...");
                rawSocket.Close();
            }
        }
    }
}
