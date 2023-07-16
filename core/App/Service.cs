using DividendsHelper.Core.States;
using DividendsHelper.Core.Utils;

namespace DividendsHelper.Core;

public class Service : BackgroundService {
    private readonly CoreState _coreState;

    public Service(CoreState coreState) {
        _coreState = coreState;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        try {
            Logger.Log("Starting Dividends Helper - API");
            await _coreState.Load();
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
        await _coreState.Stop();
    }
}