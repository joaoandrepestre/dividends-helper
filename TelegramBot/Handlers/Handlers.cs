using DividendsHelper.Models;
using DividendsHelper.States;
using Telegram.Bot.Types;

namespace DividendsHelper.TelegramBot.Handlers;
public class MonitorCommandHandler : BaseHandler<MonitorCommand> {
    public MonitorCommandHandler(State state) : base(state) { }

    protected override MonitorCommand Parse(Message message) {
        var ret = base.Parse(message);
        var args = message.Text?.Split(" ");
        if (args is null || args.Length < 2) {
            ret.Valid = false;
            ret.Error = "expects a symbol";
            return ret;
        }
        var symbol = args[1];
        ret.Symbol = symbol.ToUpper();
        return ret;
    }

    protected override async Task<string> GetResponse(MonitorCommand command) {
        var exists = await _state.Monitor(command.Symbol);
        if (!exists) {
            Console.WriteLine($"Received /monitor command for non existent symbol {command.Symbol} from {command.Sender}");
            return $"Could not find {command.Symbol} at B3\nPlease check the spelling";
        }
        var summary = _state.CashProvisions.GetSummary(command.Symbol.ToUpper(), DateTime.Today.AddYears(-1));
        return $"Started monitoring {command.Symbol.ToUpper()}\nHere's the most recent data:\n\n``` {summary} ```";
    }
}

public class SummaryCommandHandler : BaseHandler<SummaryCommand> {
    public SummaryCommandHandler(State state) : base(state) { }

    protected override SummaryCommand Parse(Message message) {
        var ret = base.Parse(message);
        var args = message.Text?.Split(" ");
        if (args is null || args.Length < 2) {
            ret.Valid = false;
            ret.Error = "expects a symbol";
            return ret;
        }
        var symbol = args[1];
        ret.Symbol = symbol.ToUpper();

        if (args.Length >= 3) // minDate
            if (DateTime.TryParse(args[2], out var minDate))
                ret.MinDate = minDate;
        if (args.Length >= 4) // maxDate
            if (DateTime.TryParse(args[3], out var maxDate))
                ret.MaxDate = maxDate;
        return ret;
    }

    protected override Task<string> GetResponse(SummaryCommand command) {
        if (!_state.MonitoredSymbols.Contains(command.Symbol)) {
            return Task.FromResult($"{command.Symbol} is not yet monitored\n\nTry:\n/monitor {command.Symbol}");
        }

        var summary = _state.CashProvisions.GetSummary(command.Symbol, command.MinDate, command.MaxDate);
        return Task.FromResult(summary.ToMarkdown());

    }
}
