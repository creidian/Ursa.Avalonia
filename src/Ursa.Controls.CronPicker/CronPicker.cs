using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Logging;
using Avalonia.LogicalTree;
using Irihi.Avalonia.Shared.Helpers;

namespace Ursa.Controls;

/// <summary>
/// A control that allows the user to select a cron expression.
/// </summary>
/// <remarks>
/// 具备 功能：
/// 1. 显示cron表达式
/// 2. 选择cron表达式
/// 3. 显示cron表达式的自然语言描述
/// 4. 显示cron表达式的指定步数运行时间节点
/// </remarks>
[TemplatePart(PART_TabControl, typeof(TabControl))]
[TemplatePart(PART_ParseButton, typeof(Button))]
[TemplatePart(PART_CopyButton, typeof(Button))]
public class CronPicker : TemplatedControl
{
    public const string PART_TabControl = "PART_TabControl";
    public const string PART_ParseButton = "PART_ParseButton";
    public const string PART_CopyButton = "PART_CopyButton";
    public static readonly StyledProperty<string?> SecondsProperty = AvaloniaProperty.Register<CronPicker, string?>(nameof(Seconds));
    public static readonly StyledProperty<string?> MinutesProperty = AvaloniaProperty.Register<CronPicker, string?>(nameof(Minutes));
    public static readonly StyledProperty<string?> HoursProperty = AvaloniaProperty.Register<CronPicker, string?>(nameof(Hours));
    public static readonly StyledProperty<string?> DaysOfMonthProperty = AvaloniaProperty.Register<CronPicker, string?>(nameof(DaysOfMonth));
    public static readonly StyledProperty<string?> MonthsProperty = AvaloniaProperty.Register<CronPicker, string?>(nameof(Months));
    public static readonly StyledProperty<string?> DaysOfWeekProperty = AvaloniaProperty.Register<CronPicker, string?>(nameof(DaysOfWeek));
    public static readonly StyledProperty<string?> YearsProperty = AvaloniaProperty.Register<CronPicker, string?>(nameof(Years));
    public static readonly StyledProperty<string?> CronExpressionProperty = AvaloniaProperty.Register<CronPicker, string?>(nameof(CronExpression));
    public static readonly StyledProperty<ICronExpressionParser?> CronExpressionParserProperty = AvaloniaProperty.Register<CronPicker, ICronExpressionParser?>(nameof(CronExpressionParser));
    public static readonly StyledProperty<IEnumerable<DateTime>?> NextRunTimesProperty = AvaloniaProperty.Register<CronPicker, IEnumerable<DateTime>?>(nameof(NextRunTimes));

    public static readonly StyledProperty<string?> DateTimeFormatProperty = AvaloniaProperty.Register<CronPicker, string?>(nameof(DateTimeFormat), defaultValue: "yyyy-MM-dd HH:mm:ss");

