using System;
using System.Collections.Generic;
using System.Net;
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
        ushort _src_port;

        /// <summary>
        /// Порт получателя 
        /// </summary>
        ushort _dst_port;

        /// <summary>
        /// Номер очереди
        /// </summary>
        uint _seq_n;

        /// <summary>
        /// Номер подтверждения
        /// </summary>
        uint _ack_n;

        /// <summary>
        /// Смещение данных (4 бита) + Зарезервировано (4 бита)
        /// </summary>
        byte offset;

        /// <summary>
        /// Зарезервировано (2 бита) + Флаги (6 бит)
        /// </summary>
        byte _flags;

        /// <summary>
        /// Размер окна
        /// </summary>
        ushort _win;

        /// <summary>
        /// Контрольная сумма заголовка
        /// </summary>
        ushort _crc;

        /// <summary>
        /// Дополнение до 20 байт
        /// </summary>
        ushort padding;

        /// <summary>
        /// Класс-псевдоструктура заголовка пакета TCP протокола для вычисления контрольной суммы
        /// </summary>
        class PseudoHeader
        {
            /// <summary>
            /// Адрес отправителя
            /// </summary>
            uint _src_addr;

            /// <summary>
            /// Адрес получателя
            /// </summary>
            uint _dst_addr;

            /// <summary>
            /// Начальная установка
            /// </summary>
            byte _zero;

            /// <summary>
            /// Протокол
            /// </summary>
            byte _proto;

            /// <summary>
            /// Длина заголовка
            /// </summary>
            ushort _length;

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

        ushort rs_crc(ushort* buffer, int length)
        {
            ulong crc = 0;
            // Вычисление CRC 
            while (length > 1)
            {
                crc += *buffer++;
                length -= sizeof(ushort);
            }
            if (length) crc += *(byte*)buffer;
            // Закончить вычисления 
            crc = (crc >> 16) + (crc & 0xffff);
            crc += (crc >> 16);
            // Возвращаем инвертированное значение 
            return (ushort)(~crc);
        }

        void rs_pseudo_crc(string data,
            int data_length,
            uint src_addr,
            uint dst_addr,
            int packet_length,
            byte proto)
        {
            char[] buffer;
            uint full_length;
            int header_length;
            PseudoHeader ph = new PseudoHeader(src_addr, dst_addr, packet_length, proto);
            _crc = 0;
            header_length = System.Runtime.InteropServices.Marshal.SizeOf(ph);
            full_length = (uint)(header_length + data_length);
            buffer = new char[full_length];

            // Генерация псевдозаголовка 
            memcpy(buffer, &ph, header_length);
            memcpy(buffer + header_length, data, data_length);

            // Вычисление CRC. 
            _crc = rs_crc((unsigned short *) buffer, full_length);
        }
    }
}
