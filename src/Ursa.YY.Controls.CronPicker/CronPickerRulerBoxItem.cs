using System.Collections;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Metadata;

namespace Ursa.Controls;

/// <summary>
/// 基于 <see cref="CronPickerRuler"/> 基础控件的自定义控件。
/// </summary>
public abstract class CronPickerRulerBoxItem : /*ContentControl*/ TemplatedControl, ICronPickerRulerItem
{
    public static readonly StyledProperty<CronFieldTypes> FieldTypeProperty = AvaloniaProperty.Register<CronPickerRulerBoxItem, CronFieldTypes>(nameof(FieldType));
    public static readonly StyledProperty<int> RulerCodeProperty = AvaloniaProperty.Register<CronPickerRulerBoxItem, int>(nameof(RulerCode));
    public static readonly StyledProperty<int> PriorityProperty = AvaloniaProperty.Register<CronPickerRulerBoxItem, int>(nameof(Priority));
    public static readonly StyledProperty<string> SymbolProperty = AvaloniaProperty.Register<CronPickerRulerBoxItem, string>(nameof(Symbol));
    public static readonly StyledProperty<string> ValueProperty = AvaloniaProperty.Register<CronPickerRulerBoxItem, string>(nameof(Value), defaultBindingMode: Avalonia.Data.BindingMode.OneWay);
    public static readonly RoutedEvent<UsualValueChangedEventArgs<string>> ValueChangedEvent = RoutedEvent.Register<CronPickerRulerBoxItem, UsualValueChangedEventArgs<string>>(nameof(ValueChanged), RoutingStrategies.Bubble);

    private ICronPickerRulerItemParent? _parent;
    public CronFieldTypes FieldType
    {
        get => GetValue(FieldTypeProperty);
        set => SetValue(FieldTypeProperty, value);
    }

    public int RulerCode
    {
        get => GetValue(RulerCodeProperty);
        set => SetValue(RulerCodeProperty, value);
    }

    public string Symbol
    {
        get => GetValue(SymbolProperty);
        set => SetValue(SymbolProperty, value);
    }

    public event EventHandler<UsualValueChangedEventArgs<string>>? ValueChanged
    {
        add => AddHandler(ValueChangedEvent, value);
        remove => RemoveHandler(ValueChangedEvent, value);
    }

    public int Priority
    {
        get => GetValue(PriorityProperty);
        set => SetValue(PriorityProperty, value);
    }
    
    public string Value => GetValue(ValueProperty);

    public CronPickerRulerBoxItem()
    {
    }
    
    public abstract void VerifyCurrentCronValue();

    public virtual CronParseResult ParseTo(string text) => CronParseResult.UnSupported();

    protected void ChangeValue(string value) => SetValue(ValueProperty, value);

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SymbolProperty)
        {
            this.OnSymbolChanged();
        }
        else if (change.Property == ValueProperty)
        {
            this.RaiseEvent(new UsualValueChangedEventArgs<string>(ValueChangedEvent, change.GetOldValue<string>(), change.GetNewValue<string>()));
            this.OnValueChanged();
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _parent = GetParent<ICronPickerRulerItemParent>();
    }

    protected void OnValueChanged()
    {
        if (_parent is not null && this.IsLoaded)
        {
            _parent.ValueChanged(this.Value);
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (_parent is not null)
        {
            _parent.OnRulerBoxItemLoaded(this);
        }
    }

    protected void OnSymbolChanged() => this.RefreshCurrentValue();

    protected abstract void RefreshCurrentValue();

    private T? GetParent<T>()
    {
        StyledElement? parent = this.Parent;
        while (parent is not null)
        {
            if (parent is T t)
            {
                return t;
            }

            parent = parent.Parent;
        }

        return default;
    }
}

public class OnlySymbolCronPickerRulerBoxItem : CronPickerRulerBoxItem
{
    public static readonly StyledProperty<bool> IsNullEnableProperty = AvaloniaProperty.Register<OnlySymbolCronPickerRulerBoxItem, bool>(nameof(IsNullEnable));
    public static readonly StyledProperty<IAvaloniaStringFormatter?> SymbolNullOrWhitespaceMessageProperty = AvaloniaProperty.Register<OnlySymbolCronPickerRulerBoxItem, IAvaloniaStringFormatter?>(nameof(SymbolNullOrWhitespaceMessage));
    public static readonly StyledProperty<IAvaloniaStringFormatter?> ValueSymbolNotMatchMessageProperty = AvaloniaProperty.Register<OnlySymbolCronPickerRulerBoxItem, IAvaloniaStringFormatter?>(nameof(ValueSymbolNotMatchMessage));

    public bool IsNullEnable
    {
        get => GetValue(IsNullEnableProperty);
        set => SetValue(IsNullEnableProperty, value);
    }