    // public static readonly StyledProperty<IEnumerable<CronPickerFieldItem>> FieldsProperty = AvaloniaProperty.Register<CronPicker, IEnumerable<CronPickerFieldItem>>(nameof(CornFields));
    public static readonly DirectProperty<CronPicker, CronPickerFieldItem> SecondsFieldProperty = AvaloniaProperty.RegisterDirect<CronPicker, CronPickerFieldItem>(nameof(SecondsField), o => o.SecondsField, (o, v) => { /*o.SecondsField = v;*/ });
    public static readonly DirectProperty<CronPicker, CronPickerFieldItem> MinutesFieldProperty = AvaloniaProperty.RegisterDirect<CronPicker, CronPickerFieldItem>(nameof(MinutesField), o => o.MinutesField, (o, v) => { /*o.MinutesField = v;*/ });
    public static readonly DirectProperty<CronPicker, CronPickerFieldItem> HoursFieldProperty = AvaloniaProperty.RegisterDirect<CronPicker, CronPickerFieldItem>(nameof(HoursField), o => o.HoursField, (o, v) => { /*o.HoursField = v;*/ });
    public static readonly DirectProperty<CronPicker, CronPickerFieldItem> DaysOfMonthFieldProperty = AvaloniaProperty.RegisterDirect<CronPicker, CronPickerFieldItem>(nameof(DaysOfMonthField), o => o.DaysOfMonthField, (o, v) => { /*o.DaysOfMonthField = v;*/ });
    public static readonly DirectProperty<CronPicker, CronPickerFieldItem> MonthsFieldProperty = AvaloniaProperty.RegisterDirect<CronPicker, CronPickerFieldItem>(nameof(MonthsField), o => o.MonthsField, (o, v) => { /*o.MonthsField = v;*/ });
    public static readonly DirectProperty<CronPicker, CronPickerFieldItem> DaysOfWeekFieldProperty = AvaloniaProperty.RegisterDirect<CronPicker, CronPickerFieldItem>(nameof(DaysOfWeekField), o => o.DaysOfWeekField, (o, v) => { /*o.DaysOfWeekField = v;*/ });
    public static readonly DirectProperty<CronPicker, CronPickerFieldItem> YearsFieldProperty = AvaloniaProperty.RegisterDirect<CronPicker, CronPickerFieldItem>(nameof(YearsField), o => o.YearsField, (o, v) => { /*o.YearsField = v;*/ });
    public static readonly DirectProperty<CronPicker, bool> IsCurrentOperationResultVisibleProperty = AvaloniaProperty.RegisterDirect<CronPicker, bool>(nameof(IsCurrentOperationResultVisible), o => o.IsCurrentOperationResultVisible, (o, v) => { o.IsCurrentOperationResultVisible = v; });
    public static readonly DirectProperty<CronPicker, NotificationType> CurrentOperationResultTypeProperty = AvaloniaProperty.RegisterDirect<CronPicker, NotificationType>(nameof(CurrentOperationResultType), o => o.CurrentOperationResultType, (o, v) => { o.CurrentOperationResultType = v; });
    public static readonly DirectProperty<CronPicker, IMessage?> CurrentOperationResultMessageProperty = AvaloniaProperty.RegisterDirect<CronPicker, IMessage?>(nameof(CurrentOperationResultMessage), o => o.CurrentOperationResultMessage, (o, v) => { o.CurrentOperationResultMessage = v; });
    public static readonly StyledProperty<IAvaloniaStringFormatter?> CopySuccessMessageProperty = AvaloniaProperty.Register<CronPicker, IAvaloniaStringFormatter?>(nameof(CopySuccessMessage)/*, defaultValue: "复制成功！"*/);
    public static readonly StyledProperty<IAvaloniaStringFormatter?> FailedToGetClipboardMessageProperty = AvaloniaProperty.Register<CronPicker, IAvaloniaStringFormatter?>(nameof(FailedToGetClipboardMessage)/*, defaultValue: "无法获取剪贴板！"*/);
    public static readonly StyledProperty<IAvaloniaStringFormatter?> CronExpressionParsedMessageProperty = AvaloniaProperty.Register<CronPicker, IAvaloniaStringFormatter?>(nameof(CronExpressionParsedMessage)/*, defaultValue: "成功解析！"*/);
    public static readonly StyledProperty<IAvaloniaStringFormatter?> CronExpressionParamsNotEnoughErrorMessageProperty = AvaloniaProperty.Register<CronPicker, IAvaloniaStringFormatter?>(nameof(CronExpressionParamsNotEnoughErrorMessage)/*, defaultValue: "参数不足！"*/);
    public static readonly StyledProperty<IAvaloniaStringFormatter?> SecondsExpressionParseErrorFormatProperty = AvaloniaProperty.Register<CronPicker, IAvaloniaStringFormatter?>(nameof(SecondsExpressionParseErrorFormat)/*, defaultValue: "秒解析错误：{0}"*/);
    public static readonly StyledProperty<IAvaloniaStringFormatter?> MinutesExpressionParseErrorFormatProperty = AvaloniaProperty.Register<CronPicker, IAvaloniaStringFormatter?>(nameof(MinutesExpressionParseErrorFormat)/*, defaultValue: "分解析错误：{0}"*/);
    public static readonly StyledProperty<IAvaloniaStringFormatter?> HoursExpressionParseErrorFormatProperty = AvaloniaProperty.Register<CronPicker, IAvaloniaStringFormatter?>(nameof(HoursExpressionParseErrorFormat)/*, defaultValue: "时解析错误：{0}"*/);
    public static readonly StyledProperty<IAvaloniaStringFormatter?> DaysOfMonthExpressionParseErrorFormatProperty = AvaloniaProperty.Register<CronPicker, IAvaloniaStringFormatter?>(nameof(DaysOfMonthExpressionParseErrorFormat)/*, defaultValue: "日解析错误：{0}"*/);
    public static readonly StyledProperty<IAvaloniaStringFormatter?> MonthsExpressionParseErrorFormatProperty = AvaloniaProperty.Register<CronPicker, IAvaloniaStringFormatter?>(nameof(MonthsExpressionParseErrorFormat)/*, defaultValue: "月解析错误：{0}"*/);
    public static readonly StyledProperty<IAvaloniaStringFormatter?> DaysOfWeekExpressionParseErrorFormatProperty = AvaloniaProperty.Register<CronPicker, IAvaloniaStringFormatter?>(nameof(DaysOfWeekExpressionParseErrorFormat)/*, defaultValue: "周解析错误：{0}"*/);
    public static readonly StyledProperty<IAvaloniaStringFormatter?> YearsExpressionParseErrorFormatProperty = AvaloniaProperty.Register<CronPicker, IAvaloniaStringFormatter?>(nameof(YearsExpressionParseErrorFormat)/*, defaultValue: "年解析错误：{0}"*/);
    public static readonly StyledProperty<IAvaloniaStringFormatter?> RecentRunTimesTitleFormatProperty = AvaloniaProperty.Register<CronPicker, IAvaloniaStringFormatter?>(nameof(RecentRunTimesTitleFormat)/*, defaultValue: "最近运行时间：{0}"*/);
    
