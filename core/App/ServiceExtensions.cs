using DividendsHelper.Core.Fetching;
using DividendsHelper.Core.States;
using DividendsHelper.Core.TelegramBot;
using DividendsHelper.Core.TelegramBot.Handlers;

namespace DividendsHelper.Core; 

public static class ServiceExtensions {
    public static IServiceCollection SetupFetching(this IServiceCollection me) =>
        me
            .AddSingleton<HttpClient>()
            .AddTransient<InstrumentFetcher>()
            .AddTransient<CashProvisionFetcher>()
            .AddTransient<TradingDataFetcher>();
    public static IServiceCollection SetupStates(this IServiceCollection me) =>
        me
            .AddSingleton<InstrumentState>()
            .AddSingleton<CashProvisionState>()
            .AddSingleton<TradingDataState>()
            .AddSingleton<CoreState>();

    public static IServiceCollection SetupTelegramBot(this IServiceCollection me) =>
        me
            .AddTransient<MonitorCommandHandler>()
            .AddTransient<SummaryCommandHandler>()
            .AddTransient<SimulationCommandHandler>()
            .AddTransient<PortfolioCommandHandler>()
            .AddSingleton<TelegramBotRouter>();
}