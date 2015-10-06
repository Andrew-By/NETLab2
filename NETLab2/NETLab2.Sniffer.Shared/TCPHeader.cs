using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;

namespace NETLab2.Sniffer.Shared
{
    public class TCPHeader
    {
        //Поля заголовков TCP пакета
        private ushort usSourcePort;              //Шестнадцать битов порта источника
        private ushort usDestinationPort;         //Шестнадцать битов порта назначения
        private uint uiSequenceNumber = 555;          //Тридцать два бита номера последовательности
        private uint uiAcknowledgementNumber = 555;   //Тридцать два бита подтверждения
        private ushort usDataOffsetAndFlags = 555;      //Шестнадцать битов флагов и смещения
        private ushort usWindow = 555;                  //Шестнадцать битов на размер окна
        private short sChecksum = 555;                 //Шестнадцать битов для контрольной суммы
                                                       //(может иметь отрицательное значение)
        private ushort usUrgentPointer;           //Шестнадцать битов для указателя срочности
        //Конец полей заголовков TCP пакета

        private byte byHeaderLength;            //Длиина заголовка
        private ushort usMessageLength;           //Длина передаваемых данных
        private byte[] byTCPData = new byte[4096];//Данные TCP пакета

        public TCPHeader(byte[] byBuffer, int nReceived)
        {
            try
            {
                MemoryStream memoryStream = new MemoryStream(byBuffer, 0, nReceived);
                BinaryReader binaryReader = new BinaryReader(memoryStream);

                usSourcePort = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
                usDestinationPort = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
                uiSequenceNumber = (uint)IPAddress.NetworkToHostOrder(binaryReader.ReadInt32());
                uiAcknowledgementNumber = (uint)IPAddress.NetworkToHostOrder(binaryReader.ReadInt32());
                usDataOffsetAndFlags = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
                usWindow = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
                sChecksum = (short)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
                usUrgentPointer = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

                byHeaderLength = (byte)(usDataOffsetAndFlags >> 12);
                byHeaderLength *= 4;

                usMessageLength = (ushort)(nReceived - byHeaderLength);

                Array.Copy(byBuffer, byHeaderLength, byTCPData, 0, nReceived - byHeaderLength);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public string SourcePort
        {
            get { return usSourcePort.ToString(); }
        }

        public string DestinationPort
        {
            get { return usDestinationPort.ToString(); }
        }

        public string SequenceNumber
        {
            get { return uiSequenceNumber.ToString(); }
        }

        public string AcknowledgementNumber
        {
            get
            {
                //Проверить флаг ACK
                if ((usDataOffsetAndFlags & 0x10) != 0)
                {
                    return uiAcknowledgementNumber.ToString();
                }
                else
                    return "";
            }
        }

        public string HeaderLength
        {
            get { return byHeaderLength.ToString(); }
        }

        public string WindowSize
        {
            get { return usWindow.ToString(); }
        }

        public string UrgentPointer
        {
            get
            {
                //Проверить флаг URG
                if ((usDataOffsetAndFlags & 0x20) != 0)
                {
                    return usUrgentPointer.ToString();
                }
                else
                    return "";
            }
        }

        public string Flags
        {
            get
            {
                int nFlags = usDataOffsetAndFlags & 0x3F;

                string strFlags = string.Format("0x{0:x2} (", nFlags);

                if ((nFlags & 0x01) != 0)
                    strFlags += "FIN, ";
                if ((nFlags & 0x02) != 0)
                    strFlags += "SYN, ";
                if ((nFlags & 0x04) != 0)
                    strFlags += "RST, ";
                if ((nFlags & 0x08) != 0)
                    strFlags += "PSH, ";
                if ((nFlags & 0x10) != 0)
                    strFlags += "ACK, ";
                if ((nFlags & 0x20) != 0)
                    strFlags += "URG";
                strFlags += ")";

                if (strFlags.Contains("()"))
                    strFlags = strFlags.Remove(strFlags.Length - 3);
                else if (strFlags.Contains(", )"))
                    strFlags = strFlags.Remove(strFlags.Length - 3, 2);

                return strFlags;
            }
        }

        public string Checksum
        {
            get { return string.Format("0x{0:x2}", sChecksum); }
        }

        public byte[] Data
        {
            get { return byTCPData; }
        }

        public ushort MessageLength
        {
            get { return usMessageLength; }
        }
    }
}
