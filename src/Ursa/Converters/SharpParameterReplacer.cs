using System.Text.RegularExpressions;

namespace Ursa.Converters;

public class SharpParameterReplacer : IParameterReplacer
{
    public static readonly SharpParameterReplacer Instance = new SharpParameterReplacer();
    public string? Replace(string? source, IParameterReplaceRulerInfo rulerInfo, object? value)
    {
        if (source is null)
        {
            return null;
        }

        return Replace(source, rulerInfo.Parameter, rulerInfo.ParameterName, rulerInfo.ParameterValueStringFormat, value);
    }
    
    public string Replace(string source, string? parameter, string? parameterName, string? valueFormat, object? value, bool isIgnoreCase = true)
    {
        return Replace(source, parameter, parameterName, valueFormat, value, isRegexMode: false, isIgnoreCase: isIgnoreCase, out int replacedCount);
    }
    
    public string Replace(string source, string? parameter, string? parameterName, string? valueFormat, object? value, bool isRegexMode, bool isIgnoreCase, out int replacedCount)
    {
        replacedCount = 0;
        if (parameter is not null)
        {
            return Replace(source, parameter, valueFormat, value, isRegexMode, isIgnoreCase, out replacedCount);
        }

        if (parameterName is null)
        {
            return source;
        }

        parameter = $"{{{parameterName}}}";
        return Replace(source, parameter, valueFormat, value, false, isIgnoreCase, out replacedCount);
    }
    
    public string Replace(string source, string parameter, string? valueFormat, object? value, bool isRegexMode, bool isIgnoreCase, out int replacedCount)
    {
        string? replacedValue = valueFormat is null ? value?.ToString() : string.Format(valueFormat, value);
        return Replace(source, parameter, replacedValue, isRegexMode, isIgnoreCase, out replacedCount);
    }
    
    public string Replace(string source, string parameter, string? replacedValue, bool isRegexMode, bool isIgnoreCase, out int replacedCount)
    {
        replacedCount = 0;
        if (isRegexMode)
        {
            Regex regex = isIgnoreCase ? new Regex(parameter, RegexOptions.IgnoreCase) : new Regex(parameter);
            MatchCollection matches = regex.Matches(source);
            foreach (Match match in matches)
            {
                if (match.Success)
                {
                    source = source.Replace(match.Value, replacedValue);
                    replacedCount++; // {0}
                }
            }
            
            return source;
        }
        
        return Replace(source, parameter, 0, replacedValue, ref replacedCount, isIgnoreCase? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture);
    }

    private static string Replace(string source, string parameter, int startIndex, string? replacedValue, ref int replacedCount, StringComparison comparison = StringComparison.CurrentCultureIgnoreCase)
    {
        int index = source.IndexOf(parameter, startIndex, comparison);
        if (index < 0)
        {
            return source;
        }

        replacedCount++;
        string rightPart = source.Substring(index + parameter.Length);
        rightPart = Replace(rightPart, parameter, 0, replacedValue, ref replacedCount, comparison);
        source = string.Concat(source.Substring(0, index), replacedValue, rightPart);
        return source;
    }
}