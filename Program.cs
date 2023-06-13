using DividendsHelper.States;
using DividendsHelper.TelegramBot;

namespace DividendsHelper;
internal class Program {
    static async Task Main() {

        // Start up
        Logger.Log("Starting Dividends Helper");

        var state = new State();
        await state.Load();

        var telegram = new TelegramBotRouter(state);
        if (!(await telegram.Load())) {
            Logger.Log("Failed to load Telegram Bot. Forcefully shutting down...");
            return;
        }

        Logger.Log("Starting done.");
        Console.WriteLine();

        Logger.Log("Press enter to quit...");

        Console.ReadLine();
        Logger.Log("Shutting down...");
        telegram.Stop();
        await state.Stop();
    }
}