    /// <summary>
    /// <see cref="CronPickerRulerBoxItem.Symbol"/> 为空白时的消息。
    /// </summary>
    public IAvaloniaStringFormatter? SymbolNullOrWhitespaceMessage
    {
        get => GetValue(SymbolNullOrWhitespaceMessageProperty);
        set => SetValue(SymbolNullOrWhitespaceMessageProperty, value);
    }

    /// <summary>
    /// <see cref="CronPickerRulerBoxItem.Value"/> 与 <see cref="CronPickerRulerBoxItem.Symbol"/> 不匹配时的消息。
    /// </summary>
    /// <remarks>
    /// 提供参数(顺序：value, symbol)：
    /// - value: 当前值。
    /// - symbol: 符号。
    /// </remarks>
    public IAvaloniaStringFormatter? ValueSymbolNotMatchMessage
    {
        get => GetValue(ValueSymbolNotMatchMessageProperty);
        set => SetValue(ValueSymbolNotMatchMessageProperty, value);
    }
    
    public override void VerifyCurrentCronValue()
    {
        if (string.IsNullOrWhiteSpace(this.Value) && IsNullEnable)
        {
            return;
        }
        
        if (string.IsNullOrWhiteSpace(this.Symbol) && !IsNullEnable)
        {
            throw new Exception(SymbolNullOrWhitespaceMessage?.Format());
        }
        
        if (this.Value != this.Symbol)
        {
            throw new Exception(ValueSymbolNotMatchMessage?.Format(("value", this.Value), ("symbol", this.Symbol)));
        }
    }

    public override CronParseResult ParseTo(string text)
    {
        if (string.Equals(Symbol, text))
        {
            return CronParseResult.Success();
        }

        return CronParseResult.UnSupported();
    }

    protected override void RefreshCurrentValue() => this.ChangeValue(Symbol);
}

public abstract class WithParamsCronPickerRulerBoxItem : CronPickerRulerBoxItem
{
    public static readonly StyledProperty<IAvaloniaStringFormatter?> ValueStringFormatterProperty = AvaloniaProperty.Register<WithParamsCronPickerRulerBoxItem, IAvaloniaStringFormatter?>(nameof(ValueStringFormatter));
    
    public virtual IAvaloniaStringFormatter? ValueStringFormatter
    {
        get => GetValue(ValueStringFormatterProperty);
        set => SetValue(ValueStringFormatterProperty, value);
    }

    /// <summary>
    /// 是否符号始终包含在值中。
    /// </summary>
    protected virtual bool IsSymbolAlwaysContain { get; } = true;
    
    public override CronParseResult ParseTo(string text)
    {
        if (IsSymbolAlwaysContain && !text.Contains(Symbol))
        {
            return CronParseResult.UnSupported();
        }
        
        string[] args = this.ReadParamsFrom(text);
        return this.ParseTo(args);
    }

    protected virtual string[] ReadParamsFrom(string text)
    {
        return text.Split(new string[] { Symbol }, StringSplitOptions.RemoveEmptyEntries);
    }

    protected abstract CronParseResult ParseTo(string[] args);

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ValueStringFormatterProperty)
        {
            this.RefreshCurrentValue();
        }
    }

    protected void RefreshValue(params (string Key, object? Value)[] paramArray) => RefreshValue((IEnumerable<(string Key, object? Value)>)paramArray);
    protected void RefreshValue(IEnumerable<(string Key, object? Value)> paramArray)
    {
        if (ValueStringFormatter == null)
        {
            this.ChangeValue(string.Join(Symbol, paramArray.Select(x => x.Value)));
            return;
        }

        this.ChangeValue(ValueStringFormatter.Format(paramArray.ToArray()));
    }
}

