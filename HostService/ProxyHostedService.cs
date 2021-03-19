using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Winter.Gui;
using Winter.InProxy;
using Winter.Model;
using Winter.OutProxy;
using Winter.Utility;

namespace Winter.HostService
{
    public class ProxyHostedService : IHostedService
    {
        private Action _action;
        private readonly ILogger _logger;
        private MySetting _setting;
        private IProxySetting _proxySetting;
        private HttpProxy _httpProxy;
        private TrojanContext _trojanContext;

        
        public ProxyHostedService( ILogger<ProxyHostedService> logger, IOptions<MySetting> options, IUserInput input, IProxySetting proxySetting, MainApp mainApp, TrojanContext trojanContext)
        {
            _logger = logger;
            _proxySetting = proxySetting;

            _trojanContext = trojanContext;
            _setting = options.Value ?? throw  new Exception("ConfigError");
            _action = mainApp.Run;

        }

        public  Task StartAsync(CancellationToken cancellationToken)
        {
             GeneratePacFile(_setting.HttpProxyPort.ToString());
            _ = Task.Run(async () =>
            {
               var pacAddress = Helper.GetPacAddress(_setting.PacServerPort);
                _proxySetting.SetPacProxy(pacAddress);
                string[] prefixes = new string[] {pacAddress};
                await new PacServer().SimpleListenerExampleAsync(prefixes);
            }, cancellationToken);
         
            _ = Task.Factory.StartNew(async () =>
            {
                _httpProxy = new HttpProxy(_setting.HttpProxyPort);
                while (true)
                {
                    var client = await _httpProxy.BeginListenAsync();
                
                    Channel channel = new Channel(client,
                        new TrojanClientOutProxy(_trojanContext.GetUseTrojan().Pass, _trojanContext.GetUseTrojan().Host,_trojanContext.GetUseTrojan().Port,_trojanContext.GetUseTrojan().ValidServerCert));
                    channel.StartChannel();
                }
            }, cancellationToken);
            while (true)
            {
                _action.Invoke();
            }
            
        }

        
        static void GeneratePacFile(string port)
        {
            string pacText = File.ReadAllText("Pac/proxy-template.pac");
            File.WriteAllText("Pac/proxy.pac", pacText.Replace("{{port_setting}}", port));
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            
            Console.WriteLine("Exit Fay Proxy");
            _proxySetting.RemoveSetting();
            return Task.CompletedTask;
        }
    }
}
