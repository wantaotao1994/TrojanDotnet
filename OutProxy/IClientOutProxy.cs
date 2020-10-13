using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Winter.OutProxy
{
    public interface IClientOutProxy : IDisposable
    {

        public Task ConnectAsync(byte[] buffer, int offset, int size, DomainModel doman);
        public Task WriteDataAsync(byte[] buffer, int offset, int size);


        public Task<byte[]> ReadDataAsync();
        public Task ReadDataAsync(Stream stream);


    }

    public abstract class AbsClientOutProxy:IClientOutProxy
    {
        public abstract System.IO.Stream OutStream { protected set; get; }

        public abstract Task ConnectAsync(byte[] buffer, int offset, int size, DomainModel doman);
        public abstract void Dispose();
        public abstract Task<byte[]> ReadDataAsync();
        public abstract Task ReadDataAsync(Stream stream);
        public abstract Task WriteDataAsync(byte[] buffer, int offset, int size);
    }
}
