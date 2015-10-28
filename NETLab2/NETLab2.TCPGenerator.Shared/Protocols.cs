using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NETLab2.TCPGenerator.Shared
{
    abstract class ProtocolHeader
    {
        abstract public byte[] GetProtocolPacketBytes(
                byte[] payLoad
                );

        public byte[] BuildPacket(ArrayList headerList, byte[] payLoad)
        {
            ProtocolHeader protocolHeader;
            byte[] newPayload = null;

            for (int i = headerList.Count - 1; i >= 0; i--)
            {
                protocolHeader = (ProtocolHeader)headerList[i];

                newPayload = protocolHeader.GetProtocolPacketBytes(payLoad);

                payLoad = newPayload;
            }

            return payLoad;
        }

        static public ushort ComputeChecksum(byte[] payLoad)
        {
            uint xsum = 0;
            ushort shortval = 0,
                    hiword = 0,
                    loword = 0;

            for (int i = 0; i < payLoad.Length / 2; i++)
            {
                hiword = (ushort)(((ushort)payLoad[i * 2]) << 8);
                loword = (ushort)payLoad[(i * 2) + 1];

                shortval = (ushort)(hiword | loword);

                xsum = xsum + (uint)shortval;
            }
            if ((payLoad.Length % 2) != 0)
            {
                xsum += (uint)payLoad[payLoad.Length - 1];
            }

            xsum = ((xsum >> 16) + (xsum & 0xFFFF));
            xsum = (xsum + (xsum >> 16));
            shortval = (ushort)(~xsum);

            return shortval;
        }

        public class Ipv4Header : ProtocolHeader
        {
            private byte ipVersion;
            private byte ipLength;
            private byte ipTypeOfService;
            private ushort ipTotalLength;
            private ushort ipId;
            private ushort ipOffset;
            private byte ipTtl;
            private byte ipProtocol;
            private ushort ipChecksum;
            private IPAddress ipSourceAddress;
            private IPAddress ipDestinationAddress;

            static public int Ipv4HeaderLength = 20;

            public Ipv4Header() : base()
            {
                ipVersion = 4;
                ipLength = (byte)Ipv4HeaderLength;
                ipTypeOfService = 0;
                ipId = 0;
                ipOffset = 0;
                ipTtl = 1;
                ipProtocol = 0;
                ipChecksum = 0;
                ipSourceAddress = IPAddress.Any;
                ipDestinationAddress = IPAddress.Any;
            }

            public byte Version
            {
                get
                {
                    return ipVersion;
                }
                set
                {
                    ipVersion = value;
                }
            }

            public byte Length
            {
                get
                {
                    return (byte)(ipLength * 4);
                }
                set
                {
                    ipLength = (byte)(value / 4);
                }
            }

            public byte TypeOfService
            {
                get
                {
                    return ipTypeOfService;
                }
                set
                {
                    ipTypeOfService = value;
                }
            }

            public ushort TotalLength
            {
                get
                {
                    return (ushort)IPAddress.NetworkToHostOrder((short)ipTotalLength);
                }
                set
                {
                    ipTotalLength = (ushort)IPAddress.HostToNetworkOrder((short)value);
                }
            }

            public ushort Id
            {
                get
                {
                    return (ushort)IPAddress.NetworkToHostOrder((short)ipId);
                }
                set
                {
                    ipId = (ushort)IPAddress.HostToNetworkOrder((short)value);
                }
            }

            public ushort Offset
            {
                get
                {
                    return (ushort)IPAddress.NetworkToHostOrder((short)ipOffset);
                }
                set
                {
                    ipOffset = (ushort)IPAddress.HostToNetworkOrder((short)value);
                }
            }

            public byte Ttl
            {
                get
                {
                    return ipTtl;
                }
                set
                {
                    ipTtl = value;
                }
            }

            public byte Protocol
            {
                get
                {
                    return ipProtocol;
                }
                set
                {
                    ipProtocol = value;
                }
            }

            public ushort Checksum
            {
                get
                {
                    return (ushort)IPAddress.NetworkToHostOrder((short)ipChecksum);
                }
                set
                {
                    ipChecksum = (ushort)IPAddress.HostToNetworkOrder((short)value);
                }
            }

            public IPAddress SourceAddress
            {
                get
                {
                    return ipSourceAddress;
                }
                set
                {
                    ipSourceAddress = value;
                }
            }

            public IPAddress DestinationAddress
            {
                get
                {
                    return ipDestinationAddress;
                }
                set
                {
                    ipDestinationAddress = value;
                }
            }

            public override byte[] GetProtocolPacketBytes(byte[] payLoad)
            {
                byte[] ipv4Packet,
                        byteValue;
                int index = 0;

                ipv4Packet = new byte[Ipv4HeaderLength + payLoad.Length];

                ipv4Packet[index++] = (byte)((ipVersion << 4) | ipLength);
                ipv4Packet[index++] = ipTypeOfService;

                byteValue = BitConverter.GetBytes(ipTotalLength);
                Array.Copy(byteValue, 0, ipv4Packet, index, byteValue.Length);
                index += byteValue.Length;

                byteValue = BitConverter.GetBytes(ipId);
                Array.Copy(byteValue, 0, ipv4Packet, index, byteValue.Length);
                index += byteValue.Length;

                byteValue = BitConverter.GetBytes(ipOffset);
                Array.Copy(byteValue, 0, ipv4Packet, index, byteValue.Length);
                index += byteValue.Length;

                ipv4Packet[index++] = ipTtl;
                ipv4Packet[index++] = ipProtocol;
                ipv4Packet[index++] = 0;
                ipv4Packet[index++] = 0;

                byteValue = ipSourceAddress.GetAddressBytes();
                Array.Copy(byteValue, 0, ipv4Packet, index, byteValue.Length);
                index += byteValue.Length;

                byteValue = ipDestinationAddress.GetAddressBytes();
                Array.Copy(byteValue, 0, ipv4Packet, index, byteValue.Length);
                index += byteValue.Length;

                Array.Copy(payLoad, 0, ipv4Packet, index, payLoad.Length);
                index += payLoad.Length;

                Checksum = ComputeChecksum(ipv4Packet);

                byteValue = BitConverter.GetBytes(ipChecksum);
                Array.Copy(byteValue, 0, ipv4Packet, 10, byteValue.Length);

                return ipv4Packet;
            }
        }

        public enum TcpFlags
        {
            FIN = 0x01,
            SYN = 0x02,
            RST = 0x04,
            PSH = 0x08,
            ACK = 0x10,
            URG = 0x20
        }

        public class TcpHeader : ProtocolHeader
        {
            private ushort _src_port;
            private ushort _dst_port;
            private uint _seq_n;
            private uint _ack_n;
            private byte _offset;
            private byte _flags;
            private ushort _win;
            private ushort _crc;
            private ushort _urgent_pointer;

            public Ipv4Header ipv4PacketHeader;

            static public int TcpHeaderLength = 20;

            public TcpHeader() : base()
            {
                _src_port = 0;
                _dst_port = 0;
                _seq_n = 0;
                _ack_n = 0;
                _offset = 0;
                _flags = 0;
                _win = 0;
                _crc = 0;
                _urgent_pointer = 0;
                ipv4PacketHeader = null;
            }

            public TcpHeader(string src_port, string dest_port, bool? urg, bool? ack, bool? psh, bool? rst, bool? syn, bool? fin, string data) : base()
            {
                Random rand = new Random();
                try
                {
                    _src_port = Convert.ToUInt16(src_port);
                    _dst_port = Convert.ToUInt16(dest_port);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                if (urg == true)
                {
                    _flags |= (byte)TcpFlags.URG;
                    _urgent_pointer = (ushort)rand.Next();
                }
                else
                    _urgent_pointer = 0;
                if (ack == true)
                {
                    _flags |= (byte)TcpFlags.ACK;
                    _ack_n = (uint)rand.Next();
                }
                if (ack == false)
                    _ack_n = 1;
                if (psh == true)
                    _flags |= (byte)TcpFlags.PSH;
                if (rst == true)
                    _flags |= (byte)TcpFlags.RST;
                if (syn == true)
                    _flags |= (byte)TcpFlags.SYN;
                _seq_n = (uint)1000;
                if (fin == true)
                    _flags |= (byte)TcpFlags.FIN;
                _win = 256;
                _offset = (byte)(0xF0 & (5 << 4));
                _crc = 0;
                ipv4PacketHeader = null;
            }

            public ushort SrcPort
            {
                get
                {
                    return (ushort)IPAddress.NetworkToHostOrder((short)_src_port);
                }
                set
                {
                    _src_port = (ushort)IPAddress.HostToNetworkOrder((short)value);
                }
            }

            public ushort DstPort
            {
                get
                {
                    return (ushort)IPAddress.NetworkToHostOrder((short)_dst_port);
                }
                set
                {
                    _dst_port = (ushort)IPAddress.HostToNetworkOrder((short)value);
                }
            }

            public uint SeqN
            {
                get
                {
                    return (uint)IPAddress.NetworkToHostOrder((int)_seq_n);
                }
                set
                {
                    _seq_n = (uint)IPAddress.HostToNetworkOrder((int)value);
                }
            }

            public uint AckN
            {
                get
                {
                    return (uint)IPAddress.NetworkToHostOrder((int)_ack_n);
                }
                set
                {
                    _ack_n = (uint)IPAddress.HostToNetworkOrder((int)value);
                }
            }

            public byte Offset
            {
                get
                {
                    return _offset;
                }
                set
                {
                    _offset = value;
                }
            }

            public byte Flags
            {
                get
                {
                    return _flags;
                }
                set
                {
                    _flags = value;
                }
            }

            public ushort Win
            {
                get
                {
                    return (ushort)IPAddress.NetworkToHostOrder((short)_win);
                }
                set
                {
                    _win = (ushort)IPAddress.HostToNetworkOrder((short)value);
                }
            }

            public ushort Crc
            {
                get
                {
                    return (ushort)IPAddress.NetworkToHostOrder((short)_crc);
                }
                set
                {
                    _crc = (ushort)IPAddress.HostToNetworkOrder((short)value);
                }
            }

            public ushort UrgentPointer
            {
                get
                {
                    return (ushort)IPAddress.NetworkToHostOrder((short)_urgent_pointer);
                }
                set
                {
                    _urgent_pointer = (ushort)IPAddress.HostToNetworkOrder((short)value);
                }
            }

            public override byte[] GetProtocolPacketBytes(byte[] payLoad)
            {
                byte[] tcpPacket = new byte[TcpHeaderLength + payLoad.Length],
                    pseudoHeader = null,
                    byteValue = null;
                int offset = 0;

                byteValue = BitConverter.GetBytes(SrcPort);
                Array.Copy(byteValue, 0, tcpPacket, offset, byteValue.Length);
                offset += byteValue.Length;

                byteValue = BitConverter.GetBytes(DstPort);
                Array.Copy(byteValue, 0, tcpPacket, offset, byteValue.Length);
                offset += byteValue.Length;

                byteValue = BitConverter.GetBytes(SeqN);
                Array.Copy(byteValue, 0, tcpPacket, offset, byteValue.Length);
                offset += byteValue.Length;

                byteValue = BitConverter.GetBytes(AckN);
                Array.Copy(byteValue, 0, tcpPacket, offset, byteValue.Length);
                offset += byteValue.Length;

                tcpPacket[offset++] = _offset;
                tcpPacket[offset++] = _flags;

                byteValue = BitConverter.GetBytes(Win);
                Array.Copy(byteValue, 0, tcpPacket, offset, byteValue.Length);
                offset += byteValue.Length;

                tcpPacket[offset++] = 0;
                tcpPacket[offset++] = 0;

                byteValue = BitConverter.GetBytes(UrgentPointer);
                Array.Copy(byteValue, 0, tcpPacket, offset, byteValue.Length);
                offset += byteValue.Length;

                Array.Copy(payLoad, 0, tcpPacket, offset, payLoad.Length);

                if (ipv4PacketHeader != null)
                {
                    pseudoHeader = new byte[TcpHeaderLength + 12 + payLoad.Length];

                    offset = 0;

                    byteValue = ipv4PacketHeader.SourceAddress.GetAddressBytes();
                    Array.Copy(byteValue, 0, pseudoHeader, offset, byteValue.Length);
                    offset += byteValue.Length;

                    byteValue = ipv4PacketHeader.DestinationAddress.GetAddressBytes();
                    Array.Copy(byteValue, 0, pseudoHeader, offset, byteValue.Length);
                    offset += byteValue.Length;

                    pseudoHeader[offset++] = 0;
                    pseudoHeader[offset++] = ipv4PacketHeader.Protocol;

                    byteValue = BitConverter.GetBytes((ushort)(TcpHeaderLength + payLoad.Length));
                    Array.Copy(byteValue, 0, pseudoHeader, offset, byteValue.Length);
                    offset += byteValue.Length;

                    Array.Copy(tcpPacket, 0, pseudoHeader, offset, tcpPacket.Length);
                }

                if (pseudoHeader != null)
                {
                    Crc = ComputeChecksum(pseudoHeader);
                }

                byteValue = BitConverter.GetBytes(Crc);
                Array.Copy(byteValue, 0, tcpPacket, 16, byteValue.Length);

                return tcpPacket;
            }
        }
    }
}
