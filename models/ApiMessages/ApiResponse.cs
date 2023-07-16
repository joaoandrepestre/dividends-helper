namespace DividendsHelper.Models.ApiMessages;

public class ApiResponse<TContent> {
    public TContent? Content { get; set; }
    public bool Success { get; set; }
    public string? Feedback { get; set; }
}