using Beef.Fetchers;
using DividendsHelper.Core.Controllers;
using DividendsHelper.Core.States;

namespace DividendsHelper.Core; 

public static class ServiceExtensions {
    public static IServiceCollection SetupFetching(this IServiceCollection me) =>
        me
            .AddSingleton<HttpClient>()
            .AddTransient<InstrumentFetcher>()
            .AddTransient<CashProvisionFetcher>()
            .AddTransient<TradingDataFetcher>();

    public static IServiceCollection SetupLoaders(this IServiceCollection me) =>
        me
            .AddTransient<InstrumentLoader>()
            .AddTransient<CashProvisionLoader>()
            .AddTransient<TradingDataLoader>();

    public static IServiceCollection SetupConverters(this IServiceCollection me) =>
        me
            .AddTransient<InstrumentDtoConverter>()
            .AddTransient<CashProvisionDtoConverter>()
            .AddTransient<TradingDataDtoConverter>();
    
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