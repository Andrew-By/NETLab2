using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using System.Text;
using System.Windows.Threading;

namespace NETLab2.Sniffer.Shared.ViewModels
{
    public class Sniffer : INotifyPropertyChanged
    {

        private SnifferSocket _socket;

        private string _currentInterface;
        public string CurrentInterface
        {
            get { return _currentInterface; }
            set
            {
                if (value != _currentInterface)
                {
                    _currentInterface = value;
                    NotifyPropertyChanged("CurrentInterface");
                }
            }
        }

        private ObservableCollection<String> _interfaces = new ObservableCollection<string>();
        public ObservableCollection<String> Interfaces
        {
            get { return _interfaces; }
        }

        private ObservableCollection<Packet> _packets = new ObservableCollection<Packet>();
        public ObservableCollection<Packet> Packets
        {
            get { return _packets; }
        }

        public Sniffer()
        {
            string strIP = null;
            IPHostEntry HostEntry = Dns.GetHostEntry((Dns.GetHostName()));
            if (HostEntry.AddressList.Length > 0)
            {
                foreach (IPAddress ip in HostEntry.AddressList)
                {
                    strIP = ip.ToString();
                    _interfaces.Add(strIP);
                }
            }


        }

        public void Start()
        {
            _socket = new SnifferSocket(new IPEndPoint(IPAddress.Parse(_currentInterface), 0));
            _socket.OnPackageReceived += _socket_OnPackageReceived;
        }

        private void _socket_OnPackageReceived(object sender, Models.IPHeader e)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                Packets.Add(new Packet(e.Identification, DateTime.Now.ToLongTimeString(), e.SourceAddress.ToString(), e.DestinationAddress.ToString(), e.ProtocolType.ToString()));
            }));
        }

        public void Stop()
        {
            if (_socket != null)
            {
                _socket.Close();
                _socket.OnPackageReceived -= _socket_OnPackageReceived;
            }
        }

        #region INotify
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this,
                    new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
