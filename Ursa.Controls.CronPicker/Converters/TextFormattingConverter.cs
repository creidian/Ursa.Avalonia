using System.Globalization;
using Avalonia.Data.Converters;

namespace Ursa.Themes.Semi.Converters;

public class TextFormattingConverter: IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter is string format)
        {
            return string.Format(format, value);
        }

        if (parameter is IAvaloniaStringFormatter formatter)
        {
            return formatter.Format(("value", value));
        }

        return value?.ToString();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class ListDateTimesTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        string format = parameter as string?? "yyyy-MM-dd HH:mm:ss dddd";
        if (value is IEnumerable<DateTime> dates)
        {
            return string.Join(Environment.NewLine, dates.Select(d => string.IsNullOrWhiteSpace(format)? d.ToString() : d.ToString(format)));
        }
        
        return value?.ToString();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class WeekDayConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int dayOfWeek)
        {
            return culture.DateTimeFormat.GetDayName(GetDayOfWeek(dayOfWeek));
        }
        
        return value?.ToString();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
    
    private static DayOfWeek GetDayOfWeek(int dayOfWeek)
    {
        switch (dayOfWeek)
        {
            case 0:
            case 7:
                return DayOfWeek.Sunday;
            case 1:
                return DayOfWeek.Monday;
            case 2:
                return DayOfWeek.Tuesday;
            case 3:
                return DayOfWeek.Wednesday;
            case 4:
                return DayOfWeek.Thursday;
            case 5:
                return DayOfWeek.Friday;
            case 6:
                return DayOfWeek.Saturday;
            default:
                throw new ArgumentOutOfRangeException(nameof(dayOfWeek), dayOfWeek, null);
        }
    }
}

public class MonthConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int month)
        {
            return culture.DateTimeFormat.GetMonthName(month);
        }
        
        return value?.ToString();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}