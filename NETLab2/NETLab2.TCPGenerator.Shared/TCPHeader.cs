using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;

namespace NETLab2.TCPGenerator.Shared
{
    /// <summary>
    /// Флаги, используемые в пакетах протокола TCP
    /// </summary>
    public enum TcpFlags
    {
        FIN = 0x01,
        SYN = 0x02,
        RST = 0x04,
        PSH = 0x08,
        ACK = 0x16
    }

    /// <summary>
    /// Класс-заголовок пакета TCP протокола
    /// </summary>
    [Serializable()]
    class TCPHeader
    {
        /// <summary>
        /// Порт отправителя
        /// </summary>
        ushort _src_port;
        public ushort SrcPort
        {
            set { _src_port = value; }
        }

        /// <summary>
        /// Порт получателя 
        /// </summary>
        ushort _dst_port;
        public ushort DstPort
        {
            set { _dst_port = value; }
        }

        /// <summary>
        /// Номер очереди
        /// </summary>
        uint _seq_n;
        public uint SeqN
        {
            set { _seq_n = value; }
            get { return _seq_n; }
        }

        /// <summary>
        /// Номер подтверждения
        /// </summary>
        uint _ack_n;
        public uint AckN
        {
            set { _ack_n = value; }
            get { return _ack_n; }
        }

        /// <summary>
        /// Смещение данных (4 бита) + Зарезервировано (4 бита)
        /// </summary>
        byte _offset;
        public byte Offset
        {
            set { _offset = value; }
        }

        /// <summary>
        /// Зарезервировано (2 бита) + Флаги (6 бит)
        /// </summary>
        byte _flags;
        public byte Flags
        {
            set { _flags = value; }
            get { return _flags; }
        }

        /// <summary>
        /// Размер окна
        /// </summary>
        ushort _win;
        public ushort Win
        {
            set { _win = value; }
            get { return _win; }
        }

        /// <summary>
        /// Контрольная сумма заголовка
        /// </summary>
        ushort _crc;
        public ushort Crc
        {
            set { _crc = value; }
            get { return _crc; }
        }

        /// <summary>
        /// Дополнение до 20 байт
        /// </summary>
        ushort _padding;
        public ushort Padding
        {
            set { _padding = value; }
        }

        /// <summary>
        /// Пустой коструктор класса TCPHeader
        /// </summary>
        public TCPHeader() { }