public class OneInt32ParamCronPickerRulerBoxItem : WithParamsCronPickerRulerBoxItem
{
    public static readonly StyledProperty<int> MinValueParamProperty = AvaloniaProperty.Register<OneInt32ParamCronPickerRulerBoxItem, int>(nameof(MinValueParam));
    public static readonly StyledProperty<int> MaxValueParamProperty = AvaloniaProperty.Register<OneInt32ParamCronPickerRulerBoxItem, int>(nameof(MaxValueParam));
    public static readonly StyledProperty<int> ParamValueProperty = AvaloniaProperty.Register<OneInt32ParamCronPickerRulerBoxItem, int>(nameof(ParamValue), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<string?> ParamNameProperty = AvaloniaProperty.Register<OneInt32ParamCronPickerRulerBoxItem, string?>(nameof(ParamName));
    public new static readonly StyledProperty<IAvaloniaStringFormatter?> ValueStringFormatterProperty = WithParamsCronPickerRulerBoxItem.ValueStringFormatterProperty;
    public static readonly StyledProperty<IAvaloniaStringFormatter?> ParamValueOutOfRangeMessageProperty = AvaloniaProperty.Register<OneInt32ParamCronPickerRulerBoxItem, IAvaloniaStringFormatter?>(nameof(ParamValueOutOfRangeMessage));
    public static readonly StyledProperty<IAvaloniaStringFormatter?> ParamParseErrorMessageProperty = AvaloniaProperty.Register<OneInt32ParamCronPickerRulerBoxItem, IAvaloniaStringFormatter?>(nameof(ParamParseErrorMessage));
    public static readonly StyledProperty<IAvaloniaStringFormatter?> ParamNullParseErrorMessageProperty = AvaloniaProperty.Register<OneInt32ParamCronPickerRulerBoxItem, IAvaloniaStringFormatter?>(nameof(ParamNullParseErrorMessage));

    public string? ParamName
    {
        get => GetValue(ParamNameProperty);
        set => SetValue(ParamNameProperty, value);
    }

    public int ParamValue
    {
        get => GetValue(ParamValueProperty);
        set => SetValue(ParamValueProperty, value);
    }

    public int MinValueParam
    {
        get => GetValue(MinValueParamProperty);
        set => SetValue(MinValueParamProperty, value);
    }

    public int MaxValueParam
    {
        get => GetValue(MaxValueParamProperty);
        set => SetValue(MaxValueParamProperty, value);
    }

    /// <summary>
    /// 值格式化器。
    /// </summary>
    /// <remarks>
    /// 用于格式化值。
    /// 提供参数：
    /// - param: 参数值。
    /// - symbol: 符号。
    /// </remarks>
    public override IAvaloniaStringFormatter? ValueStringFormatter
    {
        get => GetValue(ValueStringFormatterProperty);
        set => SetValue(ValueStringFormatterProperty, value);
    }

    /// <summary>
    /// 参数值越界时的消息。
    /// </summary>
    /// <remarks>
    /// 提供参数(顺序：value, min, max)：
    /// - value: 当前值。
    /// - min: 最小值。
    /// - max: 最大值。
    /// </remarks>
    public IAvaloniaStringFormatter? ParamValueOutOfRangeMessage
    {
        get => GetValue(ParamValueOutOfRangeMessageProperty);
        set => SetValue(ParamValueOutOfRangeMessageProperty, value);
    }
    
    /// <summary>
    /// 参数解析错误时的消息。
    /// </summary>
    /// <remarks>
    /// 提供参数(顺序：source)：
    /// - source: 原始值。
    /// </remarks>
    public IAvaloniaStringFormatter? ParamParseErrorMessage
    {
        get => GetValue(ParamParseErrorMessageProperty);
        set => SetValue(ParamParseErrorMessageProperty, value);
    }
    
    /// <summary>
    /// 参数为空时的消息。
    /// </summary>
    public IAvaloniaStringFormatter? ParamNullParseErrorMessage
    {
        get => GetValue(ParamNullParseErrorMessageProperty);
        set => SetValue(ParamNullParseErrorMessageProperty, value);
    }
    
    public override void VerifyCurrentCronValue()
    {
        if (ParamValue < MinValueParam || ParamValue > MaxValueParam)
        {
            throw new Exception(ParamValueOutOfRangeMessage?.Format(("value", ParamValue), ("min", MinValueParam), ("max", MaxValueParam), ("param", ParamName)));
        }
    }

    protected override CronParseResult ParseTo(string[] args)
    {
        if (args.Length != 1)
        {
            return CronParseResult.FormatError(ParamNullParseErrorMessage?.Format(("param", ParamName)));
        }

        if (!int.TryParse(args[0], out int param))
        {
            return CronParseResult.FormatError(ParamParseErrorMessage?.Format(("source", args[0]), ("param", ParamName)));
        }

        if (param < MinValueParam || param > MaxValueParam)
        {
            return CronParseResult.FormatError(ParamValueOutOfRangeMessage?.Format(("value", param), ("min", MinValueParam), ("max", MaxValueParam), ("param", ParamName)));
        }

        ParamValue = param;
        return CronParseResult.Success();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ParamValueProperty)
        {
            this.RefreshCurrentValue();
        }
        else if (change.Property == MinValueParamProperty || change.Property == MaxValueParamProperty)
        {
            this.ParamValue = Math.Min(MinValueParam, MaxValueParam);
        }
    }

    protected override void RefreshCurrentValue() => this.RefreshValue(("param", ParamValue), ("symbol", Symbol));
}

