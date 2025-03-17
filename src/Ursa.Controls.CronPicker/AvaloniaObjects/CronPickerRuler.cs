using System.Collections;
using Avalonia;
using Avalonia.Controls;

namespace Ursa.Controls;

/// <summary>
/// Cron 规则内容基类
/// </summary>
public abstract class CronPickerRuler : AvaloniaObject
{
    public static readonly StyledProperty<int> CodeProperty = AvaloniaProperty.Register<CronPickerRuler, int>(nameof(Code));
    public static readonly StyledProperty<string> SymbolProperty = AvaloniaProperty.Register<CronPickerRuler, string>(nameof(Symbol));
    public static readonly DirectProperty<CronPickerRuler, CronFieldTypes> FieldTypeProperty = AvaloniaProperty.RegisterDirect<CronPickerRuler, CronFieldTypes>(nameof(FieldType), o => o.FieldType);
    public static readonly StyledProperty<Dock> HeaderPlacementProperty = AvaloniaProperty.Register<CronPickerRuler, Dock>(nameof(HeaderPlacement), defaultValue: Dock.Left);
    public static readonly StyledProperty<int> PriorityProperty = AvaloniaProperty.Register<CronPickerRuler, int>(nameof(Priority));
    private CronFieldTypes _fieldType;

    /// <summary>
    /// 所属规则类型
    /// </summary>
    public CronFieldTypes FieldType => _fieldType;

    /// <summary>
    /// 用作类型标识
    /// </summary>
    public int Code
    {
        get => GetValue(CodeProperty);
        set => SetValue(CodeProperty, value);
    }

    /// <summary>
    /// 符号(用作表达式构建标识符) 
    /// </summary>
    public string Symbol
    {
        get => GetValue(SymbolProperty);
        set => SetValue(SymbolProperty, value);
    }

    public Dock HeaderPlacement
    {
        get => GetValue(HeaderPlacementProperty);
        set => SetValue(HeaderPlacementProperty, value);
    }

    /// <summary>
    /// 优先级, 用于排序解析，值越小越优先
    /// </summary>
    public int Priority
    {
        get => GetValue(PriorityProperty);
        set => SetValue(PriorityProperty, value);
    }

    internal void SetFieldType(CronFieldTypes fieldType)
    {
        this.SetAndRaise(FieldTypeProperty, ref _fieldType, fieldType);
    }
}

/// <summary>
/// 只包含简单符号的规则表达式
/// </summary>
public sealed class CronPickerRulerSimpleSymbol : CronPickerRuler
{
    public static readonly StyledProperty<bool> IsNullAllowedProperty = AvaloniaProperty.Register<CronPickerRulerSimpleSymbol, bool>(nameof(IsNullAllowed));

    public bool IsNullAllowed
    {
        get => GetValue(IsNullAllowedProperty);
        set => SetValue(IsNullAllowedProperty, value);
    }
}

/// <summary>
/// 范围值规则表达式
/// </summary>
public class CronPickerRulerRangeExpression : CronPickerRulerFormatExpression
{
    public static readonly StyledProperty<int> MinStartValueProperty = AvaloniaProperty.Register<CronPickerRulerRangeExpression, int>(nameof(MinStartValue));
    public static readonly StyledProperty<int> MaxStartValueProperty = AvaloniaProperty.Register<CronPickerRulerRangeExpression, int>(nameof(MaxStartValue));
    public static readonly StyledProperty<int> MinEndValueProperty = AvaloniaProperty.Register<CronPickerRulerRangeExpression, int>(nameof(MinEndValue));
    public static readonly StyledProperty<int> MaxEndValueProperty = AvaloniaProperty.Register<CronPickerRulerRangeExpression, int>(nameof(MaxEndValue));
    public static readonly StyledProperty<string> ParamName_StartProperty = AvaloniaProperty.Register<CronPickerRulerRangeExpression, string>(nameof(ParamName_Start));
    public static readonly StyledProperty<string> ParamName_EndProperty = AvaloniaProperty.Register<CronPickerRulerRangeExpression, string>(nameof(ParamName_End));
    
    /// <summary>
    /// 参数名称_开始 
    /// </summary>
    public string ParamName_Start
    {
        get => GetValue(ParamName_StartProperty);
        set => SetValue(ParamName_StartProperty, value);
    }

