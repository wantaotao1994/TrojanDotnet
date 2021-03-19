
using Microsoft.Extensions.Hosting;


using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NStack;
using Terminal.Gui;
using Winter.Gui;
using Winter.HostService;
using Winter.Model;

namespace Winter
{
    class Program
    {

        static async Task Main(string[] args) =>CreateHostBuilder(args).Build().RunAsync();
        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureHostConfiguration(configHost =>
                {
                    configHost.SetBasePath(Directory.GetCurrentDirectory());
                    configHost.AddJsonFile("client.json", optional: true);
                    configHost.AddEnvironmentVariables(prefix: "PREFIX_");
                    configHost.AddCommandLine(args);
                    
                })
             
                .ConfigureServices((context, services) =>
                {
                    services.Configure<MySetting>(context.Configuration.GetSection(("ProxySetting")));
                    services.AddSingleton<TrojanContext>();

                    services.AddSingleton<MainApp>();

                    
                    services.AddSingleton<IProxySetting,ProxySettingByWindowsRegistry>();
                    services.AddSingleton<IUserInput,ConsoleUserInput>();
                    services.AddHostedService<ProxyHostedService>();
                });
    }
}