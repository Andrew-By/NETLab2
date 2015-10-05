using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NETLab2.Sniffer.Shared
{
    class SnifferSocket
    {
        private const int RECEIVE_TIMEOUT = 5000;
        private const int BUFFER_SIZE = 2048;

        private Socket _socket;
        private CancellationTokenSource _cts;

        public SnifferSocket(IPEndPoint endPoint)
        {
            byte[] byTrue = new byte[4] { 1, 0, 0, 0 };
            byte[] byOut = new byte[4];

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP);
            _socket.ReceiveTimeout = RECEIVE_TIMEOUT;
            _socket.Bind(endPoint);
            //_socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, true);
            _socket.IOControl(IOControlCode.ReceiveAll, byTrue, byOut);

            _cts = new CancellationTokenSource();
            CancellationToken ct = _cts.Token;
            Task.Run(() => Listen(ct));
        }

        private void Listen(CancellationToken cs)
        {
            byte[] Buffer = new byte[BUFFER_SIZE];
            while(!cs.IsCancellationRequested)
            {
                try
                {
                    int received = _socket.Receive(Buffer, SocketFlags.None);
                    Debug.WriteLine("Получено {0} байт", received);
                }
                catch { }
            }
        }

        public void Close()
        {
            if (_cts != null)
                _cts.Cancel();
            _socket.Close();
        }
    }
}
