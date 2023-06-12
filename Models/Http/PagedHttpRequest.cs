namespace DividendsHelper.Models;
public class PagedHttpRequest {
    private static string _emptyString = "aaaaa";

    public RequestType RequestType { get; set; }

    public string Language { get; set; } = "en-us";
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 60;

    private string _tradingName;
    public string TradingName {
        get => string.IsNullOrEmpty(_tradingName) ? _emptyString : _tradingName;
        set => _tradingName = value;
    }

    private string _company;
    public string Company {
        get => string.IsNullOrEmpty(_company) ? _emptyString : _company;
        set => _company = value;
    }

    public override string ToString() =>
        $"{{\"language\":\"{Language}\",\"pageNumber\":{PageNumber},\"pageSize\":{PageSize},\"tradingName\":\"{TradingName}\",\"company\":\"{Company}\"}}";
}

public enum RequestType {
    Search,
    CashProvisions
}
