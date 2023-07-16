using DividendsHelper.Core.States;
using DividendsHelper.Core.TelegramBot;

namespace DividendsHelper.Core;
internal class Program {
    private static CoreState? _state;
    private static TelegramBotRouter? _router;
    static async Task Main(string[] args) {
        IHostBuilder builder = Host.CreateDefaultBuilder(args)
            .UseWindowsService(options => {
                options.ServiceName = "DividendsHelper";
            })
            .ConfigureServices((context, services) => {
                services
                    .SetupFetching()
                    .SetupStates()
                    .SetupApiConfig()
                    .SetupTelegramBot()
                    .AddHostedService<Service>();
            })
            .ConfigureWebHostDefaults(webHost => {
                webHost.UseStartup<ApiSetup>();
            });

        IHost host = builder.Build();
        await host.RunAsync();
    }
}