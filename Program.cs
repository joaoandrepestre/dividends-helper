using DividendsHelper.States;
using DividendsHelper.TelegramBot;

namespace DividendsHelper;
internal class Program {
    static async Task Main(string[] args) {

        // Start up
        Console.WriteLine("Starting Dividends Helper");
        var state = new State();
        await state.Load();
        var telegram = new TelegramBotHanlders(state);
        await telegram.Load();
        Console.WriteLine("Starting done.");
        Console.WriteLine();

        Console.WriteLine("Press enter to quit...");
        Console.ReadLine();
        Console.WriteLine("Shutting down...");
        telegram.Stop();
        await state.Stop();
    }
}