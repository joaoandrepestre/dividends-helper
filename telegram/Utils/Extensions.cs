using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DividendsHelper.Telegram.Utils; 

public static class PropertyExtensions {
    public static bool HasAttribute<T>(this PropertyInfo me) where T : Attribute =>
        me.GetCustomAttributes(true).Any(a => a is T);

    public static T? GetAttribute<T>(this PropertyInfo me) where T : Attribute =>
        me
            .GetCustomAttributes(true)
            .Select(a => a as T)
            .FirstOrDefault(t => t is not null);

    public static T? GetAttribute<T>(this Type me) where T : Attribute =>
        me
            .GetCustomAttributes(true)
            .Select(a => a as T)
            .FirstOrDefault(t => t is not null);

    public static IEnumerable<Type> GetTypesWithAttribute<T>(this Assembly assembly) where T : Attribute {
        foreach(var type in assembly.GetTypes()) {
            if (type.GetCustomAttributes(typeof(T), true).Length > 0) {
                yield return type;
            }
        }
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