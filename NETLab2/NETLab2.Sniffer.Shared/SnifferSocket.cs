using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NETLab2.Sniffer.Shared
{
    class SnifferSocket
    {
        private const int RECIEVE_TIMEOUT = 5000;

        private Socket _socket;
        private CancellationTokenSource _cts;

        public SnifferSocket(IPEndPoint endPoint)
        {
            byte[] byTrue = new byte[4] { 1, 0, 0, 0 };
            byte[] byOut = new byte[4] { 1, 0, 0, 0 };

            _socket = new Socket(SocketType.Raw, ProtocolType.IP);
            _socket.ReceiveTimeout = RECIEVE_TIMEOUT;
            _socket.Bind(endPoint);
            //_socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, 1);
            //_socket.IOControl(IOControlCode.ReceiveAll, byTrue, byOut);

            _cts = new CancellationTokenSource();
            CancellationToken ct = _cts.Token;
            Task.Run(() => Listen(ct));
        }

        private void Listen(CancellationToken cs)
        {
            while(!cs.IsCancellationRequested)
            {
                
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
