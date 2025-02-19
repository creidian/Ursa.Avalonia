using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls.Primitives;
using Avalonia.Data.Core;
using Avalonia.Interactivity;
using Ursa.Common;

namespace Ursa.Controls;

/// <summary>
/// A control that represents a single field in a cron expression.
/// </summary>
public abstract class CronPickerFieldItem : TemplatedControl
{
    /// <summary>
    /// * 表示匹配该域的任意值。假如在Minutes域使用*, 即表示每分钟都会触发事件。
    /// </summary>
    public const string FIELD_CHAR_ANY = "*";

    /// <summary>
    /// ? 只能用在DayofMonth和DayofWeek两个域。它也匹配域的任意值，但实际不会。因为DayofMonth和DayofWeek会相互影响。例如想在每月的20日触发调度，不管20日到底是星期几，则只能使用如下写法： 13 13 15 20 * ?, 其中最后一位只能用？，而不能使用*，如果使用*表示不管星期几都会触发，实际上并不是这样。
    /// </summary>
    public const string FIELD_CHAR_PLACEHOLDER = "?";

    /// <summary>
    /// - 表示范围。例如在Minutes域使用5-20，表示从5分到20分钟每分钟触发一次
    /// </summary>
    public const string FIELD_CHAR_RANGE = "-";

    /// <summary>
    /// / 表示起始时间开始触发，然后每隔固定时间触发一次。例如在Minutes域使用5/20,则意味着5分钟触发一次，而25，45等分别触发一次.
    /// </summary>
    public const string FIELD_CHAR_INTERVAL = "/";

    /// <summary>
    /// , 表示列出枚举值。例如：在Minutes域使用5,20，则意味着在5和20分每分钟触发一次。
    /// </summary>
    public const string FIELD_CHAR_LIST_AND = ",";

    /// <summary>
    /// L 表示最后，只能出现在DayofWeek和DayofMonth域。如果在DayofWeek域使用5L,意味着在最后的一个星期四触发。
    /// </summary>
    public const string FIELD_CHAR_LAST = "L";

    /// <summary>
    /// W 表示有效工作日(周一到周五),只能出现在DayofMonth域，系统将在离指定日期的最近的有效工作日触发事件。例如：在 DayofMonth使用5W，如果5日是星期六，则将在最近的工作日：星期五，即4日触发。如果5日是星期天，则在6日(周一)触发；如果5日在星期一到星期五中的一天，则就在5日触发。另外一点，W的最近寻找不会跨过月份 。
    /// </summary>
    public const string FIELD_CHAR_WORKDAY = "W";

    /// <summary>
    /// # 表示第几个星期几，只能出现在DayofWeek域。例如在DayofWeek域使用6#3，表示在每月的第三个星期五触发。
    /// </summary>
    public const string FIELD_CHAR_WEEKDAY = "#";

