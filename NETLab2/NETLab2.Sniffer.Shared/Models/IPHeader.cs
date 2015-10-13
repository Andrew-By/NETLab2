using System;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace NETLab2.Sniffer.Shared.Models
{
    public class IPHeader
    {
        //Поля заголовков IP пакета
        private byte byVersionAndHeaderLength;   //Восемь битов для версии и длины заголовка
        private byte byDifferentiatedServices;    //Восемь битов для дифференциированых сервисов
        private ushort usTotalLength;              //Шестнадцать битов для общей длины датаграммы (заголовок + сообщение)
        private ushort usIdentification;           //Шестнадцать битов для идентификации
        private ushort usFlagsAndOffset;           //Восемь битов для флагов и смещения фрагментации
        private byte byTTL;                      //Восемь битов TTL
        private byte byProtocol;                 //Восемь битов определяющих протокол
        private short sChecksum;                  //Шестнадцать битов контрольной суммы
                                                  //(может иметь отрицательные значения)
        private uint uiSourceIPAddress;          //Тридцать два бита IP адреса источника
        private uint uiDestinationIPAddress;     //Тридцать два бита IP адреса назначения
                                                 //Конец IP заголовка

        private byte byHeaderLength;             //Длина заголовка
        private byte[] byIPData = new byte[4096];  //Данные


        public IPHeader(byte[] byBuffer, int nReceived)
        {

            try
            {
                MemoryStream memoryStream = new MemoryStream(byBuffer, 0, nReceived);
                BinaryReader binaryReader = new BinaryReader(memoryStream);

                byVersionAndHeaderLength = binaryReader.ReadByte();
                byDifferentiatedServices = binaryReader.ReadByte();
                usTotalLength = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
                usIdentification = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
                usFlagsAndOffset = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
                byTTL = binaryReader.ReadByte();
                byProtocol = binaryReader.ReadByte();
                sChecksum = IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
                uiSourceIPAddress = (uint)(binaryReader.ReadInt32());
                uiDestinationIPAddress = (uint)(binaryReader.ReadInt32());

                byHeaderLength = byVersionAndHeaderLength;
                byHeaderLength <<= 4;
                byHeaderLength >>= 4;
                byHeaderLength *= 4;

                Array.Copy(byBuffer, byHeaderLength, byIPData, 0, usTotalLength - byHeaderLength);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public string Version
        {
            get
            {
                if ((byVersionAndHeaderLength >> 4) == 4)
                    return "IPv4";
                else if ((byVersionAndHeaderLength >> 4) == 6)
                    return "IPv6";
                else
                    return "Неизвестно";
            }
        }

        public string HeaderLength
        {
            get { return byHeaderLength.ToString(); }
        }

        public ushort MessageLength
        {
            get { return (ushort)(usTotalLength - byHeaderLength); }
        }

        public string DifferentiatedServices
        {
            get { return string.Format("0x{0:x2} ({1})", byDifferentiatedServices, byDifferentiatedServices); }
        }

        public string Flags
        {
            get
            {
                int nFlags = usFlagsAndOffset >> 13;
                if (nFlags == 2)
                    return "Не фрагметировано";
                else if (nFlags == 1)
                    return "Ожидаются фрагменты";
                else
                    return nFlags.ToString();
            }
        }

        public string FragmentationOffset
        {
            get
            {
                int nOffset = usFlagsAndOffset << 3;
                nOffset >>= 3;

                return nOffset.ToString();
            }
        }

        public string TTL
        {
            get { return byTTL.ToString(); }
        }

        public Protocol ProtocolType
        {
            get
            {
                switch (byProtocol)
                {
                    case 1:
                        return Protocol.ICMP;
                    case 6:
                        return Protocol.TCP;
                    case 17:
                        return Protocol.UDP;
                    default:
                        return Protocol.Unknown;
                }
            }
        }

        public string Checksum
        {
            get { return string.Format("0x{0:x2}", sChecksum); }
        }

        public IPAddress SourceAddress
        {
            get { return new IPAddress(uiSourceIPAddress); }
        }

        public IPAddress DestinationAddress
        {
            get { return new IPAddress(uiDestinationIPAddress); }
        }

        public string TotalLength
        {
            get { return usTotalLength.ToString(); }
        }

        public string Identification
        {
            get { return usIdentification.ToString(); }
        }

        public byte[] Data
        {
            get { return byIPData; }
        }
    }
}
