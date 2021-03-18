using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Winter.InProxy;
using Winter.OutProxy;

namespace Winter
{
    public class Channel:IDisposable
    {
        public static string HTTP_CONECTED_RES_STR = "HTTP/1.1 200 Connection Established\r\n\r\n";
        public string Id { get; } = Guid.NewGuid().ToString();
        public static string HTTPS_PROXY_FLAG = "CONNECT";

        TcpClient _clientInProxy;

        AbsClientOutProxy _clientOutProxy;

        LocalProxyStatus localProxyStatus;



        bool running = true;


        public Channel(TcpClient clientInProxy, AbsClientOutProxy clientOutProxy)
        {
            localProxyStatus = LocalProxyStatus.Connect;
            _clientInProxy = clientInProxy;
            _clientOutProxy = clientOutProxy;
        }






        public void StartChannel()
        {
            var task1 = Task.Run(async () =>
            {
                await BeginReccivedFromLocalAsync();
            });


            var task2 = Task.Run(async () =>
            {
                await BeginReccivedFromRemoteAsync();
            });

        }

        private async System.Threading.Tasks.Task BeginReccivedFromLocalAsync()
        {
            byte[] memoryBuffer = new byte[81920];
            var stream = _clientInProxy.GetStream();

            while (running)
            {
                int result = 0;
                //   var result2 = stream.Read(data2,0,248);
                using (var memoryStream = new MemoryStream())
                {
                    do
                    {
                        try
                        {
                            result = await stream.ReadAsync(memoryBuffer);
                            memoryStream.Write(memoryBuffer, 0, result);
                        }
                        catch (Exception)  //异常吃掉  不要学 偷懒之作 因为浏览器完成后会关闭连接 
                        {
                            this.Dispose();
                        }
                    }
                    while (stream.DataAvailable && stream.CanRead);
                    memoryStream.TryGetBuffer(out var _buffer);
                    if (result > 0)
                    {
                        await HandRecieveDataFromClientAsync(_buffer, 0, _buffer.Count);
                    }
                    else
                    {
                        //TODO:  recived  null from client  
                        this.Dispose();
                    }
                }
            }
        }
        private async System.Threading.Tasks.Task BeginReccivedFromRemoteAsync()
        {
            while (running)
            {
                try
                {

                    await _clientOutProxy.ReadDataAsync(_clientInProxy.GetStream());
                    Console.WriteLine("ddd");

                }
                finally
                {
                    this.Dispose();
                } 
            }
        }


        private async System.Threading.Tasks.Task HandRecieveDataFromClientAsync(Memory<byte> memoryBuffer, int offset, int length)
        {

            var stream = _clientInProxy.GetStream();

            var byteData = memoryBuffer.ToArray();
            string str = Encoding.UTF8.GetString(byteData);
            if (LocalProxyStatus.Connect == localProxyStatus)
            {
                var address = SetHttpProxy(byteData);
                await _clientOutProxy.ConnectAsync(memoryBuffer.ToArray(), offset, length, address);
                localProxyStatus = LocalProxyStatus.Transfer;
                if (address.IsHttps)
                {
                    await stream.WriteAsync(Encoding.UTF8.GetBytes(HTTP_CONECTED_RES_STR));
                }
                else
                {
                    await _clientOutProxy.WriteDataAsync(memoryBuffer.ToArray(), offset, length);
                }

            }
            else if (LocalProxyStatus.Transfer == localProxyStatus)
            {
                await _clientOutProxy.WriteDataAsync(memoryBuffer.ToArray(), offset, length);
            }
        }

        private DomainModel SetHttpProxy(Memory<byte> memory)
        {
            string str = Encoding.UTF8.GetString(memory.ToArray(), 0, memory.Length);
            string[] dataArr = str.Split("\r\n");
            string address = "";
            short port = 80;
            bool isHttps = false;
            if (dataArr.Length>=2)
            {
                string[] firstLine = dataArr[0].Split(' ');
                string[] secondLine = dataArr[1].Split(':');

                if (firstLine.Length>=2)
                {
                    if (firstLine[0] ==HTTPS_PROXY_FLAG)  //
                    {
                        isHttps = true;
                    }
                }
                if (secondLine.Length >= 2)
                {
                    if (secondLine[0] == "Host")
                    {
                        address = secondLine[1].Trim();

                        if (secondLine.Length > 2)
                        {
                            port = short.Parse(secondLine[2].Trim());
                        }
                    }
                }
            }
            return new DomainModel(address, port, isHttps);
        }

        public void Dispose()
        {
            this._clientInProxy?.Dispose();


            this._clientOutProxy?.Dispose();
            this.running = false;
        }
    }


    public class DomainModel
    {


        public string Address { get; set; }

        public short Port { get; set; }


        public bool IsHttps { get; set; }

        public DomainModel(string address, short port,bool isHttps)
        {
            Address = address;
            Port = port;
            IsHttps = isHttps;
        }
    }

    public enum LocalProxyStatus
    {
        Connect = 1,
        Transfer = 2

    }
}
