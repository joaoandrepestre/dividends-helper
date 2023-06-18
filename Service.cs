using DividendsHelper.States;
using DividendsHelper.TelegramBot;
using DividendsHelper.Utils;
using Microsoft.Extensions.Hosting;

namespace DividendsHelper;

public class Service : BackgroundService {
    private State? _state;
    private TelegramBotRouter? _router;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        try {
            Logger.Log("Starting Dividends Helper");
            _state = new State();
            await _state.Load();
            _router = new TelegramBotRouter(_state);
            if (!(await _router.Load())) {
                Logger.Log("Failed to load Telegram Bot. Forcefully shutting down...");
                return;
            }

            Logger.Log("Starting done.");
            await stoppingToken;
        }
        catch (TaskCanceledException) { }
        finally {
            await Stop();
        }
    }

    private async Task Stop() {
        Logger.Log("Shutting down...");
        _router.Stop();
        await _state.Stop();
    }
}