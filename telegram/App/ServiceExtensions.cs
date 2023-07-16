using DividendsHelper.Telegram.ApiClient;
using DividendsHelper.Telegram.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace DividendsHelper.Telegram; 

public static class ServiceExtensions {
    public static IServiceCollection SetupTelegramBot(this IServiceCollection me) =>
        me
            .AddSingleton<DhApiClient>()
            .AddTransient<MonitorCommandHandler>()
            .AddTransient<SummaryCommandHandler>()
            .AddTransient<SimulationCommandHandler>()
            .AddTransient<PortfolioCommandHandler>()
            .AddSingleton<TelegramBotRouter>();
}