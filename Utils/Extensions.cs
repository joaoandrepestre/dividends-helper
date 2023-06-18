using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using DividendsHelper.Models;

namespace DividendsHelper.Utils;
public static class DictionaryExtensions {
    public static HashSet<T> GetOrAdd<TKey, T>(this Dictionary<TKey, HashSet<T>> me, TKey key) where TKey : notnull {
        if (me.TryGetValue(key, out var value)) return value;
        var v = new HashSet<T>();
        me.Add(key, v);
        return v;
    }
}

public static class HttpExtensions {
    private static string GetBaseUrl(this RequestType me) => me switch {
        RequestType.Search => "https://sistemaswebb3-listados.b3.com.br/listedCompaniesProxy/CompanyCall/GetInitialCompanies/",
        RequestType.CashProvisions => "https://sistemaswebb3-listados.b3.com.br/listedCompaniesProxy/CompanyCall/GetListedCashDividends/",
        RequestType.TradingData => "https://arquivos.b3.com.br/apinegocios/ticker/",
        _ => "",
    };

    private static string ToBase64Url(this PagedHttpRequest me) {
        var bytes = Encoding.UTF8.GetBytes(me.ToString());
        return Convert.ToBase64String(bytes);
        //.TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    public static string GetUrl(this PagedHttpRequest me) =>
        $"{me.RequestType.GetBaseUrl()}{me.ToBase64Url()}";

    public static string GetUrl(this UnpagedHttpRequest me)
    {
        var args = string.Join("/", me.Params);
        return $"{me.RequestType.GetBaseUrl()}{args}";
    }

    public static IEnumerable<T>? ParseResponse<T>(this UnpagedHttpResponse me) where T : class, new() {
        var props = typeof(T)
            .GetProperties()
            .Where(p => p is { CanRead: true, CanWrite: true })
            .ToDictionary(i => i.Name);
        var ret = new List<T>();
        foreach (var value in me.ValuesAsString) {
            var instance = new T();
            var columnsAndValues = me.Columns.Zip(value, (c, v) => (c.Name, v));
            foreach (var (c, v) in columnsAndValues)
            {
                if (!props.TryGetValue(c, out var prop)) continue;
                if (!v.TryParse(out var parsed, prop.PropertyType)) continue;
                prop.SetValue(instance, parsed);
            }
            ret.Add(instance);
        }
        return ret;
    }
}

public static class DateExtensions {
    public static int YearsUntil(this DateTime me, DateTime date) =>
       (new DateTime(1, 1, 1) + (date - me)).Year;

    public static int MonthsUntil(this DateTime me, DateTime date) =>
        ((date.Year - me.Year) * 12) + date.Month - me.Month;

    public static int DaysUntil(this DateTime me, DateTime date) =>
        (int)(date - me).TotalDays;

    public static string DateString(this DateTime me) => me == DateTime.MinValue ?
        "first recorded date" : $"{me.Day:00}/{me.Month:00}/{me.Year:0000}";
}

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