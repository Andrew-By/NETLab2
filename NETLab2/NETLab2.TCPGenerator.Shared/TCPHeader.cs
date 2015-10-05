using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
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
    class TCPHeader
    {
        /// <summary>
        /// Порт отправителя
        /// </summary>
        ushort _src_port { get; set; }

        /// <summary>
        /// Порт получателя 
        /// </summary>
        ushort _dst_port { get; set; }

        /// <summary>
        /// Номер очереди
        /// </summary>
        uint _seq_n { get; set; }

        /// <summary>
        /// Номер подтверждения
        /// </summary>
        uint _ack_n { get; set; }

        /// <summary>
        /// Смещение данных (4 бита) + Зарезервировано (4 бита)
        /// </summary>
        byte _offset { get; set; }

        /// <summary>
        /// Зарезервировано (2 бита) + Флаги (6 бит)
        /// </summary>
        byte _flags { get; set; }

        /// <summary>
        /// Размер окна
        /// </summary>
        ushort _win { get; set; }

        /// <summary>
        /// Контрольная сумма заголовка
        /// </summary>
        ushort _crc { get; set; }

        /// <summary>
        /// Дополнение до 20 байт
        /// </summary>
        ushort _padding { get; set; }

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
            _win = (ushort)Math.Pow(2, rand.Next(11));
            _offset = (byte)Marshal.SizeOf(new TCPHeader());
            rs_pseudo_crc(data, data.Length, _src_port, _dst_port, _offset + data.Length, 6);
            _padding = 0;
        }

        /// <summary>
        /// Класс-псевдоструктура заголовка пакета TCP протокола для вычисления контрольной суммы
        /// </summary>
        class PseudoHeader
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
    }
}
