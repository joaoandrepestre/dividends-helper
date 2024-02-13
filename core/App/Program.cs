using DividendsHelper.Core.States;

namespace DividendsHelper.Core;
internal class Program {
    private static CoreState? _state;
    static async Task Main(string[] args) {
        IHostBuilder builder = Host.CreateDefaultBuilder(args)
            .UseWindowsService(options => {
                options.ServiceName = "DividendsHelper.Api";
            })
            .ConfigureServices((context, services) => {
                services
                    .SetupFetching()
                    .SetupLoaders()
                    .SetupConverters()
                    .SetupStates()
                    .SetupApiConfig()
                    .AddHostedService<Service>();
            })
            .ConfigureWebHostDefaults(webHost => {
                webHost.UseStartup<ApiSetup>();
            });

        IHost host = builder.Build();
        await host.RunAsync();
    }
}