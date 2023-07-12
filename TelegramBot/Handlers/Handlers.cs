﻿using DividendsHelper.Models;
using DividendsHelper.States;
using DividendsHelper.Utils;
using Telegram.Bot.Types;

namespace DividendsHelper.TelegramBot.Handlers;

[TelegramMessageHandler("/monitor", "Adds a stock to be monitored for dividends")]
public class MonitorCommandHandler : BaseHandler<MonitorCommand> {
    public MonitorCommandHandler(CoreState coreState) : base(coreState) { }

    protected override MonitorCommand ValidateArgs(MonitorCommand command) {
        if (!command.Valid) return command;
        command.Symbol = command.Symbol.ToUpper();
        return command;
    }
    
    protected override async Task<string> GetResponse(MonitorCommand command) {
        var exists = await CoreState.Monitor(command.Symbol);
        if (!exists) {
            Console.WriteLine($"Received /monitor command for non existent symbol {command.Symbol} from {command.Sender}");
            return $"Could not find *{command.Symbol}* at [B3](https://www.b3.com.br/pt_br/produtos-e-servicos/negociacao/renda-variavel/empresas-listadas.htm)\nPlease check the spelling";
        }
        var summary = CoreState.CashProvisions.GetSummary(command.Symbol.ToUpper(), DateTime.Today.AddYears(-1));
        return $"Started monitoring *{command.Symbol.ToUpper()}* \nHere's the most recent data:\n\n{summary.ToMarkdown()}";
    }
}

[TelegramMessageHandler("/summary", "Gets stats for a stock and date interval")]
public class SummaryCommandHandler : BaseHandler<SummaryCommand> {
    public SummaryCommandHandler(CoreState coreState) : base(coreState) { }

    protected override SummaryCommand ValidateArgs(SummaryCommand command) {
        if (!command.Valid) return command;
        command.Symbol = command.Symbol.ToUpper();
        if (command.MaxDate > command.MinDate) return command;
        command.Valid = false;
        command.Error = $"expects _first date_ to be before _second date_ \n\nTry `{command.CommandName()} {command.Symbol} {command.MaxDate.DateString()} {command.MinDate.DateString()}`";
        return command;
    }

    protected override Task<string> GetResponse(SummaryCommand command) {
        if (!CoreState.MonitoredSymbols.Contains(command.Symbol)) {
            return Task.FromResult($"*{command.Symbol}* is not yet monitored\n\nTry `/monitor {command.Symbol}`");
        }

        var summary = CoreState.CashProvisions.GetSummary(command.Symbol, command.MinDate, command.MaxDate);
        return Task.FromResult(summary.ToMarkdown());

    }
}

[TelegramMessageHandler("/simulate", "Simulates an investment in a stock for a certain period")]
public class SimulationCommandHandler : BaseHandler<SimulationCommand>
{
    public SimulationCommandHandler(CoreState coreState) : base(coreState) { }

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
        if (!CoreState.MonitoredSymbols.Contains(command.Symbol))
        {
            return $"*{command.Symbol}* is not yet monitored\n\nTry `/monitor {command.Symbol}`";
        }

        var simulation = await CoreState.CashProvisions
            .Simulate(
                command.Symbol, 
                command.MinDate, 
                command.MaxDate, 
                command.Investment);
        return simulation.ToMarkdown();
    }
}

[TelegramMessageHandler("/portfolio", "Builds a portfolio from monitored assets to maximize earnings over a period")]
public class PortfolioCommandHandler : BaseHandler<PortfolioCommand> {
    public PortfolioCommandHandler(CoreState coreState) : base(coreState) { }

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
        var portfolio = await CoreState.CashProvisions
            .BuildPortfolio(
                CoreState.MonitoredSymbols.ToArray(),
                command.MinDate,
                command.MaxDate,
                command.Investment,
                command.Limit / 100);
        return portfolio.ToMarkdown();
    }
}