    private AvaloniaDictionary<string, string>? _fieldTypeValueMap;
    private readonly CronPickerFieldItem _secondsField;
    private readonly CronPickerFieldItem _minutesField;
    private readonly CronPickerFieldItem _hoursField;
    private readonly CronPickerFieldItem _daysOfMonthField;
    private readonly CronPickerFieldItem _monthsField;
    private readonly CronPickerFieldItem _daysOfWeekField;
    private readonly CronPickerFieldItem _yearsField;
    
    private TabControl? _tabControl;
    private Button? _buttonParse;
    private Button? _buttonCopy;
    private bool _isOperationResultShow;
    private NotificationType _currentOperationResultType;
    private IMessage? _currentOperationResultMessage;
    private bool _isTabControlInited;
    
    public CronPicker()
    {
        _fieldTypeValueMap = new AvaloniaDictionary<string, string>();
        WeakReference<CronPicker> weakReference = new WeakReference<CronPicker>(this);
        _fieldTypeValueMap.CollectionChanged += (sender, e) =>
        {
            if (weakReference.TryGetTarget(out CronPicker? target))
            {
                target.OnFieldTypeValueMapCollChanged(e);
            }
        };
        _fieldTypeValueMap.PropertyChanged += (sender, e) =>
        {
            if (weakReference.TryGetTarget(out CronPicker? target))
            {
                target.OnFieldTypeValueMapChanged(e);
            }
        };
        _secondsField = new SecondsCronPickerFieldItem()
        {
            _fieldTypeValueMap = _fieldTypeValueMap,
            _valueChanged = x =>
            {
                if (weakReference.TryGetTarget(out CronPicker? target))
                {
                    target.Seconds = x;
                }
            }
        };
        _minutesField = new MinutesCronPickerFieldItem()
        {
            _fieldTypeValueMap = _fieldTypeValueMap,
            _valueChanged = x =>
            {
                if (weakReference.TryGetTarget(out CronPicker? target))
                {
                    target.Minutes = x;
                }
            }
        };
        _hoursField = new HoursCronPickerFieldItem()
        {
            _fieldTypeValueMap = _fieldTypeValueMap,
            _valueChanged = x =>
            {
                if (weakReference.TryGetTarget(out CronPicker? target))
                {
                    target.Hours = x;
                }
            }
        };
        _daysOfMonthField = new DaysOfMonthCronPickerFieldItem()
        {
            _fieldTypeValueMap = _fieldTypeValueMap,
            _valueChanged = x =>
            {
                if (weakReference.TryGetTarget(out CronPicker? target))
                {
                    target.DaysOfMonth = x;
                }
            }
        };
        _monthsField = new MonthsCronPickerFieldItem()
        {
            _fieldTypeValueMap = _fieldTypeValueMap,
            _valueChanged = x =>
            {
                if (weakReference.TryGetTarget(out CronPicker? target))
                {
                    target.Months = x;
                }
            }
        };
        _daysOfWeekField = new DaysOfWeekCronPickerFieldItem()
        {
            _fieldTypeValueMap = _fieldTypeValueMap,
            _valueChanged = x =>
            {
                if (weakReference.TryGetTarget(out CronPicker? target))
                {
                    target.DaysOfWeek = x;
                }
            }
        };
        _yearsField = new YearsCronPickerFieldItem()
        {
            _fieldTypeValueMap = _fieldTypeValueMap,
            _valueChanged = x =>
            {
                if (weakReference.TryGetTarget(out CronPicker? target))
                {
                    target.Years = x;
                }
            }
        };
        this.CornFields = new CronPickerFieldItem[] { _secondsField, _minutesField, _hoursField, _daysOfMonthField, _monthsField, _daysOfWeekField, _yearsField };
    }

