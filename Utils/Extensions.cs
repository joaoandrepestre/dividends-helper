using System.Text;
using DividendsHelper.Models;

namespace DividendsHelper.Utils;
public static class DictionaryExtensions {
    public static HashSet<T> GetOrAdd<TKey, T>(this Dictionary<TKey, HashSet<T>> me, TKey key) {
        if (me.TryGetValue(key, out var value)) return value;
        var v = new HashSet<T>();
        me.Add(key, v);
        return v;
    }
}

public static class PagedRequestExtensions {
    public static string GetBaseUrl(this RequestType me) => me switch {
        RequestType.Search => "https://sistemaswebb3-listados.b3.com.br/listedCompaniesProxy/CompanyCall/GetInitialCompanies/",
        RequestType.CashProvisions => "https://sistemaswebb3-listados.b3.com.br/listedCompaniesProxy/CompanyCall/GetListedCashDividends/",
        _ => "",
    };

    public static string ToBase64Url(this PagedHttpRequest me) {
        var bytes = Encoding.UTF8.GetBytes(me.ToString());
        return Convert.ToBase64String(bytes);
        //.TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    public static string GetUrl(this PagedHttpRequest me) =>
        $"{me.RequestType.GetBaseUrl()}{me.ToBase64Url()}";
}

public static class DateExtensions {
    public static int YearsUntill(this DateTime me, DateTime date) =>
       (new DateTime(1, 1, 1) + (date - me)).Year - 1;

    public static int MonthsUntill(this DateTime me, DateTime date) =>
        ((date.Year - me.Year) * 12) + date.Month - me.Month;

    public static int DaysUntill(this DateTime me, DateTime date) =>
        (int)(date - me).TotalDays;

    public static string DateString(this DateTime me) => me == DateTime.MinValue ?
        "first recorded date" : $"{me.Day}/{me.Month}/{me.Year}";
}