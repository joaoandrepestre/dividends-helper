using DividendsHelper.States;
using DividendsHelper.TelegramBot;

namespace DividendsHelper;
internal class Program {
    private static State? _state;
    private static TelegramBotRouter? _router;
    static async Task Main(string[] args) {
        IHostBuilder builder = Host.CreateDefaultBuilder(args)
            .UseWindowsService(options => {
                options.ServiceName = "DividendsHelper";
            })
            .ConfigureWebHostDefaults(webHost => {
                webHost.UseStartup<ApiSetup>();
            })
            .ConfigureServices((context, services) => {
                services.AddHostedService<Service>();
            });

        IHost host = builder.Build();
        await host.RunAsync();
    }
}