namespace DividendsHelper.Models.Utils;

public static class NumberExtensions {
    public static decimal ConvertInterestRate(this decimal me, int originNumberOfDays, int targetNumberOfDays) =>
        100m * ((decimal)Math.Pow((double)(1 + me / 100), (double)targetNumberOfDays / originNumberOfDays) - 1);
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