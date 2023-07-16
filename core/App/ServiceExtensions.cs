using DividendsHelper.Core.Controllers;
using DividendsHelper.Core.Fetching;
using DividendsHelper.Core.States;

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

    public static IServiceCollection SetupApiConfig(this IServiceCollection me) =>
        me
            .AddSingleton<InstrumentConfig>()
            .AddSingleton<CashProvisionConfig>()
            .AddSingleton<TradingDataConfig>();
}