    public static readonly StyledProperty<string?> ValueProperty = AvaloniaProperty.Register<CronPickerFieldItem, string?>(nameof(Value));
    public static readonly StyledProperty<string?> HeaderProperty = AvaloniaProperty.Register<CronPickerFieldItem, string?>(nameof(Header));
    public static readonly StyledProperty<bool> IsAllowSpecialValueProperty = AvaloniaProperty.Register<CronPickerFieldItem, bool>(nameof(IsAllowSpecialValue));
    public static readonly StyledProperty<IAvaloniaStringFormatter?> NotSupportMatchTypeFormatterProperty = AvaloniaProperty.Register<CronPickerFieldItem, IAvaloniaStringFormatter?>(nameof(NotSupportMatchTypeFormatter)/*, defaultValue: AvaloniaStringFormatter.CreateSimpleStringFormatter("不支持解析的匹配类型: {0}")*/);
    public static readonly StyledProperty<IAvaloniaStringFormatter?> NotNumValueFormatterProperty = AvaloniaProperty.Register<CronPickerFieldItem, IAvaloniaStringFormatter?>(nameof(NotNumValueFormatter)/*, defaultValue: AvaloniaStringFormatter.CreateSimpleStringFormatter("非数值异常: {0}")*/);
    public static readonly StyledProperty<IAvaloniaStringFormatter?> ExpressionErrorFormatterProperty = AvaloniaProperty.Register<CronPickerFieldItem, IAvaloniaStringFormatter?>(nameof(ExpressionErrorFormatter)/*, defaultValue: AvaloniaStringFormatter.CreateSimpleStringFormatter("表达式异常: {0}")*/);
    public static readonly StyledProperty<IAvaloniaStringFormatter?> ExpressionNotSupportFormatterProperty = AvaloniaProperty.Register<CronPickerFieldItem, IAvaloniaStringFormatter?>(nameof(ExpressionNotSupportFormatter)/*, defaultValue: AvaloniaStringFormatter.CreateSimpleStringFormatter("未知规则表达式: {0}")*/);
    public static readonly StyledProperty<IAvaloniaStringFormatter?> RangeStartOrEndOutOfRangeFormatterProperty = AvaloniaProperty.Register<CronPickerFieldItem, IAvaloniaStringFormatter?>(nameof(RangeStartOrEndOutOfRangeFormatter)/*, defaultValue:AvaloniaStringFormatter.CreateSimpleStringFormatter("起始或结束值超出范围！")*/);
    public static readonly StyledProperty<IAvaloniaStringFormatter?> RangeEndLessThenStartFormatterProperty = AvaloniaProperty.Register<CronPickerFieldItem, IAvaloniaStringFormatter?>(nameof(RangeEndLessThenStartFormatter)/*, defaultValue:AvaloniaStringFormatter.CreateSimpleStringFormatter("截止年份不能小于起始年份！")*/);
    public static readonly StyledProperty<IAvaloniaStringFormatter?> EnumeratorEmptyFormatterProperty = AvaloniaProperty.Register<CronPickerFieldItem, IAvaloniaStringFormatter?>(nameof(EnumeratorEmptyFormatter)/*, defaultValue:AvaloniaStringFormatter.CreateSimpleStringFormatter("The list of values cannot be empty.")*/);
    public static readonly StyledProperty<IAvaloniaStringFormatter?> EnumeratorOutOfRangeFormatterProperty = AvaloniaProperty.Register<CronPickerFieldItem, IAvaloniaStringFormatter?>(nameof(EnumeratorOutOfRangeFormatter)/*, defaultValue:AvaloniaStringFormatter.CreateSimpleStringFormatter("The value must be between 1 and 7.")*/);
    public static readonly StyledProperty<IAvaloniaStringFormatter?> IntervalLeftOutOfRangeFormatterProperty = AvaloniaProperty.Register<CronPickerFieldItem, IAvaloniaStringFormatter?>(nameof(IntervalLeftOutOfRangeFormatter)/*, defaultValue:AvaloniaStringFormatter.CreateSimpleStringFormatter("The left value must be between 0 and 59.")*/);
    public static readonly StyledProperty<IAvaloniaStringFormatter?> IntervalRightOutOfRangeFormatterProperty = AvaloniaProperty.Register<CronPickerFieldItem, IAvaloniaStringFormatter?>(nameof(IntervalRightOutOfRangeFormatter)/*, defaultValue:AvaloniaStringFormatter.CreateSimpleStringFormatter("The right value must be between 0 and 59.")*/);
    public static readonly StyledProperty<IAvaloniaStringFormatter?> SharpLeftOutOfRangeFormatterProperty = AvaloniaProperty.Register<CronPickerFieldItem, IAvaloniaStringFormatter?>(nameof(SharpLeftOutOfRangeFormatter)/*, defaultValue:AvaloniaStringFormatter.CreateSimpleStringFormatter("The left value must be between 1 and 5.")*/);
    public static readonly StyledProperty<IAvaloniaStringFormatter?> SharpRightOutOfRangeFormatterProperty = AvaloniaProperty.Register<CronPickerFieldItem, IAvaloniaStringFormatter?>(nameof(SharpRightOutOfRangeFormatter)/*, defaultValue:AvaloniaStringFormatter.CreateSimpleStringFormatter("The right value must be between 1 and 5.")*/);
    public static readonly StyledProperty<IAvaloniaStringFormatter?> LastLeftOutOfRangeFormatterProperty = AvaloniaProperty.Register<CronPickerFieldItem, IAvaloniaStringFormatter?>(nameof(LastLeftOutOfRangeFormatter)/*, defaultValue:AvaloniaStringFormatter.CreateSimpleStringFormatter("The left value must be between 1 and 5.")*/);
    public static readonly StyledProperty<IAvaloniaStringFormatter?> LastLeftEmptyFormatterProperty = AvaloniaProperty.Register<CronPickerFieldItem, IAvaloniaStringFormatter?>(nameof(LastLeftEmptyFormatter)/*, defaultValue:AvaloniaStringFormatter.CreateSimpleStringFormatter("The left value must be specified.")*/);
    public static readonly StyledProperty<IAvaloniaStringFormatter?> WModeLeftOutOfRangeFormatterProperty = AvaloniaProperty.Register<CronPickerFieldItem, IAvaloniaStringFormatter?>(nameof(WModeLeftOutOfRangeFormatter)/*, defaultValue:AvaloniaStringFormatter.CreateSimpleStringFormatter("The left value must be between 1 and 5.")*/);
    public static readonly StyledProperty<IAvaloniaStringFormatter?> WModeLeftEmptyFormatterProperty = AvaloniaProperty.Register<CronPickerFieldItem, IAvaloniaStringFormatter?>(nameof(WModeLeftEmptyFormatter)/*, defaultValue:AvaloniaStringFormatter.CreateSimpleStringFormatter("The left value cannot be empty.")*/);
    public static readonly DirectProperty<CronPickerFieldItem, byte> CurrentModeProperty = AvaloniaProperty.RegisterDirect<CronPickerFieldItem, byte>(nameof(CurrentMode), o => o.CurrentMode, (o, v) => o.CurrentMode = v);
    
    private byte _currentMode;
    private DirectPropertyBase<bool>? _currentModeProperty;
    
    internal AvaloniaDictionary<string, string>? _fieldTypeValueMap { get; set; }
    internal Action<string?>? _valueChanged { get; set; }

