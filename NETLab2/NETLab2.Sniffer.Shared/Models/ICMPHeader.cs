using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;

namespace NETLab2.Sniffer.Shared.Models
{
    public class ICMPHeader
    {
        //Поля заголовков ICMP пакета
        private byte usType;          //Восемь битов тип сообщения
        private byte usCode;          //Восемь битов код сообщения
        private ushort usChecksum = 555;  //Шестнадцать два бита для контрольной суммы
        private ushort usPointer;

        public ICMPHeader(byte[] byBuffer, int nReceived)
        {
            try
            {
                MemoryStream memoryStream = new MemoryStream(byBuffer, 0, nReceived);
                BinaryReader binaryReader = new BinaryReader(memoryStream);

                usType = binaryReader.ReadByte();
                usCode = binaryReader.ReadByte();
                usChecksum = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public string Type
        {
            get
            {
                switch (usType)
                {
                    case 0:
                        return "Эхо-ответ";
                    case 3:
                        switch (usCode)
                        {
                            case 0:
                                return "Сеть недоступна";
                            case 1:
                                return "Узел недоступен";
                            case 2:
                                return "Протокол недоступен";
                            case 3:
                                return "Порт недоступен";
                            case 4:
                                return "Необходима фрагментация";
                            case 5:
                                return "Маршрут не найден";
                            default:
                                return "Неизвестно";
                        }
                    case 4:
                        return "Превышен буфер очереди датаграммы";
                    case 5:
                        switch (usCode)
                        {
                            case 0:
                                return "Перенаправление датаграмм для сети";
                            case 1:
                                return "Перенаправление датаграмм хоста";
                            case 2:
                                return "Перенаправление датаграмм сервиса и сети";
                            case 3:
                                return "Перенаправление датаграмм сервиса и узла";
                            default:
                                return "Неизвестно";
                        }
                    case 8:
                        return "Эхо-запрос";
                    case 11:
                        return "Превышено время ожидания";
                    case 12:
                        return "Указатель содержит ошибку";
                    case 13:
                        return "Запрос метки времени";
                    case 14:
                        return "Ответ метки времени";
                    case 15:
                        return "Запрос инфрмационного сообщения";
                    case 16:
                        return "Ответ информационного сообщения";
                    default:
                        return "Неизвестно";
                }
            }
        }
    }
}
