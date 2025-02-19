using Avalonia;
using Avalonia.Collections;
using Avalonia.Metadata;
using Ursa.Converters;

namespace Ursa;

/// <summary>
/// 异常消息信息类，用于相对固定的UI异常消息显示配置（例如：空指针异常消息显示内容配置）
/// </summary>
public class AvaloniaStringFormatter : AvaloniaObject, IAvaloniaStringFormatter
{
    public static readonly StyledProperty<string?> FormatStringProperty = AvaloniaProperty.Register<AvaloniaStringFormatter, string?>(nameof(FormatString));
    public static readonly StyledProperty<IStringFormatter?> StringFormatterProperty = AvaloniaProperty.Register<AvaloniaStringFormatter, IStringFormatter?>(nameof(StringFormatter));
    public static readonly StyledProperty<ParameterReplaceRulers> ParameterReplaceRulersProperty = AvaloniaProperty.Register<AvaloniaStringFormatter, ParameterReplaceRulers>(nameof(ParameterReplaceRulers));
    public static readonly StyledProperty<bool> IsSimpleStringFormatModeProperty = AvaloniaProperty.Register<AvaloniaStringFormatter, bool>(nameof(IsSimpleStringFormatMode));
    
    /// <summary>
    /// 是否使用简单字符串格式化模式
    /// </summary>
    public bool IsSimpleStringFormatMode { get => GetValue(IsSimpleStringFormatModeProperty); set => SetValue(IsSimpleStringFormatModeProperty, value); }
    
    /// <inheritdoc />
    public string? FormatString { get => GetValue(FormatStringProperty); set => SetValue(FormatStringProperty, value); }
    
    /// <inheritdoc />
    [Content]
    public ParameterReplaceRulers ParameterReplaceRulers { get => GetValue(ParameterReplaceRulersProperty); set => SetValue(ParameterReplaceRulersProperty, value); }
    
    /// <inheritdoc />
    public IStringFormatter? StringFormatter { get => GetValue(StringFormatterProperty); set => SetValue(StringFormatterProperty, value); }

    public static IAvaloniaStringFormatter CreateSimpleStringFormatter(string message)
    {
        return new AvaloniaStringFormatter
        {
            FormatString = message,
            IsSimpleStringFormatMode = true
        };
    }
    
    public string Format(IReadOnlyDictionary<string, object?>? args = null)
    {
        string? message = FormatString;
        if (string.IsNullOrWhiteSpace(message))
        {
            return string.Empty;
        }

        if (args == null || args.Count == 0)
        {
            return message;
        }

        if (IsSimpleStringFormatMode || StringFormatter == null)
        {
            return string.Format(message, args.Values.ToArray());
        }
        
        return StringFormatter.Format(message, ParameterReplaceRulers.ToImmutable(), args);
    }

    public string Format(params (string Key, object? Value)[] args)
    {
        string? message = FormatString;
        if (string.IsNullOrWhiteSpace(message))
        {
            return string.Empty;
        }

        if (args == null || args.Length == 0)
        {
            return message;
        }

        if (IsSimpleStringFormatMode || StringFormatter == null)
        {
            return string.Format(message, args.Select(x => x.Value).ToArray());
        }
        
        return StringFormatter.Format(message, ParameterReplaceRulers.ToImmutable(), args);
    }

    public string Format(string[] keys, object?[] values)
    {
        string? message = FormatString;
        if (string.IsNullOrWhiteSpace(message))
        {
            return string.Empty;
        }

        if (keys == null || keys.Length == 0)
        {
            return message;
        }

        if (IsSimpleStringFormatMode || StringFormatter == null)
        {
            return string.Format(message, values);
        }
        
        return StringFormatter.Format(message, ParameterReplaceRulers.ToImmutable(), keys, values);
    }
}

public class SharpParameterReplaceRuler : ParameterReplaceRuler
{
    public SharpParameterReplaceRuler()
    {
        this.Replacer = SharpParameterReplacer.Instance;
    }
    
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ParameterNameProperty)
        {
            this.Parameter = string.Concat("{", this.ParameterName, "}");
        }
    }
}

public class ParameterReplaceRuler : AvaloniaObject, IParameterReplaceRuler
{
    public static readonly StyledProperty<string?> ParameterNameProperty = AvaloniaProperty.Register<ParameterReplaceRuler, string?>(nameof(ParameterName));
    public static readonly StyledProperty<string?> ParameterProperty = AvaloniaProperty.Register<ParameterReplaceRuler, string?>(nameof(Parameter));
    public static readonly StyledProperty<string?> ParameterValueStringFormatProperty = AvaloniaProperty.Register<ParameterReplaceRuler, string?>(nameof(ParameterValueStringFormat));
    public static readonly StyledProperty<IParameterReplacer> ReplacerProperty = AvaloniaProperty.Register<ParameterReplaceRuler, IParameterReplacer>(nameof(Replacer));
    
    /// <summary>
    /// 不带格式的参数名称
    /// </summary>
    public string? ParameterName
    {
        get => GetValue(ParameterNameProperty);
        set => SetValue(ParameterNameProperty, value);
    }

    /// <summary>
    ///  带格式的参数表达式
    /// </summary>
    public string? Parameter
    {
        get => GetValue(ParameterProperty);
        set => SetValue(ParameterProperty, value);
    }

    /// <summary>
    /// 参数值字符串格式化器
    /// </summary>
    public string? ParameterValueStringFormat
    {
        get => GetValue(ParameterValueStringFormatProperty);
        set => SetValue(ParameterValueStringFormatProperty, value);
    }
    
    /// <summary>
    /// 参数替换器
    /// </summary>
    public IParameterReplacer Replacer
    {
        get => GetValue(ReplacerProperty);
        set => SetValue(ReplacerProperty, value);
    }
}

public class ParameterReplaceRulers : AvaloniaList<ParameterReplaceRuler>
{
    public ParameterReplaceRulers() => this.ResetBehavior = ResetBehavior.Remove;

    public IReadOnlyList<IParameterReplaceRuler> ToImmutable()
    {
        return (IReadOnlyList<IParameterReplaceRuler>)this.Select<ParameterReplaceRuler, ImmutableIParameterReplaceRuler>((Func<ParameterReplaceRuler, ImmutableIParameterReplaceRuler>)(x => new ImmutableIParameterReplaceRuler(x.ParameterName, x.Parameter, x.ParameterValueStringFormat, x.Replacer))).ToArray<ImmutableIParameterReplaceRuler>();
    }
}