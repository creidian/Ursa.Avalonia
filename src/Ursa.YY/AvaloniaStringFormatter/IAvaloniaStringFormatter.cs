namespace Ursa;

/// <summary>
/// 异常消息信息接口，用于相对固定的UI异常消息显示配置（例如：空指针异常消息显示内容配置）
/// </summary>
public interface IAvaloniaStringFormatter
{
    /// <summary>
    /// 异常消息内容(允许带格式化参数)
    /// </summary>
    string? FormatString { get; }
    
    /// <summary>
    /// 参数替换规则集
    /// </summary>
    ParameterReplaceRulers ParameterReplaceRulers { get; }
    
    /// <summary>
    /// 异常消息内容格式化器
    /// </summary>
    IStringFormatter? StringFormatter { get; }
    
    /// <summary>
    /// 格式化异常消息内容
    /// </summary>
    /// <param name="args"> 参数字典 </param>
    /// <returns> 格式化后的异常消息内容 </returns>
    string Format(IReadOnlyDictionary<string, object?>? args = null);
    string Format(params (string Key, object? Value)[] args);
    string Format(string[] keys, object?[] values);
}