public class TwoInt32ParamsCronPickerRulerBoxItem : WithParamsCronPickerRulerBoxItem
{
    public static readonly StyledProperty<int> MinValueParam1Property = AvaloniaProperty.Register<TwoInt32ParamsCronPickerRulerBoxItem, int>(nameof(ValueParam1));
    public static readonly StyledProperty<int> MaxValueParam1Property = AvaloniaProperty.Register<TwoInt32ParamsCronPickerRulerBoxItem, int>(nameof(ValueParam1));
    public static readonly StyledProperty<int> MinValueParam2Property = AvaloniaProperty.Register<TwoInt32ParamsCronPickerRulerBoxItem, int>(nameof(ValueParam2));
    public static readonly StyledProperty<int> MaxValueParam2Property = AvaloniaProperty.Register<TwoInt32ParamsCronPickerRulerBoxItem, int>(nameof(ValueParam2));
    public static readonly StyledProperty<int> Param1ValueProperty = AvaloniaProperty.Register<TwoInt32ParamsCronPickerRulerBoxItem, int>(nameof(Param1Value), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<int> Param2ValueProperty = AvaloniaProperty.Register<TwoInt32ParamsCronPickerRulerBoxItem, int>(nameof(Param2Value), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<bool> IsParamsComparableProperty = AvaloniaProperty.Register<TwoInt32ParamsCronPickerRulerBoxItem, bool>(nameof(IsParamsComparable));
    public static readonly StyledProperty<int> LegalOffsetValueProperty = AvaloniaProperty.Register<TwoInt32ParamsCronPickerRulerBoxItem, int>(nameof(LegalOffsetValue));
    public static readonly StyledProperty<string?> Param1NameProperty = AvaloniaProperty.Register<TwoInt32ParamsCronPickerRulerBoxItem, string?>(nameof(Param1Name));
    public static readonly StyledProperty<string?> Param2NameProperty = AvaloniaProperty.Register<TwoInt32ParamsCronPickerRulerBoxItem, string?>(nameof(Param2Name));
    public new static readonly StyledProperty<IAvaloniaStringFormatter?> ValueStringFormatterProperty = WithParamsCronPickerRulerBoxItem.ValueStringFormatterProperty;
    public static readonly StyledProperty<IAvaloniaStringFormatter?> Param1ValueOutOfRangeMessageProperty = AvaloniaProperty.Register<TwoInt32ParamsCronPickerRulerBoxItem, IAvaloniaStringFormatter?>(nameof(Param1ValueOutOfRangeMessage));
    public static readonly StyledProperty<IAvaloniaStringFormatter?> Param2ValueOutOfRangeMessageProperty = AvaloniaProperty.Register<TwoInt32ParamsCronPickerRulerBoxItem, IAvaloniaStringFormatter?>(nameof(Param2ValueOutOfRangeMessage));
    public static readonly StyledProperty<IAvaloniaStringFormatter?> Param1ParseErrorMessageProperty = AvaloniaProperty.Register<TwoInt32ParamsCronPickerRulerBoxItem, IAvaloniaStringFormatter?>(nameof(Param1ParseErrorMessage));
    public static readonly StyledProperty<IAvaloniaStringFormatter?> Param2ParseErrorMessageProperty = AvaloniaProperty.Register<TwoInt32ParamsCronPickerRulerBoxItem, IAvaloniaStringFormatter?>(nameof(Param2ParseErrorMessage));
    public static readonly StyledProperty<IAvaloniaStringFormatter?> Param2NullParseErrorMessageProperty = AvaloniaProperty.Register<TwoInt32ParamsCronPickerRulerBoxItem, IAvaloniaStringFormatter?>(nameof(Param2NullParseErrorMessage));
    public static readonly StyledProperty<IAvaloniaStringFormatter?> ParamsNullParseErrorMessageProperty = AvaloniaProperty.Register<TwoInt32ParamsCronPickerRulerBoxItem, IAvaloniaStringFormatter?>(nameof(ParamsNullParseErrorMessage));
    public static readonly StyledProperty<IAvaloniaStringFormatter?> ParamsOutOfRangeMessageProperty = AvaloniaProperty.Register<TwoInt32ParamsCronPickerRulerBoxItem, IAvaloniaStringFormatter?>(nameof(ParamsOutOfRangeMessage));
    public static readonly StyledProperty<IAvaloniaStringFormatter?> OffsetOutOfRangeMessageProperty = AvaloniaProperty.Register<TwoInt32ParamsCronPickerRulerBoxItem, IAvaloniaStringFormatter?>(nameof(OffsetOutOfRangeMessage));
    
    public int ValueParam1
    {
        get => GetValue(MinValueParam1Property);
        set => SetValue(MinValueParam1Property, value);
    }
    
    public int ValueParam2
    {
        get => GetValue(MinValueParam2Property);
        set => SetValue(MinValueParam2Property, value);
    }
    
    public int Param1Value
    {
        get => GetValue(Param1ValueProperty);
        set => SetValue(Param1ValueProperty, value);
    }
    
    public int Param2Value
    {
        get => GetValue(Param2ValueProperty);
        set => SetValue(Param2ValueProperty, value);
    }

    public int MinValueParam1
    {
        get => GetValue(MinValueParam1Property);
        set => SetValue(MinValueParam1Property, value);
    }

    public int MaxValueParam1
    {
        get => GetValue(MaxValueParam1Property);
        set => SetValue(MaxValueParam1Property, value);
    }

    public int MinValueParam2
    {
        get => GetValue(MinValueParam2Property);
        set => SetValue(MinValueParam2Property, value);
    }

    public int MaxValueParam2
    {
        get => GetValue(MaxValueParam2Property);
        set => SetValue(MaxValueParam2Property, value);
    }

    // 是否参数值可比较，用于判断是否可进行偏移操作，如：param1 < param2 && param2 - param1 >= 1 && param1 + 1 <= param2
    public bool IsParamsComparable
    {
        get => GetValue(IsParamsComparableProperty);
        set => SetValue(IsParamsComparableProperty, value);
    }

    /// <summary>
    /// <see cref="Param1Value"/> 相较于 <see cref="Param2Value"/> 的合法偏移值(取值：param2 - param1)
    /// </summary>
    public int LegalOffsetValue
    {
        get => GetValue(LegalOffsetValueProperty);
        set => SetValue(LegalOffsetValueProperty, value);
    }

    public string? Param1Name
    {
        get => GetValue(Param1NameProperty);
        set => SetValue(Param1NameProperty, value);
    }

    public string? Param2Name
    {
        get => GetValue(Param2NameProperty);
        set => SetValue(Param2NameProperty, value);
    }

    /// <summary>
    /// 值格式化器。
    /// </summary>
    /// <remarks>
    /// 用于格式化值。
    /// 提供参数：
    /// - param1: 第一个参数值。
    /// - param2: 第二个参数值。
    /// - symbol: 符号。
    /// </remarks>
    public override IAvaloniaStringFormatter? ValueStringFormatter
    {
        get => GetValue(ValueStringFormatterProperty);
        set => SetValue(ValueStringFormatterProperty, value);
    }

    /// <summary>
    /// 参数1值越界时的消息。
    /// </summary>
    /// <remarks>
    /// 提供参数(顺序：value, min, max, param)：
    /// - value: 当前值。
    /// - min: 最小值。
    /// - max: 最大值。
    /// - param: 第一个参数名称。
    /// </remarks>
    public IAvaloniaStringFormatter? Param1ValueOutOfRangeMessage
    {
        get => GetValue(Param1ValueOutOfRangeMessageProperty);
        set => SetValue(Param1ValueOutOfRangeMessageProperty, value);
    }
    
    /// <summary>
    /// 参数2值越界时的消息。
    /// </summary>
    /// <remarks>
    /// 提供参数(顺序：value, min, max, param)：
    /// - value: 当前值。
    /// - min: 最小值。
    /// - max: 最大值。
    /// - param: 第二个参数名称。
    /// </remarks>
    public IAvaloniaStringFormatter? Param2ValueOutOfRangeMessage
    {
        get => GetValue(Param2ValueOutOfRangeMessageProperty);
        set => SetValue(Param2ValueOutOfRangeMessageProperty, value);
    }
    
    /// <summary>
    /// 参数1解析错误时的消息。
    /// </summary>
    /// <remarks>
    /// 提供参数(顺序：source, param)：
    /// - source: 原始值。
    /// - param: 第一个参数名称。
    /// </remarks>
    public IAvaloniaStringFormatter? Param1ParseErrorMessage
    {
        get => GetValue(Param1ParseErrorMessageProperty);
        set => SetValue(Param1ParseErrorMessageProperty, value);
    }
    
    /// <summary>
    /// 参数2解析错误时的消息。
    /// </summary>
    /// <remarks>
    /// 提供参数(顺序：source, param)：
    /// - source: 原始值。
    /// - param: 第二个参数名称。
    /// </remarks>
    public IAvaloniaStringFormatter? Param2ParseErrorMessage
    {
        get => GetValue(Param2ParseErrorMessageProperty);
        set => SetValue(Param2ParseErrorMessageProperty, value);
    }
    
    /// <summary>
    /// 参数2为空时的消息。
    /// </summary>
    /// <remarks>
    /// 提供参数(顺序：param)：
    /// - param: 第二个参数名称。
    /// </remarks>
    public IAvaloniaStringFormatter? Param2NullParseErrorMessage
    {
        get => GetValue(Param2NullParseErrorMessageProperty);
        set => SetValue(Param2NullParseErrorMessageProperty, value);
    }
    
    /// <summary>
    /// 参数为空时的消息。
    /// </summary>
    /// <remarks>
    /// 提供参数(顺序：param0, param1)：
    /// - param0: 第一个参数名称。
    /// - param1: 第二个参数名称。
    /// </remarks>
    public IAvaloniaStringFormatter? ParamsNullParseErrorMessage
    {
        get => GetValue(ParamsNullParseErrorMessageProperty);
        set => SetValue(ParamsNullParseErrorMessageProperty, value);
    }
    
    /// <summary>
    /// 允许的参数个数超出范围时的消息。
    /// </summary>
    public IAvaloniaStringFormatter? ParamsOutOfRangeMessage
    {
        get => GetValue(ParamsOutOfRangeMessageProperty);
        set => SetValue(ParamsOutOfRangeMessageProperty, value);
    }
    
    /// <summary>
    /// 偏移值越界时的消息。
    /// </summary>
    /// <remarks>
    /// 提供参数(顺序：offset, min, param1, param2, param1Value, param2Value)：
    /// - offset: 偏移值。
    /// - min: 最小偏移值。
    /// - param1: 第一个参数名称。
    /// - param2: 第二个参数名称。
    /// - param1Value: 第一个参数值。
    /// - param2Value: 第二个参数值。
    /// </remarks>
    public IAvaloniaStringFormatter? OffsetOutOfRangeMessage
    {
        get => GetValue(OffsetOutOfRangeMessageProperty);
        set => SetValue(OffsetOutOfRangeMessageProperty, value);
    }

    protected override string[] ReadParamsFrom(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Array.Empty<string>();
        }

        int index = text.LastIndexOf(Symbol);
        return new[] { text.Substring(0, index).Trim(), text.Substring(index + 1).Trim() };
    }

    protected override CronParseResult ParseTo(string[] args)
    {
        if (args.Length != 2)
        {
            if (args.Length == 0)
            {
                return CronParseResult.FormatError(ParamsNullParseErrorMessage?.Format(("param0", Param1Name), ("param1", Param2Name)));
            }

            if (args.Length == 1)
            {
                return CronParseResult.FormatError(Param2NullParseErrorMessage?.Format(("param", Param2Name)));
            }
            
            return CronParseResult.FormatError(ParamsOutOfRangeMessage?.Format(("param0", Param1Name), ("param1", Param2Name)));
        }

        if (!int.TryParse(args[0], out int param1))
        {
            return CronParseResult.FormatError(Param1ParseErrorMessage?.Format(("source", args[0]), ("param", Param1Name)));
        }

        if (!int.TryParse(args[1], out int param2))
        {
            return CronParseResult.FormatError(Param2ParseErrorMessage?.Format(("source", args[1]), ("param", Param2Name)));
        }

        if (param1 < MinValueParam1 || param1 > MaxValueParam1)
        {
            throw new Exception(Param1ValueOutOfRangeMessage?.Format(("value", param1), ("min", MinValueParam1), ("max", MaxValueParam1), ("param", Param1Name)));
        }

        if (param2 < MinValueParam2 || param2 > MaxValueParam2)
        {
            throw new Exception(Param2ValueOutOfRangeMessage?.Format(("value", param2), ("min", MinValueParam2), ("max", MaxValueParam2), ("param", Param2Name)));
        }

        if (IsParamsComparable)
        {
            int offset = param2 - param1;
            if (offset < LegalOffsetValue)
            {
                throw new Exception(OffsetOutOfRangeMessage?.Format(("offset", offset), ("min", LegalOffsetValue), ("param1", Param1Name), ("param2", Param2Name), ("param1Value", param1), ("param2Value", param2)));
            }
        }
        
        Param1Value = param1;
        Param2Value = param2;
        return CronParseResult.Success();
    }

    public override void VerifyCurrentCronValue()
    {
        if (Param1Value < MinValueParam1 || Param1Value > MaxValueParam1)
        {
            throw new Exception(Param1ValueOutOfRangeMessage?.Format(("value", Param1Value), ("min", MinValueParam1), ("max", MaxValueParam1), ("param", Param1Name)));
        }
        
        if (Param2Value < MinValueParam2 || Param2Value > MaxValueParam2)
        {
            throw new Exception(Param2ValueOutOfRangeMessage?.Format(("value", Param2Value), ("min", MinValueParam2), ("max", MaxValueParam2), ("param", Param2Name)));
        }

        if (IsParamsComparable)
        {
            int offset = Param2Value - Param1Value;
            if (offset < LegalOffsetValue)
            {
                throw new Exception(OffsetOutOfRangeMessage?.Format(("offset", offset), ("min", LegalOffsetValue), ("param1", Param1Name), ("param2", Param2Name), ("param1Value", Param1Value), ("param2Value", Param2Value)));
            }
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == Param1ValueProperty || change.Property == Param2ValueProperty)
        {
            this.RefreshCurrentValue();
        }
        else if (change.Property == MinValueParam1Property || change.Property == MaxValueParam1Property)
        {
            this.Param1Value = Math.Min(MinValueParam1, MaxValueParam1);
            InnerRefreshParam2Value();
        }
        else if (change.Property == MinValueParam2Property || change.Property == MaxValueParam2Property)
        {
            InnerRefreshParam2Value();
        }
        else if (change.Property == LegalOffsetValueProperty)
        {
            InnerRefreshParam2Value();
        }
    }
    
    protected override void RefreshCurrentValue() => this.RefreshValue(("param1", Param1Value), ("param2", Param2Value), ("symbol", Symbol));

    private void InnerRefreshParam2Value()
    {
        int param2 = Param1Value + LegalOffsetValue;
        int minValue = Math.Min(MinValueParam2, MaxValueParam2);
        if (param2 >= minValue && param2 <= MaxValueParam2)
        {
            Param2Value = param2;
        }
        else
        {
            this.Param2Value = minValue;
        }
    }
}

public class Int32SetCronPickerRulerBoxItem : WithParamsCronPickerRulerBoxItem
{
    public static readonly StyledProperty<IEnumerable<int>> ItemsSourceProperty = AvaloniaProperty.Register<Int32SetCronPickerRulerBoxItem, IEnumerable<int>>(nameof(ItemsSource));
    public static readonly DirectProperty<Int32SetCronPickerRulerBoxItem, IList> SelectedItemsProperty = AvaloniaProperty.RegisterDirect<Int32SetCronPickerRulerBoxItem, IList>(nameof(SelectedItems), o => o.SelectedItems);
    public static readonly StyledProperty<string> DefaultEmptySelectedValueProperty = AvaloniaProperty.Register<Int32SetCronPickerRulerBoxItem, string>(nameof(DefaultEmptySelectedValue));
    public static readonly StyledProperty<string> DefaultSelectedAllValueProperty = AvaloniaProperty.Register<Int32SetCronPickerRulerBoxItem, string>(nameof(DefaultSelectedAllValue));
    public static readonly StyledProperty<ITemplate<Panel?>> ItemsPanelProperty = AvaloniaProperty.Register<Int32SetCronPickerRulerBoxItem, ITemplate<Panel?>>(nameof(ItemsPanel));
    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty = AvaloniaProperty.Register<Int32SetCronPickerRulerBoxItem, IDataTemplate?>(nameof(ItemTemplate));
    public static readonly StyledProperty<IAvaloniaStringFormatter?> ParamNullParseErrorMessageProperty = AvaloniaProperty.Register<Int32SetCronPickerRulerBoxItem, IAvaloniaStringFormatter?>(nameof(ParamNullParseErrorMessage));
    public static readonly StyledProperty<IAvaloniaStringFormatter?> HasElementParseErrorMessageProperty = AvaloniaProperty.Register<Int32SetCronPickerRulerBoxItem, IAvaloniaStringFormatter?>(nameof(HasElementParseErrorMessage));
    public static readonly StyledProperty<IAvaloniaStringFormatter?> HasElementOutOfRangeMessageProperty = AvaloniaProperty.Register<Int32SetCronPickerRulerBoxItem, IAvaloniaStringFormatter?>(nameof(HasElementOutOfRangeMessage));
    public static readonly StyledProperty<double?> ItemHeightProperty = AvaloniaProperty.Register<Int32SetCronPickerRulerBoxItem, double?>(nameof(ItemHeight));
    public static readonly StyledProperty<double?> ItemWidthProperty = AvaloniaProperty.Register<Int32SetCronPickerRulerBoxItem, double?>(nameof(ItemWidth));
    public static readonly StyledProperty<Thickness> ItemPaddingProperty = AvaloniaProperty.Register<Int32SetCronPickerRulerBoxItem, Thickness>(nameof(ItemPadding));
    public static readonly StyledProperty<double> ItemsPanelRowSpacingProperty = AvaloniaProperty.Register<Int32SetCronPickerRulerBoxItem, double>(nameof(ItemsPanelRowSpacing), 0);
    public static readonly StyledProperty<double> ItemsPanelColumnSpacingProperty = AvaloniaProperty.Register<Int32SetCronPickerRulerBoxItem, double>(nameof(ItemsPanelColumnSpacing), 0);