        /// <summary>
        /// Конструктор класса TCPHeader
        /// </summary>
        /// <param name="src_port"></param>
        /// <param name="dest_port"></param>
        /// <param name="ack"></param>
        /// <param name="psh"></param>
        /// <param name="rst"></param>
        /// <param name="syn"></param>
        /// <param name="fin"></param>
        /// <param name="data"></param>
        public TCPHeader(string src_port, string dest_port, bool? ack, bool? psh, bool? rst, bool? syn, bool? fin, string data)
        {
            Random rand = new Random();
            try
            {
                _src_port = ushort.Parse(src_port);
                _dst_port = ushort.Parse(dest_port);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            if (ack == true)
            {
                _flags |= (byte)TcpFlags.ACK;
                _ack_n = (uint)rand.Next();
            }
            if (psh == true)
                _flags |= (byte)TcpFlags.PSH;
            if (rst == true)
                _flags |= (byte)TcpFlags.RST;
            if (syn == true)
                _flags |= (byte)TcpFlags.SYN;
            if (syn != null)
                _seq_n = (uint)rand.Next();
            if (fin == true)
                _flags |= (byte)TcpFlags.FIN;
            _win = (ushort)Math.Pow(2, 5 + rand.Next(6));
            _offset = (byte)(0xF0 & ((Marshal.SizeOf(new PseudoHeader()) / 4) << 4));
            rs_pseudo_crc(data, data.Length, _src_port, _dst_port, _offset + data.Length, 6);
            _padding = 0;
        }

        /// <summary>
        /// Класс-псевдоструктура заголовка пакета TCP протокола для вычисления контрольной суммы
        /// </summary>
        struct PseudoHeader
        {
            /// <summary>
            /// Адрес отправителя
            /// </summary>
            uint _src_addr { get; set; }

            /// <summary>
            /// Адрес получателя
            /// </summary>
            uint _dst_addr { get; set; }

            /// <summary>
            /// Начальная установка
            /// </summary>
            byte _zero { get; set; }

            /// <summary>
            /// Протокол
            /// </summary>
            byte _proto { get; set; }

            /// <summary>
            /// Длина заголовка
            /// </summary>
            ushort _length { get; set; }

            public PseudoHeader(uint src_addr,
                uint dst_addr,
                int packet_length,
                byte proto)
            {
                _src_addr = src_addr;
                _dst_addr = dst_addr;
                _zero = 0;
                _proto = proto;
                _length = (ushort)IPAddress.HostToNetworkOrder(packet_length);
            }
        }

        ushort rs_crc(List<byte> buffer, int length)
        {
            ulong crc = 0;
            int i = 0;
            var vals = Array.ConvertAll(buffer.ToArray(), b => (ushort)b);
            while (length > 1)
            {
                crc += vals[i++];
                length -= sizeof(ushort);
            }
            if (length > 0)
                crc += buffer[buffer.Count - 1];
            crc = (crc >> 16) + (crc & 0xffff);
            crc += (crc >> 16);
            return (ushort)(~crc);
        }

        void rs_pseudo_crc(string data,
            int data_length,
            uint src_addr,
            uint dst_addr,
            int packet_length,
            byte proto)
        {
            List<byte> buffer;
            int full_length;
            int header_length;
            PseudoHeader ph = new PseudoHeader(src_addr, dst_addr, packet_length, proto);
            _crc = 0;
            header_length = Marshal.SizeOf(ph);
            full_length = header_length + data_length;
            buffer = new List<byte>(Utils.SerializeMessage(ph));
            buffer.InsertRange(header_length, Encoding.Unicode.GetBytes(data));
            _crc = rs_crc(buffer, full_length);
        }

        public void Send(Socket socket, string data, int data_length, string receiver_ip, string dst_port_raw)
        {
            List<byte> buffer = new List<byte>(Utils.ObjectToByteArray(this));
            foreach (var ch in data)
                buffer.Add((byte)ch);
            try
            {
                EndPoint remote = (EndPoint)(new IPEndPoint(IPAddress.Parse(receiver_ip), int.Parse(dst_port_raw)));
                socket.SendTo(buffer.ToArray(), remote);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info"></param>
        /// <param name="ctxt"></param>
        public TCPHeader(SerializationInfo info, StreamingContext ctxt)
        {
            //Get the values from info and assign them to the appropriate properties
            _src_port = (ushort)info.GetValue("SrcPort", typeof(ushort));
            _dst_port = (ushort)info.GetValue("DstPort", typeof(ushort));
            _seq_n = (uint)info.GetValue("SeqN", typeof(uint));
            _ack_n = (uint)info.GetValue("AckN", typeof(uint));
            _offset = (byte)info.GetValue("Offset", typeof(byte));
            _flags = (byte)info.GetValue("Flags", typeof(byte));
            _win = (ushort)info.GetValue("Win", typeof(ushort));
            _crc = (ushort)info.GetValue("Crc", typeof(ushort));
            _padding = (ushort)info.GetValue("Padding", typeof(ushort));
        }

        /// <summary>
        /// Serialization function
        /// </summary>
        /// <param name="info"></param>
        /// <param name="ctxt"></param>
        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("SrcPort", _src_port);
            info.AddValue("DstPort", _dst_port);
            info.AddValue("SeqN", _seq_n);
            info.AddValue("AckN", _ack_n);
            info.AddValue("Offset", _offset);
            info.AddValue("Flags", _flags);
            info.AddValue("Win", _win);
            info.AddValue("Crc", _crc);
            info.AddValue("Padding", _padding);
        }
    }
}
