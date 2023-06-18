using DividendsHelper.States;
using DividendsHelper.TelegramBot;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DividendsHelper;
internal class Program {
    private static State? _state;
    private static TelegramBotRouter? _router;
    static async Task Main(string[] args) {
        IHostBuilder builder = Host.CreateDefaultBuilder(args)
            .UseWindowsService(options => {
                options.ServiceName = "DividendsHelper";
            })
            .ConfigureServices((context, services) => {
                services.AddHostedService<Service>();
            });

        IHost host = builder.Build();
        host.Run();
    }

    static async Task<bool> Start() {
        Logger.Log("Starting Dividends Helper");
        _state = new State();
        await _state.Load();
        _router = new TelegramBotRouter(_state);
        if (!(await _router.Load())) {
            Logger.Log("Failed to load Telegram Bot. Forcefully shutting down...");
            return false;
        }
        Logger.Log("Starting done.");
        Console.WriteLine();
        return true;
    }

    static async Task Stop() {
        Logger.Log("Shutting down...");
        _router.Stop();
        await _state.Stop();
    }
}