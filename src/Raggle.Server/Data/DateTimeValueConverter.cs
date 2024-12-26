using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Diagnostics;
using System.Globalization;

public class DateTimeValueConverter : ValueConverter<DateTime?, string?>
{
    public DateTimeValueConverter()
        : base(
            dateTime => Serialize(dateTime),
            value => Deserialize(value))
    {
    }

    private static string? Serialize(DateTime? dateTime)
    {
        if (dateTime.HasValue)
        {
            return dateTime.Value.Kind switch
            {
                DateTimeKind.Utc => dateTime.Value.ToString("o", CultureInfo.InvariantCulture) + "Z",
                DateTimeKind.Local => dateTime.Value.ToString("o", CultureInfo.InvariantCulture),
                _ => dateTime.Value.ToString("o", CultureInfo.InvariantCulture)
            };
        }
        else
        {
            return null;
        }
    }

    private static DateTime? Deserialize(string? value)
    {
        return DateTime.UtcNow;
        //if (string.IsNullOrWhiteSpace(value))
        //{
        //    return null;
        //}
        //else if (DateTime.TryParseExact(value, "oZ", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var utcDateTime))
        //{
        //    return DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
        //}
        //else if (DateTime.TryParseExact(value, "o", CultureInfo.InvariantCulture, DateTimeStyles.None, out var localDateTime))
        //{
        //    return DateTime.SpecifyKind(localDateTime, DateTimeKind.Local);
        //}
        //else
        //{
        //    throw new FormatException($"Unable to parse '{value}' as a DateTime.");
        //}
    }
}
