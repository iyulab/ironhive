using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Diagnostics;
using System.Globalization;

namespace Raggle.Stack.EFCore.Data;

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
    }
}