    public string? Seconds
    {
        get => GetValue(SecondsProperty);
        set => SetValue(SecondsProperty, value);
    }

    public string? Minutes
    {
        get => GetValue(MinutesProperty);
        set => SetValue(MinutesProperty, value);
    }

    public string? Hours
    {
        get => GetValue(HoursProperty);
        set => SetValue(HoursProperty, value);
    }

    public string? DaysOfMonth
    {
        get => GetValue(DaysOfMonthProperty);
        set => SetValue(DaysOfMonthProperty, value);
    }

    public string? Months
    {
        get => GetValue(MonthsProperty);
        set => SetValue(MonthsProperty, value);
    }

    public string? DaysOfWeek
    {
        get => GetValue(DaysOfWeekProperty);
        set => SetValue(DaysOfWeekProperty, value);
    }

    public string? Years
    {
        get => GetValue(YearsProperty);
        set => SetValue(YearsProperty, value);
    }

    public string? CronExpression
    {
        get => GetValue(CronExpressionProperty);
        set => SetValue(CronExpressionProperty, value);
    }

    /// <summary>
    /// A delegate to get the next run time of the cron expression.
    /// </summary>
    public Func<string, DateTime, DateTime>? GetNextRunTime { get; set; }

    /// <summary>
    /// The count of the next run time.
    /// </summary>
    public int NextRunTimeCount { get; set; } = 10;

    /// <summary>
    /// The next run times of the cron expression.
    /// </summary>
    public IEnumerable<DateTime>? NextRunTimes
    {
        get => GetValue(NextRunTimesProperty);
        protected set => SetValue(NextRunTimesProperty, value);
    }

    /// <summary>
    /// The format of the date time.
    /// </summary>
    public string? DateTimeFormat
    {
        get => GetValue(DateTimeFormatProperty);
        set => SetValue(DateTimeFormatProperty, value);
    }

