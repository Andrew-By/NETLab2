using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace NETLab2.Sniffer.Shared.ViewModels
{
    public class Packet
    {
        public Packet(string id, string dateTime, string sourceIP, string destinationIP, string protocol)
        {
            Id = id;
            DateTime = dateTime;
            SourceIP = sourceIP;
            DestinationIP = destinationIP;
            Protocol = protocol;
        }

        public string Id { get; set; }

        public string DateTime { get; set; }
        public string SourceIP { get; set; }
        public string DestinationIP { get; set; }
        public string Protocol { get; set; }

        public override string ToString()
        {
            return Protocol;
        }
    }
}
