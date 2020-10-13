using System.IO;
using System.Threading.Tasks;

namespace Winter.OutProxy
{
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