    /// <summary>
    /// 参数名称_结束
    /// </summary>
    public string ParamName_End
    {
        get => GetValue(ParamName_EndProperty);
        set => SetValue(ParamName_EndProperty, value);
    }
    
    public int MinStartValue
    {
        get => GetValue(MinStartValueProperty);
        set => SetValue(MinStartValueProperty, value);
    }

    public int MaxStartValue
    {
        get => GetValue(MaxStartValueProperty);
        set => SetValue(MaxStartValueProperty, value);
    }

    public int MinEndValue
    {
        get => GetValue(MinEndValueProperty);
        set => SetValue(MinEndValueProperty, value);
    }

    public int MaxEndValue
    {
        get => GetValue(MaxEndValueProperty);
        set => SetValue(MaxEndValueProperty, value);
    }
    
    public string Combine(int start, int end) => Symbol;

    public bool Parse(string expression)
    {
        if (expression == Symbol)
        {
            return true;
        }
        
        return false;
    }
}

public class CronPickerRulerIntervalExpression : CronPickerRulerFormatExpression
{
    public static readonly StyledProperty<int> MinStartValueProperty = AvaloniaProperty.Register<CronPickerRulerRangeExpression, int>(nameof(MinStartValue));
    public static readonly StyledProperty<int> MaxStartValueProperty = AvaloniaProperty.Register<CronPickerRulerRangeExpression, int>(nameof(MaxStartValue));
    public static readonly StyledProperty<int> MinIntervalProperty = AvaloniaProperty.Register<CronPickerRulerRangeExpression, int>(nameof(MinInterval));
    public static readonly StyledProperty<int> MaxIntervalProperty = AvaloniaProperty.Register<CronPickerRulerRangeExpression, int>(nameof(MaxInterval));
    public static readonly StyledProperty<string> ParamName_StartProperty = AvaloniaProperty.Register<CronPickerRulerRangeExpression, string>(nameof(ParamName_Start));
    public static readonly StyledProperty<string> ParamName_IntervalProperty = AvaloniaProperty.Register<CronPickerRulerRangeExpression, string>(nameof(ParamName_Interval));
    
    /// <summary>
    /// 参数名称_开始
    /// </summary>
    public string ParamName_Start
    {
        get => GetValue(ParamName_StartProperty);
        set => SetValue(ParamName_StartProperty, value);
    }

    /// <summary>
    /// 参数名称_间隔
    /// </summary>
    public string ParamName_Interval
    {
        get => GetValue(ParamName_IntervalProperty);
        set => SetValue(ParamName_IntervalProperty, value);
    }

    public int MinStartValue
    {
        get => GetValue(MinStartValueProperty);
        set => SetValue(MinStartValueProperty, value);
    }

    public int MaxStartValue
    {
        get => GetValue(MaxStartValueProperty);
        set => SetValue(MaxStartValueProperty, value);
    }

    public int MinInterval
    {
        get => GetValue(MinIntervalProperty);
        set => SetValue(MinIntervalProperty, value);
    }

    public int MaxInterval
    {
        get => GetValue(MaxIntervalProperty);
        set => SetValue(MaxIntervalProperty, value);
    }
}

public class CronPickerRulerSpecialWeekdayExpression : CronPickerRulerFormatExpression
{
    public static readonly StyledProperty<int> MinWeekNoProperty = AvaloniaProperty.Register<CronPickerRulerRangeExpression, int>(nameof(MinWeekNo));
    public static readonly StyledProperty<int> MaxWeekNoProperty = AvaloniaProperty.Register<CronPickerRulerRangeExpression, int>(nameof(MaxWeekNo));
    public static readonly StyledProperty<int> MinWeekDayProperty = AvaloniaProperty.Register<CronPickerRulerRangeExpression, int>(nameof(MinWeekDay));
    public static readonly StyledProperty<int> MaxWeekDayProperty = AvaloniaProperty.Register<CronPickerRulerRangeExpression, int>(nameof(MaxWeekDay));
    public static readonly StyledProperty<string> ParamName_WeekNoProperty = AvaloniaProperty.Register<CronPickerRulerRangeExpression, string>(nameof(ParamName_WeekNo));
    public static readonly StyledProperty<string> ParamName_WeekDayProperty = AvaloniaProperty.Register<CronPickerRulerRangeExpression, string>(nameof(ParamName_WeekDay));
    
