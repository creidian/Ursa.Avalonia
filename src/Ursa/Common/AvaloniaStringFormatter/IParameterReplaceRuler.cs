namespace Ursa;

public interface IParameterReplaceRulerInfo
{
    string? ParameterName { get; }
    string? Parameter { get; }
    string? ParameterValueStringFormat { get; }
}

public interface IParameterReplaceRuler :IParameterReplaceRulerInfo
{
    IParameterReplacer Replacer { get; }
}

public class ImmutableIParameterReplaceRuler : IParameterReplaceRuler
{
    public ImmutableIParameterReplaceRuler(string? parameterName, string? parameter, string? parameterValueStringFormat, IParameterReplacer replacer)
    {
        ParameterName = parameterName;
        Parameter = parameter;
        ParameterValueStringFormat = parameterValueStringFormat;
        Replacer = replacer;
    }

    public string? ParameterName { get; }
    public string? Parameter { get; }
    public string? ParameterValueStringFormat { get; }
    public IParameterReplacer Replacer { get; }
}
