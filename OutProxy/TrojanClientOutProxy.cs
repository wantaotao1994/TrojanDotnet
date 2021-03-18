using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq.Expressions;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Winter.Utility;
using Console = Colorful.Console;

namespace Winter.OutProxy
{
    public class TrojanClientOutProxy : AbsClientOutProxy
    {

        bool _validServerCert;
        private static readonly byte[] CRLF = new byte[] { (byte)'\r', (byte)'\n' };
        bool _handshakeComplete = false;
    //    private SslStream _sslStream;
        private TcpClient _tcpClient;
        string _trojanClientDomman;
        int _trojanClientPort;
        string _trojanClientPass;
        string _targetHost;
        short _targetPot;

        public bool _initSuccuss;

        public override Stream OutStream { get; protected set; }

        public TrojanClientOutProxy(string pass, string server, int port = 443, bool validServerCert = false)
        {
            _validServerCert = validServerCert;
            _trojanClientDomman = server;
            _trojanClientPort = port;
            _trojanClientPass = pass;

        }

        public override async Task ConnectAsync(byte[] buffer, int offset, int size, DomainModel domain)
        {
            _tcpClient = new TcpClient();

            await _tcpClient.ConnectAsync(_trojanClientDomman, _trojanClientPort);
            OutStream = new SslStream(_tcpClient.GetStream());
            
            if (_validServerCert)
            {
                await ((SslStream)this.OutStream).AuthenticateAsClientAsync(_trojanClientDomman);

            }

            _targetHost = domain.Address;

            _targetPot = domain.Port;
            _initSuccuss = true;

            Console.WriteLine("Request to connect :" + _targetHost+":"+ _targetPot,Color.Green);
            ///TODO:Handle AuthenticateServer faild
            //if (_validServerCert)
            //{
            //    try
            //    {
            //        await this._sslStream.AuthenticateAsClientAsync(_trojanClientDomman);

            //    }
            //    catch (Exception e)
            //    {
            //        Console.WriteLine(); ///TODO:Handle AuthenticateServer faild
            //        throw;
            //    }
            //}

        }



        public override async Task WriteDataAsync(byte[] buffer, int offset, int size)
        {

            if (_handshakeComplete)
            {
                
                await ((SslStream)this.OutStream).WriteAsync(buffer, offset, size);
            }
            else
            {

                await WriteDataWithTrojanProtocolHead(buffer, offset, size);
            }

            await this.OutStream.FlushAsync();
        }


        /// <summary>
        /// TODO:  ipv4  protocol
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public  async Task WriteDataWithTrojanProtocolHead(byte[] data, int offset, int size)
        {

            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                var ss = CryptographyManager.SH224(_trojanClientPass).ToHexString();
                writer.Write(Encoding.UTF8.GetBytes(ss));
                writer.Write(CRLF);
                writer.Write((byte)Command.Connect);
                writer.Write((byte)AddressType.Domain);
                writer.Write((byte)_targetHost.Length);
                writer.Write(Encoding.UTF8.GetBytes(_targetHost));
                writer.Write(IPAddress.HostToNetworkOrder((short)_targetPot));
                writer.Write(CRLF);
                writer.Write(data, offset, size);
                // it always true because we use MemoryStream(int capacity) constructor
                stream.TryGetBuffer(out var buffer);

                await ((SslStream)this.OutStream).WriteAsync(buffer.Array, buffer.Offset, buffer.Count);

          


                _handshakeComplete = true;
           }


        }
        public override async Task ReadDataAsync(Stream stream)
        {
            while (!this._initSuccuss)
            {
                Thread.Sleep(100);
            }
            

            await this.OutStream.CopyToAsync(stream);

        }

        
        /// <summary>
        /// 不适用
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public override async Task<byte[]> ReadDataAsync()
        {
            byte[] data = new byte[2048];

            while (!this._initSuccuss)
            {
                Thread.Sleep(100);
            }


            using (var me = new MemoryStream())
            {
                try
                {
                    
                    await ((SslStream)this.OutStream).CopyToAsync(me);
                }
                catch (Exception e)
                {

                    throw e;
                }
                me.TryGetBuffer(out var _buffer);
                return _buffer.AsMemory(0, _buffer.Count).ToArray();
            }




        }

        public override void Dispose()
        {

            ((SslStream)this.OutStream)?.Dispose();
            

            this._tcpClient?.Dispose();
        }



        enum Command
        {
            Connect = 0x1,
            UdpAssociate = 0x3
        }

        enum AddressType
        {
            IPv4 = 0x1,
            Domain = 0x3,
            IPv6 = 0x4
        }
    }
}
