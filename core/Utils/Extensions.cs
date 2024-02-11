using System.Globalization;
using System.Runtime.CompilerServices;

namespace DividendsHelper.Core.Utils;

public static class DictionaryExtensions {
    public static HashSet<T> GetOrAdd<TKey, T>(this Dictionary<TKey, HashSet<T>> me, TKey key) where TKey : notnull {
        if (me.TryGetValue(key, out var value)) return value;
        var v = new HashSet<T>();
        me.Add(key, v);
        return v;
    }
}
public static class StringExtensions {
    public static bool TryParse(this string me, out object ret, Type t) {
        ret = me;
        if (t == typeof(string))
            return true;
        if (t == typeof(DateTime)) {
            var b = DateTime.TryParse(me, out var date);
            ret = date;
            return b;
        }

        if (t == typeof(TimeSpan)) {
            var b = TimeSpan.TryParse(me, out var time);
            ret = time;
            return b;
        }
        if (t == typeof(decimal)) {
            var b = decimal.TryParse(me, NumberStyles.Any, CultureInfo.CreateSpecificCulture("en-us"), out var d);
            ret = d;
            return b;
        }
        if (t == typeof(int)) {
            var b = int.TryParse(me, out var i);
            ret = i;
            return b;
        }
        return false;
    }
}

public static class AsyncExtensions {
    public static CancellationTokenAwaiter GetAwaiter(this CancellationToken ct)
    {
        // return our special awaiter
        return new CancellationTokenAwaiter
        {
            CancellationToken = ct
        };
    }
    
    public struct CancellationTokenAwaiter : INotifyCompletion, ICriticalNotifyCompletion
    {
        public CancellationTokenAwaiter(CancellationToken cancellationToken)
        {
            CancellationToken = cancellationToken;
        }

        internal CancellationToken CancellationToken;

        public object GetResult()
        {
            if (IsCompleted) throw new OperationCanceledException();
            else throw new InvalidOperationException("The cancellation token has not yet been cancelled.");
        }

        public bool IsCompleted => CancellationToken.IsCancellationRequested;
        
        public void OnCompleted(Action continuation) =>
            CancellationToken.Register(continuation);
        public void UnsafeOnCompleted(Action continuation) =>
            CancellationToken.Register(continuation);
    }
}