    /// <summary>
    /// Whether the field allows special values.
    /// </summary>
    public bool IsAllowSpecialValue
    {
        get => GetValue(IsAllowSpecialValueProperty);
        set => SetValue(IsAllowSpecialValueProperty, value);
    }

    /// <summary>
    /// The value of the field.
    /// </summary>
    public string? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>
    /// The header of the field.
    /// </summary>
    public string? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public byte CurrentMode
    {
        get => _currentMode;
        private set => SetAndRaise(CurrentModeProperty, ref _currentMode, value);
    }
    
    #region 消息

    /// <summary>
    /// 不支持解析的匹配类型消息格式
    /// </summary>
    public IAvaloniaStringFormatter? NotSupportMatchTypeFormatter
    {
        get => GetValue(NotSupportMatchTypeFormatterProperty);
        set => SetValue(NotSupportMatchTypeFormatterProperty, value);
    }

    /// <summary>
    /// 非数值异常消息格式
    /// </summary>
    public IAvaloniaStringFormatter? NotNumValueFormatter
    {
        get => GetValue(NotNumValueFormatterProperty);
        set => SetValue(NotNumValueFormatterProperty, value);
    }

    /// <summary>
    /// 表达式异常消息格式
    /// </summary>
    public IAvaloniaStringFormatter? ExpressionErrorFormatter
    {
        get => GetValue(ExpressionErrorFormatterProperty);
        set => SetValue(ExpressionErrorFormatterProperty, value);
    }
    
    /// <summary>
    /// 不支持的表达式消息格式
    /// </summary>
    public IAvaloniaStringFormatter? ExpressionNotSupportFormatter
    {
        get => GetValue(ExpressionNotSupportFormatterProperty);
        set => SetValue(ExpressionNotSupportFormatterProperty, value);
    }

    /// <summary>
    /// 截止数值小于起始数值时抛出的异常消息格式
    /// </summary>
    public IAvaloniaStringFormatter? RangeEndLessThenStartFormatter
    {
        get => GetValue(RangeEndLessThenStartFormatterProperty);
        set => SetValue(RangeEndLessThenStartFormatterProperty, value);
    }

    /// <summary>
    /// 指定范围值超出范围时抛出的异常消息格式
    /// </summary>
    public IAvaloniaStringFormatter? RangeStartOrEndOutOfRangeFormatter
    {
        get => GetValue(RangeStartOrEndOutOfRangeFormatterProperty);
        set => SetValue(RangeStartOrEndOutOfRangeFormatterProperty, value);
    }
    
    /// <summary>
    /// The formatter for the enumerator empty error message.
    /// </summary>
    public IAvaloniaStringFormatter? EnumeratorEmptyFormatter
    {
        get => GetValue(EnumeratorEmptyFormatterProperty);
        set => SetValue(EnumeratorEmptyFormatterProperty, value);
    }

    /// <summary>
    /// The formatter for the enumerator out of range error message.
    /// </summary>
    public IAvaloniaStringFormatter? EnumeratorOutOfRangeFormatter
    {
        get => GetValue(EnumeratorOutOfRangeFormatterProperty);
        set => SetValue(EnumeratorOutOfRangeFormatterProperty, value);
    }

    /// <summary>
    /// The formatter for the interval left out of range error message.
    /// </summary>
    public IAvaloniaStringFormatter? IntervalLeftOutOfRangeFormatter
    {
        get => GetValue(IntervalLeftOutOfRangeFormatterProperty);
        set => SetValue(IntervalLeftOutOfRangeFormatterProperty, value);
    }

    /// <summary>
    /// The formatter for the interval right out of range error message.
    /// </summary>
    public IAvaloniaStringFormatter? IntervalRightOutOfRangeFormatter
    {
        get => GetValue(IntervalRightOutOfRangeFormatterProperty);
        set => SetValue(IntervalRightOutOfRangeFormatterProperty, value);
    }

    /// <summary>
    /// The formatter for the # left out of range error message.
    /// </summary>
    public IAvaloniaStringFormatter? SharpLeftOutOfRangeFormatter
    {
        get => GetValue(SharpLeftOutOfRangeFormatterProperty);
        set => SetValue(SharpLeftOutOfRangeFormatterProperty, value);
    }

    /// <summary>
    /// The formatter for the # right out of range error message.
    /// </summary>
    public IAvaloniaStringFormatter? SharpRightOutOfRangeFormatter
    {
        get => GetValue(SharpRightOutOfRangeFormatterProperty);
        set => SetValue(SharpRightOutOfRangeFormatterProperty, value);
    }

    /// <summary>
    /// The formatter for the last left out of range error message.
    /// </summary>
    public IAvaloniaStringFormatter? LastLeftOutOfRangeFormatter
    {
        get => GetValue(LastLeftOutOfRangeFormatterProperty);
        set => SetValue(LastLeftOutOfRangeFormatterProperty, value);
    }

    /// <summary>
    /// The formatter for the last left unspecified error message.
    /// </summary>
    public IAvaloniaStringFormatter? LastLeftEmptyFormatter
    {
        get => GetValue(LastLeftEmptyFormatterProperty);
        set => SetValue(LastLeftEmptyFormatterProperty, value);
    }
    