    /// <summary>
    /// 参数名称_周序号
    /// </summary>
    public string ParamName_WeekNo
    {
        get => GetValue(ParamName_WeekNoProperty);
        set => SetValue(ParamName_WeekNoProperty, value);
    }

    /// <summary>
    /// 参数名称_周天
    /// </summary>
    public string ParamName_WeekDay
    {
        get => GetValue(ParamName_WeekDayProperty);
        set => SetValue(ParamName_WeekDayProperty, value);
    }

    public int MinWeekNo
    {
        get => GetValue(MinWeekNoProperty);
        set => SetValue(MinWeekNoProperty, value);
    }

    public int MaxWeekNo
    {
        get => GetValue(MaxWeekNoProperty);
        set => SetValue(MaxWeekNoProperty, value);
    }

    public int MinWeekDay
    {
        get => GetValue(MinWeekDayProperty);
        set => SetValue(MinWeekDayProperty, value);
    }

    public int MaxWeekDay
    {
        get => GetValue(MaxWeekDayProperty);
        set => SetValue(MaxWeekDayProperty, value);
    }
}

public class CronPickerRulerWithOneParameterExpression : CronPickerRulerFormatExpression
{
    public static readonly StyledProperty<int> MinValueProperty = AvaloniaProperty.Register<CronPickerRulerRangeExpression, int>(nameof(MinValue));
    public static readonly StyledProperty<int> MaxValueProperty = AvaloniaProperty.Register<CronPickerRulerRangeExpression, int>(nameof(MaxValue));
    public static readonly StyledProperty<string> ParamNameProperty = AvaloniaProperty.Register<CronPickerRulerRangeExpression, string>(nameof(ParamName));

    /// <summary>
    /// 参数名称
    /// </summary>
    public string ParamName
    {
        get => GetValue(ParamNameProperty);
        set => SetValue(ParamNameProperty, value);
    }

    public int MinValue
    {
        get => GetValue(MinValueProperty);
        set => SetValue(MinValueProperty, value);
    }

    public int MaxValue
    {
        get => GetValue(MaxValueProperty);
        set => SetValue(MaxValueProperty, value);
    }
}

public class CronPickerRulerListExpression : CronPickerRulerFormatExpression
{
    public static readonly StyledProperty<IEnumerable<int>> ItemsSourceProperty = AvaloniaProperty.Register<CronPickerRulerListExpression, IEnumerable<int>>(nameof(ItemsSource));
    public CronPickerRulerListExpression()
    {
        this.HeaderPlacement = Dock.Top;
        this.Priority = 1;
    }

    public IEnumerable<int> ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }
}

public class CronPickerRulerFormatExpression : CronPickerRuler
{
    public static readonly StyledProperty<IAvaloniaStringFormatter?> ValueStringFormatterProperty = AvaloniaProperty.Register<CronPickerRulerFormatExpression, IAvaloniaStringFormatter?>(nameof(ValueStringFormatter));
    
    public IAvaloniaStringFormatter? ValueStringFormatter
    {
        get => GetValue(ValueStringFormatterProperty);
        set => SetValue(ValueStringFormatterProperty, value);
    }


    public string Combine(IEnumerable<(string Key, object? Value)> values)
    {
        if (ValueStringFormatter == null)
        {
            return string.Join(Symbol, values.Select(x => x.Value).ToArray());
        }
        else
        {
            return ValueStringFormatter.Format(values.ToArray());
        }
    }

    public virtual bool Parse(string expression)
    {
        if (expression == Symbol)
        {
            return true;
        }
        
        return false;
    }
}

public class CronPickerRulerDateTimeRangeExpression : CronPickerRulerRangeExpression
{
    public CronPickerRulerDateTimeRangeExpression()
    {
        this.MinStartValue = DateTime.Now.Year;
        this.MinEndValue = this.MinStartValue;
        this.MaxStartValue = DateTime.MaxValue.Year;
        this.MaxEndValue = DateTime.MaxValue.Year;
    }
}