namespace Ursa.Common;

public static class AvaloniaStringFormatterExtensions
{
    public static Exception ExceptionFormat(this IAvaloniaStringFormatter? formatter, Func<string, Exception> exceptionFactory, params (string, object?)[] args)
    {
        if (formatter is null)
        {
            return exceptionFactory(string.Empty);
        }

        return exceptionFactory(formatter.Format(args));
    }
}