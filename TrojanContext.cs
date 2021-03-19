using Microsoft.Extensions.Options;
using Winter.Model;

namespace Winter
{
    public class TrojanContext
    {

        private MySetting _options;

        private int _useIndex=0;

        public TrojanContext(IOptions<MySetting> options)
        {
            _options = options.Value;
        }


        public Trojan GetUseTrojan()
        {
            return _options.Trojan[_useIndex];
        }

        public void SetUseIndex(int val)
        {
            this._useIndex = val;

        }
    }
}