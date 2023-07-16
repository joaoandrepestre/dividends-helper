using DividendsHelper.Models.Utils;
using DividendsHelper.Telegram.Messages;
using DividendsHelper.Telegram.ApiClient;

namespace DividendsHelper.Telegram.Handlers;

[TelegramMessageHandler("/monitor", "Adds a stock to be monitored for dividends")]
public class MonitorCommandHandler : BaseHandler<MonitorCommand> {
    public MonitorCommandHandler(DhApiClient api) : base(api) { }

    protected override MonitorCommand ValidateArgs(MonitorCommand command) {
        if (!command.Valid) return command;
        command.Symbol = command.Symbol.ToUpper();
        return command;
    }
    
    protected override async Task<string> GetResponse(MonitorCommand command) {
        var summary = await Api.Monitor(command.Symbol);
        if (summary is null) {
            Console.WriteLine($"Received /monitor command for non existent symbol {command.Symbol} from {command.Sender}");
            return $"Could not find *{command.Symbol}* at [B3](https://www.b3.com.br/pt_br/produtos-e-servicos/negociacao/renda-variavel/empresas-listadas.htm)\nPlease check the spelling";
        }
        return $"Started monitoring *{command.Symbol.ToUpper()}* \nHere's the most recent data:\n\n{summary.ToMarkdown()}";
    }
}

[TelegramMessageHandler("/summary", "Gets stats for a stock and date interval")]
public class SummaryCommandHandler : BaseHandler<SummaryCommand> {
    public SummaryCommandHandler(DhApiClient api) : base(api) { }

    protected override SummaryCommand ValidateArgs(SummaryCommand command) {
        if (!command.Valid) return command;
        command.Symbol = command.Symbol.ToUpper();
        if (command.MaxDate > command.MinDate) return command;
        command.Valid = false;
        command.Error = $"expects _first date_ to be before _second date_ \n\nTry `{command.CommandName()} {command.Symbol} {command.MaxDate.DateString()} {command.MinDate.DateString()}`";
        return command;
    }

    protected override async Task<string> GetResponse(SummaryCommand command) {
        var monitored = await Api.GetMonitoredSymbols();
        if (!monitored.Contains(command.Symbol)) {
            return $"*{command.Symbol}* is not yet monitored\n\nTry `/monitor {command.Symbol}`";
        }

        var summary = await Api.GetSummary(command.Symbol, command.MinDate, command.MaxDate);
        return summary.ToMarkdown();
    }
}

[TelegramMessageHandler("/simulate", "Simulates an investment in a stock for a certain period")]
public class SimulationCommandHandler : BaseHandler<SimulationCommand>
{
    public SimulationCommandHandler(DhApiClient api) : base(api) { }

    protected override SimulationCommand ValidateArgs(SimulationCommand command) {
        if (!command.Valid) return command;
        command.Symbol = command.Symbol.ToUpper();
        if (command.Investment <= 0)
        {
            command.Valid = false;
            command.Error = $"expects _investment_ to be greater than 0";
            return command;
        }
        if (command.MaxDate > command.MinDate) return command;
        command.Valid = false;
        command.Error = $"expects _first date_ to be before _second date_ \n\nTry `{command.CommandName()} {command.Symbol} {command.MaxDate.DateString()} {command.MinDate.DateString()}`";
        return command;
    }
    
    protected override async Task<string> GetResponse(SimulationCommand command) {
        var monitored = await Api.GetMonitoredSymbols();
        if (!monitored.Contains(command.Symbol))
        {
            return $"*{command.Symbol}* is not yet monitored\n\nTry `/monitor {command.Symbol}`";
        }

        var simulation = await Api
            .Simulate(
                command.Symbol, 
                command.Investment,
                command.MinDate, 
                command.MaxDate);
        return simulation.ToMarkdown();
    }
}

[TelegramMessageHandler("/portfolio", "Builds a portfolio from monitored assets to maximize earnings over a period")]
public class PortfolioCommandHandler : BaseHandler<PortfolioCommand> {
    public PortfolioCommandHandler(DhApiClient api) : base(api) { }

    protected override PortfolioCommand ValidateArgs(PortfolioCommand command) {
        if (!command.Valid) return command;
        if (command.Investment <= 0)
        {
            command.Valid = false;
            command.Error = $"expects _investment_ to be greater than 0";
            return command;
        }
        if (command.MaxDate > command.MinDate) return command;
        command.Valid = false;
        command.Error = $"expects _first date_ to be before _second date_ \n\nTry `{command.CommandName()} {command.Limit} {command.Investment} {command.MaxDate.DateString()} {command.MinDate.DateString()}`";
        return command;
    }

    protected override async Task<string> GetResponse(PortfolioCommand command) {
        var portfolio = await Api
            .BuildPortfolio(
                command.Limit / 100,
                command.Investment,
                command.MinDate,
                command.MaxDate);
        return portfolio.ToMarkdown();
    }
}