    public CronPickerFieldItem SecondsField
    {
        get => _secondsField;
        // private set => SetAndRaise(SecondsFieldProperty, ref _secondsField, value);
    }

    public CronPickerFieldItem MinutesField
    {
        get => _minutesField;
        // private set => SetAndRaise(MinutesFieldProperty, ref _minutesField, value);
    }

    public CronPickerFieldItem HoursField
    {
        get => _hoursField;
        // private set => SetAndRaise(HoursFieldProperty, ref _hoursField, value);
    }

    public CronPickerFieldItem DaysOfMonthField
    {
        get => _daysOfMonthField;
        // private set => SetAndRaise(DaysOfMonthFieldProperty, ref _daysOfMonthField, value);
    }

    public CronPickerFieldItem MonthsField
    {
        get => _monthsField;
        // private set => SetAndRaise(MonthsFieldProperty, ref _monthsField, value);
    }

    public CronPickerFieldItem DaysOfWeekField
    {
        get => _daysOfWeekField;
        // private set => SetAndRaise(DaysOfWeekFieldProperty, ref _daysOfWeekField, value);
    }

    public CronPickerFieldItem YearsField
    {
        get => _yearsField;
        // private set => SetAndRaise(YearsFieldProperty, ref _yearsField, value);
    }

    public CronPickerFieldItem[] CornFields { get; }

    /// <summary>
    /// Whether to show the operation result.
    /// </summary>
    public bool IsCurrentOperationResultVisible
    {
        get => _isOperationResultShow;
        private set => SetAndRaise(IsCurrentOperationResultVisibleProperty, ref _isOperationResultShow, value);
    }

    /// <summary>
    /// The type of the current operation result.
    /// </summary>
    public NotificationType CurrentOperationResultType
    {
        get => _currentOperationResultType;
        private set => SetAndRaise(CurrentOperationResultTypeProperty, ref _currentOperationResultType, value);
    }
    
    /// <summary>
    /// The message of the current operation result.
    /// </summary>
    public IMessage? CurrentOperationResultMessage
    {
        get => _currentOperationResultMessage;
        private set => SetAndRaise(CurrentOperationResultMessageProperty, ref _currentOperationResultMessage, value);
    }

    /// <summary>
    /// cron 表达式解析器.
    /// </summary>
    public ICronExpressionParser? CronExpressionParser
    {
        get => GetValue(CronExpressionParserProperty);
        set => SetValue(CronExpressionParserProperty, value);
    }
    
    #region 消息提示相关属性

    public IAvaloniaStringFormatter? CopySuccessMessage
    {
        get => GetValue(CopySuccessMessageProperty);
        set => SetValue(CopySuccessMessageProperty, value);
    }
    
    public IAvaloniaStringFormatter? FailedToGetClipboardMessage
    {
        get => GetValue(FailedToGetClipboardMessageProperty);
        set => SetValue(FailedToGetClipboardMessageProperty, value);
    }
    
    public IAvaloniaStringFormatter? CronExpressionParsedMessage
    {
        get => GetValue(CronExpressionParsedMessageProperty);
        set => SetValue(CronExpressionParsedMessageProperty, value);
    }

    public IAvaloniaStringFormatter? CronExpressionParamsNotEnoughErrorMessage
    {
        get => GetValue(CronExpressionParamsNotEnoughErrorMessageProperty);
        set => SetValue(CronExpressionParamsNotEnoughErrorMessageProperty, value);
    }
    
    public IAvaloniaStringFormatter? SecondsExpressionParseErrorFormat
    {
        get => GetValue(SecondsExpressionParseErrorFormatProperty);
        set => SetValue(SecondsExpressionParseErrorFormatProperty, value);
    }
    