    /// <summary>
    /// The formatter for the W mode left out of range error message.
    /// </summary>
    public IAvaloniaStringFormatter? WModeLeftOutOfRangeFormatter
    {
        get => GetValue(WModeLeftOutOfRangeFormatterProperty);
        set => SetValue(WModeLeftOutOfRangeFormatterProperty, value);
    }

    /// <summary>
    /// The formatter for the W mode left empty error message.
    /// </summary>
    public IAvaloniaStringFormatter? WModeLeftEmptyFormatter
    {
        get => GetValue(WModeLeftEmptyFormatterProperty);
        set => SetValue(WModeLeftEmptyFormatterProperty, value);
    }

    #endregion
    
    /// <summary>
    ///  是否支持占位符模式
    /// </summary>
    protected virtual bool IsSupportedPlaceholderMode => false;
    
    /// <summary>
    ///  是否支持最后日期模式
    /// </summary>
    protected virtual bool IsSupportedLastMode => false;

    /// <summary>
    /// 是否支持间隔触发模式
    /// </summary>
    protected virtual bool IsSupportedIntervalMode => false;
    
    /// <summary>
    /// 是否支持枚举模式
    /// </summary>
    protected virtual bool IsSupportedEnumerableMode => false;
    
    /// <summary>
    /// 是否支持工作日模式（W）
    /// </summary>
    protected virtual bool IsSupportedSpecialWeekdayMode => false;
    
    /// <summary>
    /// 是否支持 # 匹配模式
    /// </summary>
    protected virtual bool IsSupportedSharpMode => false;
    
    /// <summary>
    /// The key of the field in the dictionary.
    /// </summary>
    protected abstract string DictKey { get; }

    /// <summary>
    /// 根据 枚举值数组 获取 枚举值字符串
    /// </summary>
    /// <param name="values"></param>
    /// <returns></returns>
    public static string GetListExpression(params int[] values) => string.Join(FIELD_CHAR_LIST_AND, values);

    /// <summary>
    /// 根据 枚举值数组 获取 枚举值字符串
    /// </summary>
    /// <param name="values"></param>
    /// <returns></returns>
    public static string GetListExpression(IEnumerable<int> values) => string.Join(FIELD_CHAR_LIST_AND, values);

    /// <summary>
    /// 根据 范围值 获取 范围表达式 
    /// </summary>
    /// <remarks>例如在Minutes域使用5-20，表示从5分到20分钟每分钟触发一次</remarks>
    /// <param name="start"> 开始值 </param>
    /// <param name="end"> 结束值 </param>
    /// <returns> 范围表达式 </returns>
    public static string GetRangeExpression(int start, int end) => string.Concat(start, FIELD_CHAR_RANGE, end);
    
    /// <summary>
    /// 根据起始值和间隔值获取间隔循环表达式 
    /// </summary>
    /// <remarks>例如在Minutes域使用5/20,则意味着5分钟触发一次，而25，45等分别触发一次.</remarks>
    /// <param name="start"> 开始值 </param>
    /// <param name="interval"> 间隔值 </param>
    /// <returns> 间隔循环表达式 </returns>
    public static string GetIntervalExpression(int start, int interval) => string.Concat(start, FIELD_CHAR_INTERVAL, interval);
    
    /// <summary>
    /// 获取最近工作日表达式[W模式]
    /// </summary>
    /// <param name="weekday"> 指定本月某日 </param>
    /// <returns> 最近工作日表达式 </returns>
    public static string GetRecentlyWeekdayExpression(int weekday) => string.Concat(weekday, FIELD_CHAR_WORKDAY);

    /// <summary>
    /// 获取最后日期表达式
    /// </summary>
    /// <param name="time"> 指定时间 </param>
    /// <returns> 最后日期表达式 </returns>
    public static string GetLastTimeExpression(int time) => string.Concat(time, FIELD_CHAR_LAST);
    
    /// <summary>
    /// 获取特殊星期几表达式
    /// </summary>
    /// <param name="week"> 第几周 1-5 </param>
    /// <param name="weekday"> 星期几 1-7 </param>
    /// <returns> 特殊星期几表达式 </returns>
    public static string GetSpecialWeekdayExpression(int week, int weekday) => string.Concat(weekday, FIELD_CHAR_WEEKDAY, week);
    
