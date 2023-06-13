namespace DividendsHelper.Models;

[System.AttributeUsage(AttributeTargets.Property)]
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