    private readonly AvaloniaList<int> _currSelectedItems;

    public Int32SetCronPickerRulerBoxItem()
    {
        _currSelectedItems = new();
        WeakReference<Int32SetCronPickerRulerBoxItem> weakSelf = new(this);
        _currSelectedItems.CollectionChanged += (sender, args) =>
        {
            if (weakSelf.TryGetTarget(out Int32SetCronPickerRulerBoxItem? owner))
            {
                owner?.RefreshCurrentValue();
            }
        };
    }
    
    /// <summary>
    /// 单个元素的高度。
    /// </summary>
    public double? ItemHeight
    {
        get => GetValue(ItemHeightProperty);
        set => SetValue(ItemHeightProperty, value);
    }
    
    /// <summary>
    /// 单个元素的宽度。
    /// </summary>
    public double? ItemWidth
    {
        get => GetValue(ItemWidthProperty);
        set => SetValue(ItemWidthProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the spacing between rows.
    /// </summary>
    public double ItemsPanelRowSpacing
    {
        get => GetValue(ItemsPanelRowSpacingProperty);
        set => SetValue(ItemsPanelRowSpacingProperty, value);
    }

    /// <summary>
    /// Gets or sets the spacing between columns.
    /// </summary>
    public double ItemsPanelColumnSpacing
    {
        get => GetValue(ItemsPanelColumnSpacingProperty);
        set => SetValue(ItemsPanelColumnSpacingProperty, value);
    }

    public Thickness ItemPadding
    {
        get => GetValue(ItemPaddingProperty);
        set => SetValue(ItemPaddingProperty, value);
    }

    public string DefaultEmptySelectedValue
    {
        get => GetValue(DefaultEmptySelectedValueProperty);
        set => SetValue(DefaultEmptySelectedValueProperty, value);
    }

    public string DefaultSelectedAllValue
    {
        get => GetValue(DefaultSelectedAllValueProperty);
        set => SetValue(DefaultSelectedAllValueProperty, value);
    }

    public IEnumerable<int> ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }
    
    public IList SelectedItems
    {
        get => _currSelectedItems;
        // set => SetValue(SelectedItemsProperty, value);
    }
    
    public ITemplate<Panel?> ItemsPanel
    {
        get => GetValue(ItemsPanelProperty);
        set => SetValue(ItemsPanelProperty, value);
    }
    
    [InheritDataTypeFromItems(nameof(ItemsSource))]
    public IDataTemplate? ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    public IAvaloniaStringFormatter? ParamNullParseErrorMessage
    {
        get => GetValue(ParamNullParseErrorMessageProperty);
        set => SetValue(ParamNullParseErrorMessageProperty, value);
    }

    /// <summary>
    /// 存在元素无法解析时的消息。
    /// </summary>
    /// <remarks>
    /// 提供参数(顺序：source, index)：
    /// - source: 原始值。
    /// - index: 索引。
    /// </remarks>
    public IAvaloniaStringFormatter? HasElementParseErrorMessage
    {
        get => GetValue(HasElementParseErrorMessageProperty);
        set => SetValue(HasElementParseErrorMessageProperty, value);
    }

    /// <summary>
    /// 存在元素越界时的消息。
    /// </summary>
    /// <remarks>
    /// 提供参数(顺序：source, index)：
    /// - source: 原始值。
    /// - index: 索引。
    /// </remarks>
    public IAvaloniaStringFormatter? HasElementOutOfRangeMessage
    {
        get => GetValue(HasElementOutOfRangeMessageProperty);
        set => SetValue(HasElementOutOfRangeMessageProperty, value);
    }

    protected override bool IsSymbolAlwaysContain { get; } = false;

    protected override CronParseResult ParseTo(string[] args)
    {
        if (args.Length < 0)
        {
            return CronParseResult.FormatError(ParamNullParseErrorMessage?.Format());
        }

        if (args.Length == 1)
        {
            if (args[0] == DefaultSelectedAllValue)
            {
                if (DefaultEmptySelectedValue == DefaultSelectedAllValue && SelectedItems.Count == 0)
                {
                    return CronParseResult.Success();
                }

                int[] arr = SelectedItems.OfType<int>().ToArray();
                if (arr.Length == ItemsSource.Count() && arr.All(x => ItemsSource.Contains(x)))
                {
                    return CronParseResult.Success();
                }

                foreach (int item in ItemsSource)
                {
                    if (!SelectedItems.Contains(item))
                    {
                        SelectedItems.Add(item);
                    }
                }

                return CronParseResult.Success();
            }

            if (args[0] == DefaultEmptySelectedValue)
            {
                SelectedItems.Clear();
                return CronParseResult.Success();
            }
        }

        for (var index = 0; index < args.Length; index++)
        {
            string arg = args[index];
            if (!int.TryParse(arg, out int param))
            {
                return CronParseResult.FormatError(HasElementParseErrorMessage?.Format(("source", arg), ("index", index)));
            }

            if (!ItemsSource.Contains(param))
            {
                return CronParseResult.FormatError(HasElementOutOfRangeMessage?.Format(("source", arg), ("index", index)));
            }

            if (!SelectedItems.Contains(param))
            {
                SelectedItems.Add(param);
            }
        }

        return CronParseResult.Success();
    }

    public override void VerifyCurrentCronValue()
    {
        if (Value == DefaultEmptySelectedValue || Value == DefaultSelectedAllValue)
        {
            return;
        }
        
        if (SelectedItems.OfType<int>().Any(x => !ItemsSource.Contains(x)))
        {
            throw new Exception(HasElementOutOfRangeMessage?.Format(("source", Value), ("index", -1)));
        }
    }

    protected override void RefreshCurrentValue()
    {
        int[]? arr = SelectedItems?.OfType<int>().ToArray();
        if (arr is null || arr.Length == 0)
        {
            this.ChangeValue(DefaultEmptySelectedValue);
        }
        else
        {
            if (ItemsSource is null)
            {
                return;
            }
            
            int[] source = ItemsSource.ToArray();
            if (source.All(x => arr.Contains(x)))
            {
                this.ChangeValue(DefaultSelectedAllValue);
            }
            else
            {
                this.ChangeValue(string.Join(this.Symbol, arr));
            }
        }
    }
}