    public IAvaloniaStringFormatter? MinutesExpressionParseErrorFormat
    {
        get => GetValue(MinutesExpressionParseErrorFormatProperty);
        set => SetValue(MinutesExpressionParseErrorFormatProperty, value);
    }
    
    public IAvaloniaStringFormatter? HoursExpressionParseErrorFormat
    {
        get => GetValue(HoursExpressionParseErrorFormatProperty);
        set => SetValue(HoursExpressionParseErrorFormatProperty, value);
    }
    
    public IAvaloniaStringFormatter? DaysOfMonthExpressionParseErrorFormat
    {
        get => GetValue(DaysOfMonthExpressionParseErrorFormatProperty);
        set => SetValue(DaysOfMonthExpressionParseErrorFormatProperty, value);
    }
    
    public IAvaloniaStringFormatter? MonthsExpressionParseErrorFormat
    {
        get => GetValue(MonthsExpressionParseErrorFormatProperty);
        set => SetValue(MonthsExpressionParseErrorFormatProperty, value);
    }
    
    public IAvaloniaStringFormatter? DaysOfWeekExpressionParseErrorFormat
    {
        get => GetValue(DaysOfWeekExpressionParseErrorFormatProperty);
        set => SetValue(DaysOfWeekExpressionParseErrorFormatProperty, value);
    }
    
    public IAvaloniaStringFormatter? YearsExpressionParseErrorFormat
    {
        get => GetValue(YearsExpressionParseErrorFormatProperty);
        set => SetValue(YearsExpressionParseErrorFormatProperty, value);
    }
    
    public IAvaloniaStringFormatter? RecentRunTimesTitleFormat
    {
        get => GetValue(RecentRunTimesTitleFormatProperty);
        set => SetValue(RecentRunTimesTitleFormatProperty, value);
    }
    
    #endregion
    
    public void ShowWarningNotification(string title = "Warning", string? message = null) => ShowOperationResult(NotificationType.Warning, title, message);
    
    public void ShowInfoNotification(string title = "Info", string? message = null) => ShowOperationResult(NotificationType.Information, title, message);
    
    public void ShowErrorNotification(string title = "Error", string? message = null) => ShowOperationResult(NotificationType.Error, title, message);
    
    public void ShowSuccessNotification(string title = "Success", string? message = null) => ShowOperationResult(NotificationType.Success, title, message);

    public void ShowOperationResult(NotificationType type, string title, string? message = null)
    {
        CurrentOperationResultMessage = /*new Notification(title, message)*/ new Toast(message ?? title, type);
        CurrentOperationResultType = type;
        IsCurrentOperationResultVisible = true;
    }
    
