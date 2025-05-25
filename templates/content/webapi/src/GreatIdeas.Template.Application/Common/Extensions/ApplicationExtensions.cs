namespace GreatIdeas.Template.Application.Common.Extensions;

public enum AgeGroup
{
    Child,
    Teen,
    Adult,
    Elderly,
}

public static class Extensions
{
    public static int GetCurrentAge(this DateOnly dateTime)
    {
        var birthDate = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0, DateTimeKind.Utc);
        var utcNow = TimeProvider.System.GetUtcNow().Date;
        int currentAge = utcNow.Year - dateTime.Year;
        if (utcNow < birthDate.AddYears(currentAge))
        {
            --currentAge;
        }
        return currentAge;
    }

    public static bool ValidateAge(DateTime entryDate, int ageLimit) =>
        DateTime.Today.Year - Convert.ToDateTime(entryDate).Year >= ageLimit;

    // Get day of week from date using switch statement
    public static string GetDayOfWeek(this DateTime date)
    {
        string dayOfWeek = date.DayOfWeek switch
        {
            DayOfWeek.Sunday => "Sunday",
            DayOfWeek.Monday => "Monday",
            DayOfWeek.Tuesday => "Tuesday",
            DayOfWeek.Wednesday => "Wednesday",
            DayOfWeek.Thursday => "Thursday",
            DayOfWeek.Friday => "Friday",
            DayOfWeek.Saturday => "Saturday",
            _ => "Magical day"
        };

        return dayOfWeek;
    }
}
