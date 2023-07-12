namespace DividendsHelper; 

public class ApiRequest {
    public string? Symbol { get; set; }
    public string[]? Symbols { get; set; }
    public DateTime MinDate { get; set; }
    public DateTime MaxDate { get; set; }
    public decimal Investment { get; set; }
    public decimal QtyLimit { get; set; }
}