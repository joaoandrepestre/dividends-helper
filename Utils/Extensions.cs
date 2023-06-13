﻿using System.Reflection;
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
        "first recorded date" : $"{me.Day:00}/{me.Month:00}/{me.Year:0000}";
}

public static class PropertyExtensions {
    public static bool HasAttribute<T>(this PropertyInfo me) where T : Attribute =>
        me.GetCustomAttributes(true).Any(a => a is T);

    public static T? GetAttribute<T>(this PropertyInfo me) where T : Attribute =>
        me.GetCustomAttributes(true)
        .Select(a => a as T)
        .Where(t => t is not null)
        .FirstOrDefault();
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
        return false;
    }
}