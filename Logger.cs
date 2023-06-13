namespace DividendsHelper;
public class Logger {
    public static void Log(string message, LogLevel logLevel = LogLevel.Info) {
        var color = Console.ForegroundColor;
        Console.ForegroundColor = logLevel == LogLevel.Error ? ConsoleColor.Red : color;
        Console.WriteLine($"{DateTime.Now} [{logLevel}] {message}");
        Console.ForegroundColor = color;
    }
}

public enum LogLevel {
    Info,
    Error,
}