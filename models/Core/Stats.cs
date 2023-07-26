namespace DividendsHelper.Models.Core;

public class Stats {
    private decimal[] Values { get; }

    public int Count => Values.Length;

    public decimal Total => Values.Sum();
    public decimal Average => Count == 0 ? 0 : Total / Count;

    public decimal Median => Count == 0 ? 0 : Values
        .OrderBy(v => v)
        .Skip((Count - 1) / 2)
        .Take(2 - Count % 2)
        .Average();

    public decimal Mode => Count == 0 ? 0 : Values
        .GroupBy(v => v)
        .OrderByDescending(g => g.Count())
        .First().Key;

    public decimal Variance => Count == 0 ? 0 : Values.Sum(v => (v - Average) * (v - Average)) / Count;
    public decimal StandardDeviation => (decimal) Math.Sqrt((double) Variance);

    public Stats(IEnumerable<decimal> values)
    {
        Values = values.ToArray();
    }
}