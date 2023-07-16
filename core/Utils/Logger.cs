namespace DividendsHelper.Core.Utils;
public static class Logger {
    public static void Log(string message, LogLevel logLevel = LogLevel.Info) {
        var color = Console.ForegroundColor;
        Console.ForegroundColor = logLevel switch
        {
            LogLevel.Error => ConsoleColor.Red,
            LogLevel.Warning => ConsoleColor.Yellow,
            _ => color, 
        };
        Console.WriteLine($"{DateTime.Now} [{logLevel}] {message}");
        Console.ForegroundColor = color;
    }
}

public enum LogLevel {
    Info,
    Error,
    Warning,
}