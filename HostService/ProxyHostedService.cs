using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Winter.InProxy;
using Winter.Model;
using Winter.OutProxy;
using Winter.Utility;

namespace Winter.HostService
{
    public class ProxyHostedService : IHostedService
    {
        private readonly ILogger _logger;
        private MySetting _setting;
        private IUserInput _input;
        private IProxySetting _proxySetting;
        private HttpProxy _httpProxy;
        public ProxyHostedService( ILogger<ProxyHostedService> logger, IOptions<MySetting> options, IUserInput input, IProxySetting proxySetting)
        {
            _logger = logger;
            _input = input;
            _proxySetting = proxySetting;
            _setting = options.Value ?? throw  new Exception("ConfigError");
            
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
             GeneratePacFile(_setting.HttpProxyPort.ToString());
            _ = Task.Run(async () =>
            {
               var pacAddress = Helper.GetPacAddress(_setting.PacServerPort);
                _proxySetting.SetPacProxy(pacAddress);
                string[] prefixes = new string[] {pacAddress};
                await new PacServer().SimpleListenerExampleAsync(prefixes);
            }, cancellationToken);
            _input.WaitingForUserInput();
            
            _logger.LogInformation("Http Proxy listening at : http://127.0.0.1:" + _setting.HttpProxyPort);
            _logger.LogInformation("Pac Proxy listening at : http://127.0.0.1:" + _setting.PacServerPort);

            _httpProxy = new HttpProxy(_setting.HttpProxyPort);
            while (true)
            {
                var client = await _httpProxy.BeginListenAsync();
                Channel channel = new Channel(client,
                    new TrojanClientOutProxy(_setting.Trojan.Pass, _setting.Trojan.Host,_setting.Trojan.Port,_setting.Trojan.ValidServerCert));
                channel.StartChannel();
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
