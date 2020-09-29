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

        TcpClient _clientInProxy;

        IClientOutProxy _clientOutProxy;

        LocalProxyStatus localProxyStatus;



        bool running = true;


        public Channel(TcpClient clientInProxy, IClientOutProxy clientOutProxy)
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

            byte[] memoryBuffer = new byte[512];
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
                        catch (Exception)
                        {
                            this.Dispose();


                        }
                        //numberOfBytesRead = stream.Read(myReadBuffer, 0, myReadBuffer.Length);
                        //myCompleteMessage.AppendFormat("{0}", Encoding.ASCII.GetString(myReadBuffer, 0, numberOfBytesRead));
                    }
                    while (stream.DataAvailable && stream.CanRead);


                    memoryStream.TryGetBuffer(out var _buffer);
                    if (result > 0)
                    {
                        await HandReciveDataFromClientAsync(_buffer, 0, _buffer.Count);
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


                    var data = await _clientOutProxy.ReadDataAsync();
                    if (data.Length > 0)
                    {
                        await _clientInProxy.GetStream().WriteAsync(data);
                    }
                    else
                    {

                        this.Dispose();
                    }

                }
                catch (Exception)
                {
                    this.Dispose();
                }
            }
        }


        private async System.Threading.Tasks.Task HandReciveDataFromClientAsync(Memory<byte> memoryBuffer, int offset, int length)
        {
            var stram = _clientInProxy.GetStream();

            var byteData = memoryBuffer.ToArray();

            if (LocalProxyStatus.Connect == localProxyStatus)
            {
                var address = SetHttpProxy(byteData);
                await _clientOutProxy.ConnectAsync(memoryBuffer.ToArray(), offset, length, address);
                localProxyStatus = LocalProxyStatus.Transfer;
                await stram.WriteAsync(Encoding.UTF8.GetBytes(HTTP_CONECTED_RES_STR));
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

            foreach (var item in dataArr)
            {
                string[] valueArr = item.Split(":");
                if (valueArr.Length >= 2)
                {
                    if (valueArr[0] == "Host")
                    {
                        address = valueArr[1].Trim();

                        if (valueArr.Length > 2)
                        {
                            port = short.Parse(valueArr[2].Trim());
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(address))
            {
                return null;
            }


            return new DomainModel(address, port);
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


        public DomainModel(string address, short port)
        {
            Address = address;
            Port = port;
        }
    }

    public enum LocalProxyStatus
    {
        Connect = 1,
        Transfer = 2

    }
}
