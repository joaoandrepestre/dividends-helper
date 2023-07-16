using DividendsHelper.Telegram.Utils;
using Microsoft.Extensions.Hosting;

namespace DividendsHelper.Telegram;

public class Service : BackgroundService {
    private readonly TelegramBotRouter _router;

    public Service(TelegramBotRouter router) {
        _router = router;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        try {
            Console.WriteLine("Starting Dividends Helper - Telegram");
            
            if (!(await _router.Load())) {
                Console.WriteLine("Failed to load Telegram Bot. Forcefully shutting down...");
                return;
            }

            Console.WriteLine("Starting done.");
            await stoppingToken;
        }
        catch (TaskCanceledException) { }
        finally {
            await Stop();
        }
    }

    private async Task Stop() {
        Console.WriteLine("Shutting down...");
        _router.Stop();
    }
}