using DividendsHelper.Models;
using DividendsHelper.States;
using DividendsHelper.Utils;
using Telegram.Bot.Types;

namespace DividendsHelper.TelegramBot.Handlers;
public class MonitorCommandHandler : BaseHandler<MonitorCommand> {
    public MonitorCommandHandler(State state) : base(state) { }

    protected override MonitorCommand ValidateArgs(MonitorCommand command) {
        if (!command.Valid) return command;
        command.Symbol = command.Symbol.ToUpper();
        return command;
    }
    
    protected override async Task<string> GetResponse(MonitorCommand command) {
        var exists = await _state.Monitor(command.Symbol);
        if (!exists) {
            Console.WriteLine($"Received /monitor command for non existent symbol {command.Symbol} from {command.Sender}");
            return $"Could not find *{command.Symbol}* at [B3](https://www.b3.com.br/pt_br/produtos-e-servicos/negociacao/renda-variavel/empresas-listadas.htm)\nPlease check the spelling";
        }
        var summary = _state.CashProvisions.GetSummary(command.Symbol.ToUpper(), DateTime.Today.AddYears(-1));
        return $"Started monitoring *{command.Symbol.ToUpper()}* \nHere's the most recent data:\n\n{summary.ToMarkdown()}";
    }
}

public class SummaryCommandHandler : BaseHandler<SummaryCommand> {
    public SummaryCommandHandler(State state) : base(state) { }

    protected override SummaryCommand ValidateArgs(SummaryCommand command) {
        if (!command.Valid) return command;
        command.Symbol = command.Symbol.ToUpper();
        if (command.MaxDate >= command.MinDate) return command;
        command.Valid = false;
        command.Error = $"expects _first date_ to be before _second date_ \n\nTry `{command.CommandName()} {command.Symbol} {command.MaxDate.DateString()} {command.MinDate.DateString()}`";
        return command;
    }

    protected override Task<string> GetResponse(SummaryCommand command) {
        if (!_state.MonitoredSymbols.Contains(command.Symbol)) {
            return Task.FromResult($"*{command.Symbol}* is not yet monitored\n\nTry `/monitor {command.Symbol}`");
        }

        var summary = _state.CashProvisions.GetSummary(command.Symbol, command.MinDate, command.MaxDate);
        return Task.FromResult(summary.ToMarkdown());

    }
}
