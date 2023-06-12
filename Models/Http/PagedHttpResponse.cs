namespace DividendsHelper.Models;
public class PagedHttpResponse<T> {
    public PageInfo Page { get; set; } = new();
    public IEnumerable<T> Results { get; set; } = new T[] { };
}

public class PageInfo {
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }
}
