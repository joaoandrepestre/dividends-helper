namespace DividendsHelper.Models;

[AttributeUsage(AttributeTargets.Property)]
sealed class TelegramMessageArgumentAttribute : Attribute {
    public int Position { get; }
    public bool Required { get; }
    public string ExambleIfEmpty { get; }
    public TelegramMessageArgumentAttribute(int position, bool required = false, string example = "") {
        Position = position;
        Required = required;
        ExambleIfEmpty = example;
    }
}

[AttributeUsage((AttributeTargets.Class))]
sealed class TelegramMessageHandlerAttribute : Attribute {
    public string Command { get; }
    public string Description { get; }
    public TelegramMessageHandlerAttribute(string command, string description) {
        Command = command;
        Description = description;
    }
}