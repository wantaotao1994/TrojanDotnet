using System.Drawing;
using System.Threading.Tasks;
using Colorful;
using Microsoft.Extensions.Options;
using Winter.Model;
using Winter.Utility;


namespace Winter
{
    public  class  ConsoleUserInput:IUserInput
    {
        private IProxySetting _proxySetting;

        private MySetting _options;
        public ConsoleUserInput(IProxySetting proxySetting, IOptions<MySetting> options)
        {
            _proxySetting = proxySetting;
            _options = options.Value;
        }

        public void WaitingForUserInput()
        {
            PrintVersion();
            _ = Task.Run(() =>
                {
                    while (true)
                    {
                        var key = Console.Read();

                        if (key == 'h')
                        {
                            PrintHelp();
                        }
                        else if (key == '1')
                        {
                            _proxySetting.SetPacProxy(Helper.GetPacAddress(_options.PacServerPort));

                            Console.WriteLine("Pac Proxy set  success");
                        }
                        else if (key == '2')
                        {
                            _proxySetting.SetGlobalProxy("127.0.0.1:" + _options.HttpProxyPort);

                            Console.WriteLine("Global Proxy set  success");
                        }
                    }
                }
            );
            
        }
        
        
        void PrintVersion()
        {
            int DA = 244;
            int V = 212;
            int ID = 255;

            Console.WriteAscii("Fay PROXY", Color.FromArgb(DA, V, ID));
        }

        void PrintHelp()
        {
            Console.WriteLine(
                "*************************************Jiang Jiang*****************************************************",
                Color.Green);
            Console.WriteLine("1: Pac Proxy", Color.Green);
            Console.WriteLine("2: Global Proxy", Color.Green);
            Console.WriteLine("Please enter key : 1 or 2", Color.Green);
            Console.WriteLine(
                "***************************************************************************************************",
                Color.Green);
        }
        
        
    }
}