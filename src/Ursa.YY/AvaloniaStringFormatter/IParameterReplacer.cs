namespace Ursa;

public interface IParameterReplacer
{
    string? Replace(string? source, IParameterReplaceRulerInfo rulerInfo, object? value);
}