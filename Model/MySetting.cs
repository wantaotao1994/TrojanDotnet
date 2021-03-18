using System;
using System.Collections.Generic;
using System.Text;

namespace Winter.Model
{
    public class MySetting
    {
        public Trojan Trojan { get; set;}

        public int HttpProxyPort { get; set; }
        public int PacServerPort { get; set; }

    }

    public class Trojan 
    {
        public string Host { get; set; }

        public short Port { get; set; }

        public string Pass { get; set; }
        
        public bool ValidServerCert { get; set; }



    }
}