    /// <summary>
    /// 根据指定字符串解析表达式
    /// </summary>
    /// <param name="text">待解析文本</param>
    /// <exception cref="ExpressionParseException"></exception>
    public virtual void ParsetoValue(string text)
    {
        if (string.Equals(text, FIELD_CHAR_ANY))
        {
            this.ChangeCurrentModeToAnyMode();
        }
        else if (string.Equals(text, FIELD_CHAR_PLACEHOLDER))
        {
            if (!this.IsSupportedPlaceholderMode)
            {
                throw GetParseException_NotSupportMatchType(FIELD_CHAR_PLACEHOLDER, 0);
            }

            this.ChangeCurrentModeToPlaceholderMode();
        }
        else if (text.Contains(FIELD_CHAR_RANGE))
        {
            string[] arr = text.Split(new string[] { FIELD_CHAR_RANGE }, StringSplitOptions.RemoveEmptyEntries);
            if (arr.Length == 2)
            {
                int leftParam;
                int rightParam;
                if (int.TryParse(arr[0], out int v1))
                {
                    leftParam = v1;
                }
                else
                {
                    throw GetParseException_NotNumValue(arr[0], 0);
                }

                if (int.TryParse(arr[1], out int v2))
                {
                    rightParam = v2;
                }
                else
                {
                    throw GetParseException_NotNumValue(arr[1], 1);
                }

                this.UnSafeChangeCurrentModeToRange(leftParam, rightParam);
            }
            else
            {
                throw GetParseException_ErrorExpression(FIELD_CHAR_RANGE, 0);
            }
        }
        else if (text.Contains(FIELD_CHAR_INTERVAL))
        {
            if (!this.IsSupportedIntervalMode)
            {
                throw GetParseException_NotSupportMatchType(FIELD_CHAR_INTERVAL, 0);
            }
            
            string[] arr = text.Split(new string[] { FIELD_CHAR_INTERVAL }, StringSplitOptions.RemoveEmptyEntries);
            if (arr.Length == 2)
            {
                int leftParam;
                int rightParam;
                if (int.TryParse(arr[0], out int v1))
                {
                    leftParam = v1;
                }
                else
                {
                    throw GetParseException_NotNumValue(arr[0], 0);
                }

                if (int.TryParse(arr[1], out int v2))
                {
                    rightParam = v2;
                }
                else
                {
                    throw GetParseException_NotNumValue(arr[1], 1);
                }

                this.UnSafeChangeCurrentModeToInterval(leftParam, rightParam);
            }
            else
            {
                throw GetParseException_ErrorExpression(FIELD_CHAR_INTERVAL, 0);
            }
        }
        else if (text.Contains(FIELD_CHAR_LIST_AND))
        {
            if (!this.IsSupportedEnumerableMode)
            {
                throw GetParseException_NotSupportMatchType(FIELD_CHAR_LIST_AND, 0);
            }
            
            string[] arr = text.Split(new string[] { FIELD_CHAR_LIST_AND }, StringSplitOptions.RemoveEmptyEntries);
            if (arr.Length > 0)
            {
                int[] values = new int[arr.Length];
                for (int i = 0; i < arr.Length; i++)
                {
                    if (int.TryParse(arr[i], out int value))
                    {
                        values[i] = value;
                    }
                    else
                    {
                        throw GetParseException_NotNumValue(arr[i], i);
                    }
                }
                
                this.UnSafeChangeCurrentModeToList(values);
            }
            else
            {
                throw GetParseException_ErrorExpression(FIELD_CHAR_LIST_AND, 0);
            }
        }
        else if (text.Contains(FIELD_CHAR_LAST))
        {
            if (!this.IsSupportedLastMode)
            {
                throw GetParseException_NotSupportMatchType(FIELD_CHAR_LAST, 0);
            }

            if (string.Equals(text, FIELD_CHAR_LAST))
            {
                this.UnSafeChangeCurrentModeToLast();
            }
            else if (text.EndsWith(FIELD_CHAR_LAST))
            {
                if (int.TryParse(text.TrimEnd(FIELD_CHAR_LAST.ToCharArray()), out int time))
                {
                    this.UnSafeChangeCurrentModeToLast(time);
                }
                else
                {
                    throw GetParseException_NotNumValue(text.TrimEnd(FIELD_CHAR_LAST.ToCharArray()), 0);
                }
            }
            else
            {
                throw GetParseException_ErrorExpression(FIELD_CHAR_LAST, 0);
            }
        }
        else if (text.Contains(FIELD_CHAR_WORKDAY))
        {
            if (!this.IsSupportedSpecialWeekdayMode)
            {
                throw GetParseException_NotSupportMatchType(FIELD_CHAR_WORKDAY, 0);
            }
            
            if (string.Equals(text, FIELD_CHAR_WORKDAY))
            {
                this.UnSafeChangeCurrentModeToWeekday();
            }
            else if (text.EndsWith(FIELD_CHAR_WORKDAY))
            {
                if (int.TryParse(text.TrimEnd(FIELD_CHAR_WORKDAY.ToCharArray()), out int time))
                {
                    this.UnSafeChangeCurrentModeToWeekday(time);
                }
                else
                {
                    throw GetParseException_NotNumValue(text.TrimEnd(FIELD_CHAR_WORKDAY.ToCharArray()), 0);
                }
            }
            else
            {
                throw GetParseException_ErrorExpression(FIELD_CHAR_WORKDAY, 0);
            }
        }
        else if (text.Contains(FIELD_CHAR_WEEKDAY))
        {
            if (!this.IsSupportedSharpMode)
            {
                throw GetParseException_NotSupportMatchType(FIELD_CHAR_WEEKDAY, 0);
            }
            
            string[] arr = text.Split(new string[] { FIELD_CHAR_WEEKDAY }, StringSplitOptions.RemoveEmptyEntries);
            if (arr.Length == 2)
            {
                int param0;
                int param1;
                if (int.TryParse(arr[0], out int v1))
                {
                    param0 = v1;
                }
                else
                {
                    throw GetParseException_NotNumValue(arr[0], 0);
                }

                if (int.TryParse(arr[1], out int v2))
                {
                    param1 = v2;
                }
                else
                {
                    throw GetParseException_NotNumValue(arr[1], 1);
                }

                this.UnSafeChangeCurrentModeToSharp(param0, param1);
            }
            else
            {
                throw GetParseException_ErrorExpression(FIELD_CHAR_WEEKDAY, 0);
            }
        }
        else
        {
            throw GetParseException_ExpressionNotSupport(text, 0);
        }
    }

