﻿using NETLab2.Sniffer.Shared.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Windows.Data;
using System.Windows.Threading;
using CsvHelper;

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

        private bool _filterIsOn;
        private Protocol _filter;

        private ObservableCollection<IPHeader> _packets = new ObservableCollection<IPHeader>();
        public ObservableCollection<IPHeader> Packets
        {
            get { return _packets; }
        }

        private CollectionViewSource _packetsView = new CollectionViewSource();
        public CollectionViewSource PacketsView
        {
            get { return _packetsView; }
        }

        private Dictionary<string, string> _currentPacket;
        public Dictionary<string, string> CurrentPacket
        {
            get { return _currentPacket; }
            set
            {
                if (value != _currentPacket)
                {
                    _currentPacket = value;
                    NotifyPropertyChanged("CurrentPacket");
                }

            }
        }

        public Sniffer()
        {
            string strIP = null;
            PacketsView.Source = Packets;
            PacketsView.Filter += PacketsView_Filter;
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

        private void _socket_OnPackageReceived(object sender, IPHeader e)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                //Packets.Add(new Packet(e.Identification, DateTime.Now.ToLongTimeString(), e.SourceAddress.ToString(), e.DestinationAddress.ToString(), e.ProtocolType.ToString()));
                Packets.Add(e);
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

        public void FilterChanged(int id)
        {
            if (id > 0)
            {
                _filter = (Protocol)id;
                _filterIsOn = true;
            }
            else
                _filterIsOn = false;
            NotifyPropertyChanged("PacketsView");
        }

        private void PacketsView_Filter(object sender, FilterEventArgs e)
        {
            if (_filterIsOn)
                e.Accepted = ((IPHeader)e.Item).ProtocolType == _filter;
            else
                e.Accepted = true;
        }

        public void SelectPacket(int id)
        {
            CurrentPacket = new Dictionary<string, string>();
            PropertyInfo[] properties = null;
            object header = null;
            switch (Packets[id].ProtocolType)
            {
                case Protocol.TCP:
                    TCPHeader tcpHeader = new TCPHeader(Packets[id].Data, Packets[id].MessageLength);
                    properties = tcpHeader.GetType().GetProperties();
                    header = tcpHeader;
                    break;
                case Protocol.UDP:
                    UDPHeader udpHeader = new UDPHeader(Packets[id].Data, Packets[id].MessageLength);
                    properties = udpHeader.GetType().GetProperties();
                    header = udpHeader;
                    break;
                case Protocol.ICMP:
                    ICMPHeader icmpHeader = new ICMPHeader(Packets[id].Data, Packets[id].MessageLength);
                    properties = icmpHeader.GetType().GetProperties();
                    header = icmpHeader;
                    break;
            }

            if (properties != null)
            {
                foreach (PropertyInfo pi in properties)
                    CurrentPacket.Add(pi.Name, pi.GetValue(header).ToString());
            }
        }

        public void Clear()
        {
            Packets.Clear();
        }

        public void Export(string fileName)
        {
            StreamWriter text = new StreamWriter(fileName, false, Encoding.GetEncoding("Windows-1251"));
            var csv = new CsvWriter(text);
            csv.Configuration.Delimiter = ";";
            csv.WriteRecords(Packets);
            text.Close();
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