    public void HideOperationResult(bool isClear = false)
    {
        IsCurrentOperationResultVisible = false;
        if (isClear)
        {
            CurrentOperationResultType = NotificationType.Information;
            CurrentOperationResultMessage = null;
            // CurrentOperationResultTitle = null;
        }
    }
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _tabControl = e.NameScope.Find<TabControl>(PART_TabControl);
        OnTabControlLoad();
        Button.ClickEvent.RemoveHandler(ParsePicker, _buttonParse);
        _buttonParse = e.NameScope.Find<Button>(PART_ParseButton);
        Button.ClickEvent.AddHandler(ParsePicker, _buttonParse);
        Button.ClickEvent.RemoveHandler(CopyPicker, _buttonCopy);
        _buttonCopy = e.NameScope.Find<Button>(PART_CopyButton);
        Button.ClickEvent.AddHandler(CopyPicker, _buttonCopy);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SecondsProperty || change.Property == MinutesProperty || change.Property == HoursProperty || change.Property == DaysOfMonthProperty || change.Property == MonthsProperty || change.Property == DaysOfWeekProperty || change.Property == YearsProperty)
        {
            this.RefreshCronExpression();
        }
        else if (change.Property == CronExpressionProperty)
        {
            this.OnCronExpressionChanged(change.NewValue?.ToString() ?? string.Empty);
        }
    }
    
    private static void Button_Click(object? sender, RoutedEventArgs e)
    {
        ILogical? btn = sender as ILogical;
        CronPicker? parent = btn?.GetLogicalAncestors().OfType<CronPicker>().FirstOrDefault();
        parent?.HideOperationResult();
    }
    
    private void OnTabControlLoad()
    {
        TabControl? tabControl = _tabControl;
        if (tabControl != null && _isTabControlInited == false)
        {
            _isTabControlInited = true;
            if (tabControl.DataContext is not null)
            {
                tabControl.DataContext = null;
            }
            
#if false // ItemTemplate 中的模型回传数据类型与数据源数据类型不一致
            tabControl.ItemTemplate = new FuncDataTemplate(x =>
            {
                return true;
                // if (x is CronPickerFieldItem field)
                // {
                //     return true;
                // }
                //
                // return false;
            }, (o, scope) =>
            {
                if (o is CronPickerFieldItem field)
                {
                    return new TextBlock()
                    {
                        [!TextBlock.TextProperty] = field[!CronPickerFieldItem.HeaderProperty]
                    };
                }
                
                return new TextBlock(){ Text = "Error" };
            }, true);
            tabControl.ContentTemplate = new FuncDataTemplate<CronPickerFieldItem>(x => true, x => x, true);
            tabControl.ItemsSource = this.CornFields;
#else
            CronPickerFieldItem[] items = this.CornFields;
            for (int i = 0; i < items.Length; i++)
            {
                CronPickerFieldItem item = items[i];
                if (tabControl.Items.FirstOrDefault(x => x is TabItem tabitem && tabitem.Content == item) is null)
                {
                    TabItem tabitem = new CronPickerFieldTabItem()
                    {
                        [!HeaderedContentControl.HeaderProperty] = item[!CronPickerFieldItem.HeaderProperty],
                        Content = item
                    };
                    tabControl.Items.Add(tabitem);
                }
            }
#endif
        }
    }
    
    private void OnCronExpressionChanged(string value)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            Func<string,DateTime,DateTime>? func = this.GetNextRunTime;
            if (func == null)
            {
                return;
            }

            int count = NextRunTimeCount;
            DateTime nextRunTime = DateTime.Now;
            List<DateTime> runTimes = new List<DateTime>();
            for (int i = 0; i < count; i++)
            {
                nextRunTime = func(value, nextRunTime);
                runTimes.Add(nextRunTime);
            }
            
            NextRunTimes = runTimes;
        }
        catch (Exception e)
        {
        }
    }

    private void RefreshCronExpression()
    {
        string expression;
        if (!string.IsNullOrWhiteSpace(Years))
        {
            expression = string.Format("{0} {1} {2} {3} {4} {5} {6}", Seconds, Minutes, Hours, DaysOfMonth, Months, DaysOfWeek, Years);
        }
        else
        {
            expression = string.Format("{0} {1} {2} {3} {4} {5}", Seconds, Minutes, Hours, DaysOfMonth, Months, DaysOfWeek);
        }
            
        CronExpression = expression;
    }
    
    private void ChangeCronExpression(string? expression)
    {
        CronExpression = expression;
    }

    private void OnFieldTypeValueMapCollChanged(NotifyCollectionChangedEventArgs args)
    {
    }

    private void OnFieldTypeValueMapChanged(PropertyChangedEventArgs args)
    {
    }
    
    // 复制当前 cron 表达式到粘贴板
    private async void CopyPicker(object sender, RoutedEventArgs e)
    {
        try
        {
            string? text = this.CronExpression;
            IClipboard? clipb = TopLevel.GetTopLevel(this)?.Clipboard;
            if (clipb is not null)
            {
                await clipb.SetTextAsync(text);
                if (Avalonia.Threading.Dispatcher.UIThread.CheckAccess())
                {
                    this.ShowSuccessNotification("Success", CopySuccessMessage?.Format());
                }
                else
                {
                    await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => { this.ShowSuccessNotification("Success", CopySuccessMessage?.Format()); });
                }
            }
            else
            {
                this.ShowErrorNotification("Error", FailedToGetClipboardMessage?.Format());
            }
        }
        catch (Exception exception)
        {
            this.ShowErrorNotification("Error", exception.Message);
            Logger.TryGet(LogEventLevel.Error, LogArea.Control)?.Log(this, $"{exception}");
        }
    }

    // 反解析当前 cron 表达式到各项
    private void ParsePicker(object sender, RoutedEventArgs e)
    {
        List<DateTime> runTimes;
        try
        {
            string? cronString = this.CronExpression;
            string[]? args = cronString?.Trim().Split(new char[] { ' ' }, options: StringSplitOptions.RemoveEmptyEntries);
            if (args is null || args.Length < 6)
            {
                this.ShowWarningNotification("Warning", CronExpressionParamsNotEnoughErrorMessage?.Format());
                return;
            }

            try
            {
                this._secondsField.ParsetoValue(args[0]); // 秒~解析
            }
            catch (Exception exception)
            {
                throw new Exception(SecondsExpressionParseErrorFormat?.Format(("msg", exception.Message)), exception);
            }

            try
            {
                this._minutesField.ParsetoValue(args[1]); // 分~解析
            }
            catch (Exception exception)
            {
                throw new Exception(MinutesExpressionParseErrorFormat?.Format(("msg", exception.Message)), exception);
            }

            try
            {
                this._hoursField.ParsetoValue(args[2]); // 时~解析
            }
            catch (Exception exception)
            {
                throw new Exception(HoursExpressionParseErrorFormat?.Format(("msg", exception.Message)), exception);
            }

            try
            {
                this._daysOfMonthField.ParsetoValue(args[3]); // 日~解析
            }
            catch (Exception exception)
            {
                throw new Exception(DaysOfMonthExpressionParseErrorFormat?.Format(("msg", exception.Message)), exception);
            }

            try
            {
                this._monthsField.ParsetoValue(args[4]); // 月~解析
            }
            catch (Exception exception)
            {
                throw new Exception(MonthsExpressionParseErrorFormat?.Format(("msg", exception.Message)), exception);
            }

            try
            {
                this._daysOfWeekField.ParsetoValue(args[5]); // 周~解析
            }
            catch (Exception exception)
            {
                throw new Exception(DaysOfWeekExpressionParseErrorFormat?.Format(("msg", exception.Message)), exception);
            }

            try
            {
                if (args.Length > 6)
                {
                    this._yearsField.ParsetoValue(args[6]); // 年~解析
                }
                else
                {
                    this._yearsField.ParsetoValue(string.Empty); // 年~清空
                }
            }
            catch (Exception exception)
            {
                throw new Exception(YearsExpressionParseErrorFormat?.Format(("msg", exception.Message)), exception);
            }
            
            // 外部解析器解析并获取指定次数运行时间
            if (this.CronExpressionParser is not null)
            {
                DateTime start = DateTime.Now;
                runTimes = new List<DateTime>();
                for (int i = 0; i < NextRunTimeCount; i++)
                {
                    try
                    {
                        DateTime nextTime = this.CronExpressionParser.GetNextTime(cronString, start);
                        if (nextTime == start)
                        {
                            break;
                        }
                        
                        start = nextTime;
                    }
                    catch (Exception exception)
                    {
                        throw new Exception(exception.Message, exception);
                    }
                    
                    runTimes.Add(start);
                }

                this.NextRunTimes = runTimes;
            }
            
            this.ShowSuccessNotification("Success", CronExpressionParsedMessage?.Format());
        }
        catch (Exception exception)
        {
            this.ShowErrorNotification("Error", exception.Message);
            Logger.TryGet(LogEventLevel.Error, LogArea.Control)?.Log(this, $"{exception}");
        }
        
    }
}