using System.Globalization;
using Avalonia.Controls.Notifications;
using Avalonia.Data.Converters;
using Ursa.Controls;

namespace Ursa.Converters;

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

public class CronExpressionParseToTimesResultConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is CronExpressionParseResult result0 && result0.Status != NotificationType.Success)
        {
            return result0.Message;
        }
        
        if (value is CronExpressionParseToTimesResult result)
        {
            string format = parameter as string ?? "yyyy-MM-dd HH:mm:ss dddd";
            return string.Join(Environment.NewLine, result.RunTimes.Select(d => string.IsNullOrWhiteSpace(format)? d.ToString() : d.ToString(format)));
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
        
        if (value is byte dayOfWeek2)
        {
            return culture.DateTimeFormat.GetDayName(GetDayOfWeek(dayOfWeek2));
        }

        if (int.TryParse(value?.ToString(), out int dayOfWeek3))
        {
            return culture.DateTimeFormat.GetDayName(GetDayOfWeek(dayOfWeek3));
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
        
        if (value is byte month2)
        {
            return culture.DateTimeFormat.GetMonthName(month2);
        }

        if (int.TryParse(value?.ToString(), out int month3))
        {
            return culture.DateTimeFormat.GetMonthName(month3);
        }

        return value?.ToString();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public static class CronPickerConverters
{
}