namespace DividendsHelper.Models;

public class Stats {
    private decimal[] Values { get; }

    public int Count => Values.Length;

    public decimal Total => Values.Sum();
    public decimal Average => Total / Count;

    public decimal Median => Values
        .OrderBy(v => v)
        .Skip((Count - 1) / 2)
        .Take(2 - Count % 2)
        .Average();

    public decimal Mode => Values
        .GroupBy(v => v)
        .OrderByDescending(g => g.Count())
        .First().Key;

    public decimal Variance => Values.Sum(v => (v - Average) * (v - Average)) / Count;
    public decimal StandardDeviation => (decimal) Math.Sqrt((double) Variance);

    public Stats(IEnumerable<decimal> values)
    {
        Values = values.ToArray();
    }
}