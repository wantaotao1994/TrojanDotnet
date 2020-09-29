using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Winter.InProxy;

namespace Winter
{
    public class HttpProxy : IClientInProxy
    {
        public int _port = 8700;

        TcpListener _tcpListener;


        public HttpProxy(int port = 8700) {
            _port = port;
            _tcpListener = new TcpListener(IPAddress.Any, port);
            _tcpListener.Start();
        }
        public async Task<TcpClient> BeginListenAsync()
        {

            return await _tcpListener.AcceptTcpClientAsync();

         }


        public void Dispose()
        {
            _tcpListener.Stop();
        }
    }
}
