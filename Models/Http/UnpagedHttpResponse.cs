namespace DividendsHelper.Models;

public class UnpagedHttpResponse {
    public string Name { get; set; }
    public IEnumerable<ColumnInfo> Columns { get; set; }
    public IEnumerable<IEnumerable<object>> Values { get; set; }
    public IEnumerable<IEnumerable<string>> ValuesAsString => Values
        .Select(i => i.Select(v => v?.ToString() ?? ""));
}

public class ColumnInfo {
    public string Name { get; set; }
    public string FriendlyName { get; set; }
    public string FriendlyNamePt { get; set; }
    public string FriendlyNameEn { get; set; }
    public int Type { get; set; }
    public int ColumnAlignment { get; set; }
    public int ValueAlignment { get; set; }
}