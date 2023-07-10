namespace DividendsHelper.Utils; 

public static class Algorithms {
    public enum KnapsackAlgo {
        Dynamic,
        Greedy, 
        Fptas,
    }

    public static int[] Knapsack(decimal[] values, decimal[] weights, decimal maxWeight, decimal limit, KnapsackAlgo algo) =>
        algo switch {
            KnapsackAlgo.Dynamic => KnapsackDynamic(
                values.Select(v => (int) Math.Floor(v)).ToArray(),
                weights.Select(w => (int) Math.Floor(w)).ToArray(), 
                (int) Math.Floor(maxWeight), limit),
            KnapsackAlgo.Greedy => KnapsackGreedy(values, weights, maxWeight, limit),
            KnapsackAlgo.Fptas => KnapsackFptas(values, weights, maxWeight, limit),
            _ => throw new NotImplementedException($"Algorithm {algo} is not implemented"),
        };

    private static int[] KnapsackDynamic(int[] values, int[] weights, int maxWeight, decimal limit) {
        var m = new int[maxWeight+1];
        
        for (var i = 0; i <= maxWeight; i++) {
            for (var j = 0; j < values.Length; j++) {
                if (weights[j] <= i) {
                    m[i] = Math.Max(m[i], m[i-weights[j]]+values[j]);
                }
            }
        }

        var w = maxWeight;
        var solution = new int[values.Length];
        var minWeight = weights.Min();
        while (w > 0) {
            for (var i = 0; i < solution.Length; i++) {
                if (w-weights[i] >=0 && m[w] - values[i] == m[w-weights[i]]) {
                    solution[i]++;
                    w -= weights[i];
                }
                if (w < minWeight) break;
            }
            if (w < minWeight) break;
        }
        return solution;
    }

    private static int[] KnapsackGreedy(decimal[] values, decimal[] weights, decimal maxWeight, decimal limit) {
        var valueOverWeight = values
            .Select((v, index) => (index, v, weights[index], v / weights[index]))
            .OrderByDescending(i => i.Item4)
            .Select(i => (i.index, weights[i.index]));
        var solution = new int[values.Length];
        var W = maxWeight;
        var maxVol = limit * maxWeight;
        foreach (var (i, w) in valueOverWeight) {
            var maxQty = (int)Math.Floor(maxVol / w);
            var qty = (int)Math.Min(Math.Floor(W / w), maxQty);
            solution[i] = qty;
            W -= qty * w;
        }
        return solution;
    }

    private static int[] KnapsackFptas(decimal[] values, decimal[] weights, decimal maxWeight, decimal limit) {
        var epsilon = 0.001m;
        var P = values.Max();
        var K = epsilon * P / values.Length;
        var adjustedValues = values.Select(v => (int) Math.Floor(v / K)).ToArray();
        var adjustedWeights = weights.Select(w => (int) Math.Floor(w)).ToArray();
        var dynamic = KnapsackDynamic(adjustedValues, adjustedWeights, (int) Math.Floor(maxWeight), limit);
        return dynamic;
    }
}