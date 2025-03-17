namespace Ursa.Converters;

public class CronRulerSymbolNotFoundException : NotSupportedException
{
    public string Symbol { get; }

    public CronRulerSymbolNotFoundException(string symbol) : base()
    {
        Symbol = symbol;
    }

    public CronRulerSymbolNotFoundException(string symbol, string message) : base(message)
    {
        Symbol = symbol;
    }

    public CronRulerSymbolNotFoundException(string symbol, string message, Exception innerException) : base(message, innerException)
    {
        Symbol = symbol;
    }
}

// 參數未指定異常
public class ParameterNotSpecifiedException : FormatException
{
    public string ParameterName { get; }

    public ParameterNotSpecifiedException(string parameterName) : base()
    {
        ParameterName = parameterName;
    }

    public ParameterNotSpecifiedException(string parameterName, string message) : base(message)
    {
        ParameterName = parameterName;
    }

    public ParameterNotSpecifiedException(string parameterName, string message, Exception innerException) : base(message, innerException)
    {
        ParameterName = parameterName;
    }
}