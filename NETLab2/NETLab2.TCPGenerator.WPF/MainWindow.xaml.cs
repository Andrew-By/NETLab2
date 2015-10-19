﻿using NETLab2.TCPGenerator.Shared;
using System;
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
        private TCPHeader _header;
        private IPHeader _ip_header;
        private Socket _socket;
        public MainWindow()
        {
            InitializeComponent();

            App.Current.Exit += Current_Exit;
            SenderAddressBox.Text = Dns.GetHostAddresses(Dns.GetHostName()).FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork).ToString();
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
                _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, false);
                _header = new TCPHeader(SenderPortBox.Text, ReceiverPortBox.Text, UrgOut.IsChecked, AckOut.IsChecked,
                    PshOut.IsChecked, RstOut.IsChecked, SynOut.IsChecked, FinOut.IsChecked, Message.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Некорректные аргументы", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            SendButton.IsEnabled = true;
            SeqNOut.Text = _header.SeqN.ToString();
            AckNOut.Text = _header.AckN.ToString();
            WindowOut.Text = _header.Win.ToString();
            CrcOut.Text = _header.Crc.ToString();
            Console.WriteLine("Создан Tcp пакет:\n" + _header.ToString() + "\n");
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _ip_header = new IPHeader(_socket, _header.SerializeTcpPacket(Message.Text), SenderAddressBox.Text, ReceiverAddressBox.Text, ReceiverPortBox.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Некорректные аргументы", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