    /// <summary>
    /// 设置任意值表达式
    /// </summary>
    public virtual void SetValueAnyMode() => this.OnValueChange(FIELD_CHAR_ANY);

    /// <summary>
    /// 设置占位符表达式
    /// </summary>
    public virtual void SetValuePlaceholder()
    {
        try
        {
            ThrowIfNotSupportedPlaceholderMode();
        }
        catch (Exception e)
        {
            return;
        }
        this.OnValueChange(FIELD_CHAR_PLACEHOLDER);
    }

    /// <summary>
    /// 设置周期内循环表达式
    /// </summary>
    /// <param name="start"> 开始值 </param>
    /// <param name="end"> 结束值 </param>
    public virtual void SetValueRange(int start, int end)
    {
        try
        {
            ThrowIfRangeValueInvalid(start, end);
        }
        catch (Exception e)
        {
            return;
        }
        this.OnValueChange(GetRangeExpression(start, end));
    }

    /// <summary>
    /// 设置间隔循环表达式
    /// </summary>
    /// <param name="start"> 开始值 </param>
    /// <param name="interval"> 间隔值 </param>
    public virtual void SetValueInterval(int start, int interval)
    {
        try
        {
            ThrowIfIntervalValueInvalid(start, interval);
        }
        catch (Exception e)
        {
            return;
        }
        this.OnValueChange(GetIntervalExpression(start, interval));
    }

    /// <summary>
    /// 设置枚举值表达式
    /// </summary>
    /// <param name="values"> 枚举值数组 </param>
    public virtual void SetValueList(params int[] values)
    {
        try
        {
            ThrowIfListValueInvalid(values);
        }
        catch (Exception e)
        {
            return;
        }
        this.OnValueChange(GetListExpression(values));
    }

    /// <summary>
    /// 设置枚举值表达式
    /// </summary>
    /// <param name="values"> 枚举值数组 </param>
    public virtual void SetValueList(IEnumerable<int> values)
    {
        try
        {
            ThrowIfListValueInvalid(values);
        }
        catch (Exception e)
        {
            return;
        }
        this.OnValueChange(GetListExpression(values));
    }

    /// <summary>
    /// 设置最后日期表达式
    /// </summary>
    public virtual void SetValueLast()
    {
        try
        {
            ThrowIfNotSupportedLastMode();
        }
        catch (Exception e)
        {
            return;
        }
        this.OnValueChange(FIELD_CHAR_LAST);   
    }
    
    /// <summary>
    /// 设置最后日期表达式
    /// </summary>
    /// <param name="time"> 指定时间 </param>
    public virtual void SetValueLast(int time)
    {
        try
        {
            ThrowIfNotSupportedLastMode();
        }
        catch (Exception e)
        {
            return;
        }
        this.OnValueChange(GetLastTimeExpression(time));   
    }

    public override string ToString()
    {
        return this.DictKey;
    }

    /// <summary>
    /// # 模式应用
    /// </summary>
    /// <param name="leftParam">左侧参数</param>
    /// <param name="rightParam">右侧参数</param>
    protected virtual void UnSafeChangeCurrentModeToSharp(int leftParam, int rightParam)
    {
        throw new NotImplementedException();
    }
    
    /// <summary>
    /// W 模式应用
    /// </summary>
    protected virtual void UnSafeChangeCurrentModeToWeekday()
    {
        throw new NotImplementedException();
    }
    
    /// <summary>
    /// W 模式应用
    /// </summary>
    /// <param name="time">参数</param>
    protected virtual void UnSafeChangeCurrentModeToWeekday(int time)
    {
        throw new NotImplementedException();
    }
    
    /// <summary>
    /// L 模式应用
    /// </summary>
    protected virtual void UnSafeChangeCurrentModeToLast()
    {
        throw new NotImplementedException();
    }
    
    /// <summary>
    /// L 模式应用
    /// </summary>
    /// <param name="time">参数</param>
    protected virtual void UnSafeChangeCurrentModeToLast(int time)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 枚举模式应用
    /// </summary>
    /// <param name="values">枚举值</param>
    protected virtual void UnSafeChangeCurrentModeToList(int[] values)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 间隔触发模式应用
    /// </summary>
    /// <param name="leftParam">左侧参数</param>
    /// <param name="rightParam">右侧参数</param>
    protected virtual void UnSafeChangeCurrentModeToInterval(int leftParam, int rightParam)
    {
        throw new NotImplementedException();
    }
    
