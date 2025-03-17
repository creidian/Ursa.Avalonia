namespace Ursa;

/// <summary>
/// 异常消息内容格式化器接口
/// </summary>
public interface IStringFormatter
{
    string Format(string? source, IReadOnlyList<IParameterReplaceRuler> rulers, IReadOnlyDictionary<string, object?> args);
    string Format(string? source, IReadOnlyList<IParameterReplaceRuler> rulers, params (string Key, object? Value)[] args);
    string Format(string? source, IReadOnlyList<IParameterReplaceRuler> rulers, string[] keys, object?[] values);
}