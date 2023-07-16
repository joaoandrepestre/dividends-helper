using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DividendsHelper.Telegram;

internal class Program {
    static async Task Main(string[] args) {
        IHostBuilder builder = Host.CreateDefaultBuilder(args)
            .UseWindowsService(options => {
                options.ServiceName = "DividendsHelper.Bot";
            })
            .ConfigureServices((context, services) => {
                services
                    .SetupTelegramBot()
                    .AddHostedService<Service>();
            });
        IHost host = builder.Build();
        await host.RunAsync();
    }
}