    /// <summary>
    /// 范围模式应用
    /// </summary>
    /// <param name="leftParam">左侧参数</param>
    /// <param name="rightParam">右侧参数</param>
    protected virtual void UnSafeChangeCurrentModeToRange(int leftParam, int rightParam)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 占位符模式应用
    /// </summary>
    protected virtual void ChangeCurrentModeToPlaceholderMode()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 任意值模式应用
    /// </summary>
    protected virtual void ChangeCurrentModeToAnyMode()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 检测是否支持占位符模式
    /// </summary>
    /// <exception cref="NotSupportedException"></exception>
    protected virtual void ThrowIfNotSupportedPlaceholderMode()
    {
        if (this.IsSupportedPlaceholderMode == false)
        {
            throw new NotSupportedException("Placeholder mode is not supported in this field.");
        }
    }
    
    /// <summary>
    /// 检测是否支持最后日期模式
    /// </summary>
    /// <exception cref="NotSupportedException"></exception>
    protected virtual void ThrowIfNotSupportedLastMode()
    {
        if (this.IsSupportedLastMode == false)
        {
            throw new NotSupportedException($"不支持的模式-‘{FIELD_CHAR_LAST}’.");
        }
    }

    /// <summary>
    /// 检测枚举值是否有效并抛出异常
    /// </summary>
    /// <param name="values"> 枚举值数组 </param>
    protected virtual void ThrowIfListValueInvalid(IEnumerable<int> values)
    {
        if (this.IsSupportedEnumerableMode == false)
        {
            throw new NotSupportedException($"不支持的模式-‘{FIELD_CHAR_LIST_AND}’.");
        }
    }

    /// <summary>
    /// 检测范围值是否有效并抛出异常
    /// </summary>
    /// <param name="start"> 开始值 </param>
    /// <param name="end"> 结束值 </param>
    protected abstract void ThrowIfRangeValueInvalid(int start, int end);

    /// <summary>
    /// 检测间隔值是否有效并抛出异常
    /// </summary>
    /// <param name="start"> 开始值 </param>
    /// <param name="interval"> 间隔值 </param>
    protected virtual void ThrowIfIntervalValueInvalid(int start, int interval)
    {
        if (!this.IsSupportedIntervalMode)
        {
            throw new NotSupportedException($"不支持的模式-‘{FIELD_CHAR_INTERVAL}’.");
        }
    }

    /// <summary>
    /// 尝试获取字段值
    /// </summary>
    /// <param name="fieldKeyName"> 字段名称 </param>
    /// <param name="fieldValue"> 字段值 </param>
    /// <returns> 是否获取成功 </returns>
    protected virtual bool TryGetFieldValue(string fieldKeyName, out string? fieldValue)
    {
        fieldValue = null;
        if (this._fieldTypeValueMap is not null && this._fieldTypeValueMap.TryGetValue(fieldKeyName, out fieldValue))
        {
            return true;
        }

        return false;
    }

    protected virtual void OnValueChange(string? value)
    {
        Value = value;
    }

    protected virtual void OnValueChanged(string? value)
    {
        _valueChanged?.Invoke(value);
        if (this.IsAllowSpecialValue && this._fieldTypeValueMap is not null)
        {
            value = value?.Trim() ?? string.Empty;
            if (this._fieldTypeValueMap.ContainsKey(this.DictKey))
            {
                this._fieldTypeValueMap[this.DictKey] = value;
            }
            else
            {
                this._fieldTypeValueMap.Add(this.DictKey, value);
            }
        }
    }
    
