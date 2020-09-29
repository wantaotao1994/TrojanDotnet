using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Winter.OutProxy
{
   public interface IClientOutProxy:IDisposable
    {

        public Task ConnectAsync(byte[] buffer, int offset, int size, DomainModel doman);
        public Task WriteDataAsync(byte[] buffer,int offset,int size);


        public Task<byte[]> ReadDataAsync();

    }
}
