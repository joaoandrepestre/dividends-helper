namespace DividendsHelper.Models.Fetching;

public class UnpagedHttpRequest {
    public RequestType RequestType { get; set; }
    public string[] Params { get; set; }
}