    protected virtual void OnIsAllowSpecialValueChanged(bool value)
    {
        if (value)
        {
            this.OnValueChanged(this.Value);
        }
        else
        {
            this._fieldTypeValueMap?.Remove(this.DictKey);
            this.OnValueChange(String.Empty);
        }
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        _fieldTypeValueMap = null;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ValueProperty)
        {
            this.OnValueChanged(change.NewValue as string);
        }
        else if (change.Property == IsAllowSpecialValueProperty)
        {
            this.OnIsAllowSpecialValueChanged((bool?)change.NewValue ?? false);
        }
    }

    /// <summary>
    /// 当前模式是否为指定模式
    /// </summary>
    /// <param name="modeCode"> 模式代码 </param>
    /// <returns> 是否为指定模式 </returns>
    protected bool IsModeSelected(byte modeCode) => _currentMode == modeCode;

    protected bool SwitchCurrentModeTo(DirectPropertyBase<bool> property, byte modeCode, bool isModeSelect)
    {
        if (!isModeSelect)
        {
            if (_currentMode == modeCode)
            {
                _currentMode = 0;
                this.RaisePropertyChanged(property, true, false);
                _currentMode = modeCode;
                this.RaisePropertyChanged(property, false, true);
            }

            return false;
        }

        byte currValue = _currentMode;
        if (this.SetAndRaise(CurrentModeProperty, ref currValue, modeCode))
        {
            _currentMode = modeCode;
            if (this._currentModeProperty is not null)
            {
                this.RaisePropertyChanged(this._currentModeProperty, true, false);
            }

            this._currentModeProperty = property;
            OnCurrentModeSelected(property);
            this.RaisePropertyChanged(property, false, true);
            return true;
        }

        return false;
    }

    /// <summary>
    /// 指定模式成功切换后调用
    /// </summary>
    /// <param name="property">成功切换属性</param>
    protected virtual void OnCurrentModeSelected(AvaloniaProperty<bool> property)
    {
    }

    internal Exception GetParseException_EnumeratorOutOfRange(int minValue, int maxValue, Func<string, Exception>? exceptionFactory = null)
    {
        return EnumeratorOutOfRangeFormatter.ExceptionFormat(exceptionFactory ?? (x => new ArgumentException(x)), ("minValue", minValue), ("maxValue", maxValue));
    }

    internal Exception GetParseException_EnumeratorEmpty(Func<string, Exception>? exceptionFactory = null)
    {
        return EnumeratorEmptyFormatter.ExceptionFormat(exceptionFactory ?? (x => new ArgumentException(x)));
    }
    
    internal Exception GetParseException_RangeStartOrEndOutOfRange(int minValue, int maxValue, Func<string, Exception>? exceptionFactory = null)
    {
        return RangeStartOrEndOutOfRangeFormatter.ExceptionFormat(exceptionFactory ?? (x => new ArgumentException(x)), ("minValue", minValue), ("maxValue", maxValue));
    }

    internal Exception GetParseException_RangeEndLessThenStart(Func<string, Exception>? exceptionFactory = null)
    {
        throw RangeEndLessThenStartFormatter.ExceptionFormat(exceptionFactory ?? (x => new ArgumentException(x)));
    }

    internal Exception GetParseException_IntervalLeftOutOfRange(int minValue, int maxValue, Func<string, Exception>? exceptionFactory = null)
    {
        return IntervalLeftOutOfRangeFormatter.ExceptionFormat(exceptionFactory ?? (x => new ArgumentException(x)), ("minValue", minValue), ("maxValue", maxValue));
    }

    internal Exception GetParseException_IntervalRightOutOfRange(int minValue, int maxValue, Func<string, Exception>? exceptionFactory = null)
    {
        return IntervalRightOutOfRangeFormatter.ExceptionFormat(exceptionFactory ?? (x => new ArgumentException(x)), ("minValue", minValue), ("maxValue", maxValue));
    }

    internal Exception GetParseException_SharpLeftOutOfRange(int minValue, int maxValue, Func<string, Exception>? exceptionFactory = null)
    {
        return SharpLeftOutOfRangeFormatter.ExceptionFormat(exceptionFactory ?? (x => new ArgumentException(x)), ("minValue", minValue), ("maxValue", maxValue));
    }

    internal Exception GetParseException_SharpRightOutOfRange(int minValue, int maxValue, Func<string, Exception>? exceptionFactory = null)
    {
        return SharpRightOutOfRangeFormatter.ExceptionFormat(exceptionFactory ?? (x => new ArgumentException(x)), ("minValue", minValue), ("maxValue", maxValue));
    }

    internal Exception GetParseException_LastLeftOutOfRange(int minValue, int maxValue, Func<string, Exception>? exceptionFactory = null)
    {
        return LastLeftOutOfRangeFormatter.ExceptionFormat(exceptionFactory ?? (x => new ArgumentException(x)), ("minValue", minValue), ("maxValue", maxValue));
    }

    internal Exception GetParseException_LastLeftEmpty(Func<string, Exception>? exceptionFactory = null)
    {
        return LastLeftEmptyFormatter.ExceptionFormat(exceptionFactory ?? (x => new ArgumentException(x)));
    }

    internal Exception GetParseException_WModeLeftEmpty(Func<string, Exception>? exceptionFactory = null)
    {
        return WModeLeftEmptyFormatter.ExceptionFormat(exceptionFactory ?? (x => new ArgumentException(x)));
    }

    internal Exception GetParseException_WModeLeftOutOfRange(int minValue, int maxValue, Func<string, Exception>? exceptionFactory = null)
    {
        return WModeLeftOutOfRangeFormatter.ExceptionFormat(exceptionFactory ?? (x => new ArgumentException(x)), ("minValue", minValue), ("maxValue", maxValue));
    }

    
    // 获取非数值异常【表达式解析】
    private ExpressionParseException GetParseException_NotNumValue(string elementString, int column)
    {
        IAvaloniaStringFormatter? msger = this.NotNumValueFormatter;
        return new ExpressionParseException(column, msger is not null ? msger.Format(("num", elementString)) : string.Empty);
    }

    // 获取非法表达式异常【表达式解析】
    private ExpressionParseException GetParseException_NotSupportMatchType(string matchType, int column)
    {
        IAvaloniaStringFormatter? msger = this.NotSupportMatchTypeFormatter;
        return new ExpressionParseException(column, msger is not null? msger.Format(("matchRuler", matchType)) : string.Empty);
    }
    
    // 获取非法表达式异常【表达式解析】
    private ExpressionParseException GetParseException_ErrorExpression(string matchType, int column)
    {
        IAvaloniaStringFormatter? msger = this.ExpressionErrorFormatter;
        return new ExpressionParseException(column, msger is not null? msger.Format(("matchRuler", matchType)) : string.Empty);
    }
    
    // 获取不支持的表达式异常【表达式解析】
    private ExpressionParseException GetParseException_ExpressionNotSupport(string exText, int column)
    {
        IAvaloniaStringFormatter? msger = this.ExpressionNotSupportFormatter;
        return new ExpressionParseException(column, msger is not null? msger.Format(("expression", exText)) : string.Empty);
    }
}