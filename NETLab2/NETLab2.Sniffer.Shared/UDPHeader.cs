using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace NETLab2.Sniffer.Shared
{
    public class UDPHeader
    {
        //UDP header fields
        private ushort usSourcePort;            //Шестнадцать битов порта источника
        private ushort usDestinationPort;       //Шестнадцать битов порта назначения
        private ushort usLength;                //Длина UDP заголовка
        private short sChecksum;                //Шестнадцать битов контрольной суммы
                                                //(может иметь отрицательное значение)              
                                                //Конец полей заголовка UDP

        private byte[] byUDPData = new byte[4096];  //Данные UDP пакета

        public UDPHeader(byte[] byBuffer, int nReceived)
        {
            MemoryStream memoryStream = new MemoryStream(byBuffer, 0, nReceived);
            BinaryReader binaryReader = new BinaryReader(memoryStream);

            usSourcePort = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
            usDestinationPort = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
            usLength = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
            sChecksum = IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

            Array.Copy(byBuffer, 8, byUDPData, 0, nReceived - 8);
        }

        public string SourcePort
        {
            get { return usSourcePort.ToString(); }
        }

        public string DestinationPort
        {
            get { return usDestinationPort.ToString(); }
        }

        public string Length
        {
            get { return usLength.ToString(); }
        }

        public string Checksum
        {
            get { return string.Format("0x{0:x2}", sChecksum); }
        }

        public byte[] Data
        {
            get { return byUDPData; }
        }
    }
}
