using DividendsHelper.Core.States;
using DividendsHelper.Core.TelegramBot;
using DividendsHelper.Core.Utils;
using Microsoft.Extensions.Hosting;

namespace DividendsHelper.Core;

public class Service : BackgroundService {
    private readonly CoreState _coreState;
    private readonly TelegramBotRouter _router;

    public Service(CoreState coreState, TelegramBotRouter router) {
        _coreState = coreState;
        _router = router;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        try {
            Logger.Log("Starting Dividends Helper");
            await _coreState.Load();
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
        await _coreState.Stop();
    }
}