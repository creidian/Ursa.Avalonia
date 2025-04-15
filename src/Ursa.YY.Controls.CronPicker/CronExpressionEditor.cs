using System.Collections;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Logging;
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
[TemplatePart(PART_FieldsHost, typeof(CronPickerFieldsContainer))]
[TemplatePart(PART_ParseButton, typeof(Button))]
[TemplatePart(PART_CopyButton, typeof(Button))]
[TemplatePart(PART_CronExpression, typeof(TextBox))]
[TemplatePart(PART_SecondsField, typeof(CronPickerFieldView))]
[TemplatePart(PART_MinutesField, typeof(CronPickerFieldView))]
[TemplatePart(PART_HoursField, typeof(CronPickerFieldView))]
[TemplatePart(PART_DaysOfMonthField, typeof(CronPickerFieldView))]
[TemplatePart(PART_MonthsField, typeof(CronPickerFieldView))]
[TemplatePart(PART_DaysOfWeekField, typeof(CronPickerFieldView))]
[TemplatePart(PART_YearsField, typeof(CronPickerFieldView))]
public class CronExpressionEditor : TemplatedControl
{
    public const string PART_CronExpression = "PART_CronExpression";
    public const string PART_FieldsHost = "PART_FieldsHost";
    public const string PART_ParseButton = "PART_ParseButton";
    public const string PART_CopyButton = "PART_CopyButton";
    public const string PART_SecondsField = "PART_SecondsField";
    public const string PART_MinutesField = "PART_MinutesField";
    public const string PART_HoursField = "PART_HoursField";
    public const string PART_DaysOfMonthField = "PART_DaysOfMonthField";
    public const string PART_MonthsField = "PART_MonthsField";
    public const string PART_DaysOfWeekField = "PART_DaysOfWeekField";
    public const string PART_YearsField = "PART_YearsField";
    private const char PARTS_SEPARATOR = ' ';
    
    public static readonly StyledProperty<string?> SecondsProperty = AvaloniaProperty.Register<CronExpressionEditor, string?>(nameof(Seconds), defaultBindingMode: BindingMode.OneWay);
    public static readonly StyledProperty<string?> MinutesProperty = AvaloniaProperty.Register<CronExpressionEditor, string?>(nameof(Minutes), defaultBindingMode: BindingMode.OneWay);
    public static readonly StyledProperty<string?> HoursProperty = AvaloniaProperty.Register<CronExpressionEditor, string?>(nameof(Hours), defaultBindingMode: BindingMode.OneWay);
    public static readonly StyledProperty<string?> DaysOfMonthProperty = AvaloniaProperty.Register<CronExpressionEditor, string?>(nameof(DaysOfMonth), defaultBindingMode: BindingMode.OneWay);
    public static readonly StyledProperty<string?> MonthsProperty = AvaloniaProperty.Register<CronExpressionEditor, string?>(nameof(Months), defaultBindingMode: BindingMode.OneWay);
    public static readonly StyledProperty<string?> DaysOfWeekProperty = AvaloniaProperty.Register<CronExpressionEditor, string?>(nameof(DaysOfWeek), defaultBindingMode: BindingMode.OneWay);
    public static readonly StyledProperty<string?> YearsProperty = AvaloniaProperty.Register<CronExpressionEditor, string?>(nameof(Years), defaultBindingMode: BindingMode.OneWay);
    public static readonly StyledProperty<string?> CronExpressionProperty = AvaloniaProperty.Register<CronExpressionEditor, string?>(nameof(CronExpression));
    public static readonly StyledProperty<ICronExpressionParser?> CronExpressionParserProperty = AvaloniaProperty.Register<CronExpressionEditor, ICronExpressionParser?>(nameof(CronExpressionParser));
    public static readonly StyledProperty<int> NextRunTimeCountProperty = AvaloniaProperty.Register<CronExpressionEditor, int>(nameof(NextRunTimeCount), defaultValue: 10);
    public static readonly StyledProperty<string?> DateTimeFormatProperty = AvaloniaProperty.Register<CronExpressionEditor, string?>(nameof(DateTimeFormat), defaultValue: "yyyy-MM-dd HH:mm:ss");
    public static readonly StyledProperty<string?> RecentRunTimesTitleProperty = AvaloniaProperty.Register<CronExpressionEditor, string?>(nameof(RecentRunTimesTitle), defaultValue: "");
    public static readonly DirectProperty<CronExpressionEditor, bool> IsCurrentOperationResultVisibleProperty = AvaloniaProperty.RegisterDirect<CronExpressionEditor, bool>(nameof(IsCurrentOperationResultVisible), o => o.IsCurrentOperationResultVisible, (o, v) => { o.IsCurrentOperationResultVisible = v; });
    public static readonly DirectProperty<CronExpressionEditor, NotificationType> CurrentOperationResultTypeProperty = AvaloniaProperty.RegisterDirect<CronExpressionEditor, NotificationType>(nameof(CurrentOperationResultType), o => o.CurrentOperationResultType, (o, v) => { o.CurrentOperationResultType = v; });
    public static readonly DirectProperty<CronExpressionEditor, IMessage?> CurrentOperationResultMessageProperty = AvaloniaProperty.RegisterDirect<CronExpressionEditor, IMessage?>(nameof(CurrentOperationResultMessage), o => o.CurrentOperationResultMessage, (o, v) => { o.CurrentOperationResultMessage = v; });
    public static readonly StyledProperty<IAvaloniaStringFormatter?> CopySuccessMessageProperty = AvaloniaProperty.Register<CronExpressionEditor, IAvaloniaStringFormatter?>(nameof(CopySuccessMessage) /*, defaultValue: "复制成功！"*/);
    public static readonly StyledProperty<IAvaloniaStringFormatter?> FailedToGetClipboardMessageProperty = AvaloniaProperty.Register<CronExpressionEditor, IAvaloniaStringFormatter?>(nameof(FailedToGetClipboardMessage) /*, defaultValue: "无法获取剪贴板！"*/);
    public static readonly StyledProperty<IAvaloniaStringFormatter?> CronExpressionParsedMessageProperty = AvaloniaProperty.Register<CronExpressionEditor, IAvaloniaStringFormatter?>(nameof(CronExpressionParsedMessage) /*, defaultValue: "成功解析！"*/);
    public static readonly StyledProperty<IAvaloniaStringFormatter?> CronExpressionParamsNotEnoughErrorMessageProperty = AvaloniaProperty.Register<CronExpressionEditor, IAvaloniaStringFormatter?>(nameof(CronExpressionParamsNotEnoughErrorMessage) /*, defaultValue: "参数不足！"*/);
    public static readonly StyledProperty<IAvaloniaStringFormatter?> RecentRunTimesTitleFormatProperty = AvaloniaProperty.Register<CronExpressionEditor, IAvaloniaStringFormatter?>(nameof(RecentRunTimesTitleFormat) /*, defaultValue: "最近运行时间：{0}"*/);
    public static readonly StyledProperty<IAvaloniaStringFormatter?> NotFindSecondFieldMessageProperty = AvaloniaProperty.Register<CronExpressionEditor, IAvaloniaStringFormatter?>(nameof(NotFindSecondFieldMessage) /*, defaultValue: "未找到秒字段！"*/);
    public static readonly StyledProperty<IAvaloniaStringFormatter?> NotFindMinuteFieldMessageProperty = AvaloniaProperty.Register<CronExpressionEditor, IAvaloniaStringFormatter?>(nameof(NotFindMinuteFieldMessage) /*, defaultValue: "未找到分字段！"*/);
    public static readonly StyledProperty<IAvaloniaStringFormatter?> NotFindHourFieldMessageProperty = AvaloniaProperty.Register<CronExpressionEditor, IAvaloniaStringFormatter?>(nameof(NotFindHourFieldMessage) /*, defaultValue: "未找到时字段！"*/);
    public static readonly StyledProperty<IAvaloniaStringFormatter?> NotFindDaysOfMonthFieldMessageProperty = AvaloniaProperty.Register<CronExpressionEditor, IAvaloniaStringFormatter?>(nameof(NotFindDaysOfMonthFieldMessage) /*, defaultValue: "未找到日字段！"*/);
    public static readonly StyledProperty<IAvaloniaStringFormatter?> NotFindMonthsFieldMessageProperty = AvaloniaProperty.Register<CronExpressionEditor, IAvaloniaStringFormatter?>(nameof(NotFindMonthsFieldMessage) /*, defaultValue: "未找到月字段！"*/);
    public static readonly StyledProperty<IAvaloniaStringFormatter?> NotFindDaysOfWeekFieldMessageProperty = AvaloniaProperty.Register<CronExpressionEditor, IAvaloniaStringFormatter?>(nameof(NotFindDaysOfWeekFieldMessage) /*, defaultValue: "未找到周字段！"*/);
    public static readonly StyledProperty<IAvaloniaStringFormatter?> NotFindYearsFieldMessageProperty = AvaloniaProperty.Register<CronExpressionEditor, IAvaloniaStringFormatter?>(nameof(NotFindYearsFieldMessage) /*, defaultValue: "未找到年字段！"*/);
    public static readonly StyledProperty<IAvaloniaStringFormatter?> CronPartExpressionParseErrorFormatProperty = AvaloniaProperty.Register<CronExpressionEditor, IAvaloniaStringFormatter?>(nameof(CronPartExpressionParseErrorFormat) /*, defaultValue: "秒解析错误：{0}"*/);
    public static readonly StyledProperty<IAvaloniaStringFormatter?> CronPartVerifyErrorFormatProperty = AvaloniaProperty.Register<CronExpressionEditor, IAvaloniaStringFormatter?>(nameof(CronPartVerifyErrorFormat) /*, defaultValue: "字段“{0}”验证错误：{1}"*/);
    public static readonly StyledProperty<CronExpressionParseResult?> CronExpressionCalculationResultProperty = AvaloniaProperty.Register<CronExpressionEditor, CronExpressionParseResult?>(nameof(CronExpressionCalculationResult));
    // public static readonly StyledProperty<IDataTemplate?> CronFieldContentTemplateProperty = AvaloniaProperty.Register<CronExpressionEditor, IDataTemplate?>(nameof(CronFieldContentTemplate));
    // public static readonly StyledProperty<IDataTemplate?> CronFieldItemTemplateProperty = AvaloniaProperty.Register<CronExpressionEditor, IDataTemplate?>(nameof(CronFieldItemTemplate));
    // public static readonly StyledProperty<IDataTemplate?> CronRulerItemTemplateProperty = AvaloniaProperty.Register<CronExpressionEditor, IDataTemplate?>(nameof(CronRulerItemTemplate));
    // public static readonly StyledProperty<DataTemplates?> CronRulerContentDataTemplatesProperty = AvaloniaProperty.Register<CronExpressionEditor, DataTemplates?>(nameof(CronRulerContentDataTemplates));
    public static readonly StyledProperty<DataTemplates?> CronRulerItemDataTemplatesProperty = AvaloniaProperty.Register<CronExpressionEditor, DataTemplates?>(nameof(CronRulerItemDataTemplates));
    public static readonly StyledProperty<CronPickerRulers> SecondsRulerItemsProperty = AvaloniaProperty.Register<CronExpressionEditor, CronPickerRulers>("SecondsRulerItems");
    public static readonly StyledProperty<CronPickerRulers> MinutesRulerItemsProperty = AvaloniaProperty.Register<CronExpressionEditor, CronPickerRulers>("MinutesRulerItems");
    public static readonly StyledProperty<CronPickerRulers> HoursRulerItemsProperty = AvaloniaProperty.Register<CronExpressionEditor, CronPickerRulers>("HoursRulerItems");
    public static readonly StyledProperty<CronPickerRulers> DaysOfMonthRulerItemsProperty = AvaloniaProperty.Register<CronExpressionEditor, CronPickerRulers>("DaysOfMonthRulerItems");
    public static readonly StyledProperty<CronPickerRulers> MonthsRulerItemsProperty = AvaloniaProperty.Register<CronExpressionEditor, CronPickerRulers>("MonthsRulerItems");
    public static readonly StyledProperty<CronPickerRulers> DaysOfWeekRulerItemsProperty = AvaloniaProperty.Register<CronExpressionEditor, CronPickerRulers>("DaysOfWeekRulerItems");
    public static readonly StyledProperty<CronPickerRulers> YearsRulerItemsProperty = AvaloniaProperty.Register<CronExpressionEditor, CronPickerRulers>("YearsRulerItems");
    public static readonly StyledProperty<IEnumerable<ICronRuler>?> SecondsRulerItemsSourceProperty = AvaloniaProperty.Register<CronExpressionEditor, IEnumerable<ICronRuler>?>(nameof(SecondsRulerItemsSource));
    public static readonly StyledProperty<IEnumerable<ICronRuler>?> MinutesRulerItemsSourceProperty = AvaloniaProperty.Register<CronExpressionEditor, IEnumerable<ICronRuler>?>(nameof(MinutesRulerItemsSource));
    public static readonly StyledProperty<IEnumerable<ICronRuler>?> HoursRulerItemsSourceProperty = AvaloniaProperty.Register<CronExpressionEditor, IEnumerable<ICronRuler>?>(nameof(HoursRulerItemsSource));
    public static readonly StyledProperty<IEnumerable<ICronRuler>?> DaysOfMonthRulerItemsSourceProperty = AvaloniaProperty.Register<CronExpressionEditor, IEnumerable<ICronRuler>?>(nameof(DaysOfMonthRulerItemsSource));
    public static readonly StyledProperty<IEnumerable<ICronRuler>?> MonthsRulerItemsSourceProperty = AvaloniaProperty.Register<CronExpressionEditor, IEnumerable<ICronRuler>?>(nameof(MonthsRulerItemsSource));
    public static readonly StyledProperty<IEnumerable<ICronRuler>?> DaysOfWeekRulerItemsSourceProperty = AvaloniaProperty.Register<CronExpressionEditor, IEnumerable<ICronRuler>?>(nameof(DaysOfWeekRulerItemsSource));
    public static readonly StyledProperty<IEnumerable<ICronRuler>?> YearsRulerItemsSourceProperty = AvaloniaProperty.Register<CronExpressionEditor, IEnumerable<ICronRuler>?>(nameof(YearsRulerItemsSource));
    public static readonly StyledProperty<int> SelectedFieldIndexProperty = AvaloniaProperty.Register<CronExpressionEditor, int>(nameof(SelectedFieldIndex), defaultValue: -1);
    public static readonly StyledProperty<object?> SelectedFieldObjectProperty = AvaloniaProperty.Register<CronExpressionEditor, object?>(nameof(SelectedFieldObject));
    public static readonly StyledProperty<int> SecondsSelectedIndexProperty = AvaloniaProperty.Register<CronExpressionEditor, int>(nameof(SecondsSelectedIndex), defaultValue: -1);
    public static readonly StyledProperty<int> MinutesSelectedIndexProperty = AvaloniaProperty.Register<CronExpressionEditor, int>(nameof(MinutesSelectedIndex), defaultValue: -1);
    public static readonly StyledProperty<int> HoursSelectedIndexProperty = AvaloniaProperty.Register<CronExpressionEditor, int>(nameof(HoursSelectedIndex), defaultValue: -1);
    public static readonly StyledProperty<int> DaysOfMonthSelectedIndexProperty = AvaloniaProperty.Register<CronExpressionEditor, int>(nameof(DaysOfMonthSelectedIndex), defaultValue: -1);
    public static readonly StyledProperty<int> MonthsSelectedIndexProperty = AvaloniaProperty.Register<CronExpressionEditor, int>(nameof(MonthsSelectedIndex), defaultValue: -1);
    public static readonly StyledProperty<int> DaysOfWeekSelectedIndexProperty = AvaloniaProperty.Register<CronExpressionEditor, int>(nameof(DaysOfWeekSelectedIndex), defaultValue: -1);
    public static readonly StyledProperty<int> YearsSelectedIndexProperty = AvaloniaProperty.Register<CronExpressionEditor, int>(nameof(YearsSelectedIndex), defaultValue: -1);
    public static readonly StyledProperty<object?> SecondsSelectedItemProperty = AvaloniaProperty.Register<CronExpressionEditor, object?>(nameof(SecondsSelectedItem));
    public static readonly StyledProperty<object?> MinutesSelectedItemProperty = AvaloniaProperty.Register<CronExpressionEditor, object?>(nameof(MinutesSelectedItem));
    public static readonly StyledProperty<object?> HoursSelectedItemProperty = AvaloniaProperty.Register<CronExpressionEditor, object?>(nameof(HoursSelectedItem));
    public static readonly StyledProperty<object?> DaysOfMonthSelectedItemProperty = AvaloniaProperty.Register<CronExpressionEditor, object?>(nameof(DaysOfMonthSelectedItem));
    public static readonly StyledProperty<object?> MonthsSelectedItemProperty = AvaloniaProperty.Register<CronExpressionEditor, object?>(nameof(MonthsSelectedItem));
    public static readonly StyledProperty<object?> DaysOfWeekSelectedItemProperty = AvaloniaProperty.Register<CronExpressionEditor, object?>(nameof(DaysOfWeekSelectedItem));
    public static readonly StyledProperty<object?> YearsSelectedItemProperty = AvaloniaProperty.Register<CronExpressionEditor, object?>(nameof(YearsSelectedItem));
    public static readonly StyledProperty<object?> SecondsSelectedValueProperty = AvaloniaProperty.Register<CronExpressionEditor, object?>(nameof(SecondsSelectedValue));
    public static readonly StyledProperty<object?> MinutesSelectedValueProperty = AvaloniaProperty.Register<CronExpressionEditor, object?>(nameof(MinutesSelectedValue));
    public static readonly StyledProperty<object?> HoursSelectedValueProperty = AvaloniaProperty.Register<CronExpressionEditor, object?>(nameof(HoursSelectedValue));
    public static readonly StyledProperty<object?> DaysOfMonthSelectedValueProperty = AvaloniaProperty.Register<CronExpressionEditor, object?>(nameof(DaysOfMonthSelectedValue));
    public static readonly StyledProperty<object?> MonthsSelectedValueProperty = AvaloniaProperty.Register<CronExpressionEditor, object?>(nameof(MonthsSelectedValue));
    public static readonly StyledProperty<object?> DaysOfWeekSelectedValueProperty = AvaloniaProperty.Register<CronExpressionEditor, object?>(nameof(DaysOfWeekSelectedValue));
    public static readonly StyledProperty<object?> YearsSelectedValueProperty = AvaloniaProperty.Register<CronExpressionEditor, object?>(nameof(YearsSelectedValue));
    public static readonly StyledProperty<bool> IsSecondEnabledProperty = AvaloniaProperty.Register<CronExpressionEditor, bool>(nameof(IsSecondEnabled), defaultValue: true);
    public static readonly StyledProperty<bool> IsYearEnabledProperty = AvaloniaProperty.Register<CronExpressionEditor, bool>(nameof(IsYearEnabled), defaultValue: true);
    public static readonly StyledProperty<CronExpressionStringFormatter?> CronExpressionFormatterProperty = AvaloniaProperty.Register<CronExpressionEditor, CronExpressionStringFormatter?>(nameof(CronExpressionFormatter));
    public static readonly RoutedEvent<UsualValueChangedEventArgs<string>> CronExpressionChangedEvent = RoutedEvent.Register<CronExpressionEditor, UsualValueChangedEventArgs<string>>(nameof(CronExpressionChanged), RoutingStrategies.Bubble);
    
    private Button? _buttonParse;
    private Button? _buttonCopy;
    private AvaloniaDictionary<string, string>? _fieldTypeValueMap;
    private bool _isOperationResultShow;
    private NotificationType _currentOperationResultType;
    private IMessage? _currentOperationResultMessage;
    private bool _isTabControlInited;
    private bool _isCronExpressionParsed;
    private CronPickerFieldsContainer? _fieldsHost;
    private TextBox? _tb_cronExpression;
    private CronPickerFieldView? _secondsField;
    private CronPickerFieldView? _minutesField;
    private CronPickerFieldView? _hoursField;
    private CronPickerFieldView? _daysOfMonthField;
    private CronPickerFieldView? _monthsField;
    private CronPickerFieldView? _daysOfWeekField;
    private CronPickerFieldView? _yearsField;
    private bool _isPartChangedFlag;
    
    public static CronPickerRulers GetSecondsRulerItems(CronExpressionEditor o) => o.SecondsField.RulerItems;
    public static CronPickerRulers GetMinutesRulerItems(CronExpressionEditor o) => o.MinutesField.RulerItems;
    public static CronPickerRulers GetHoursRulerItems(CronExpressionEditor o) => o.HoursField.RulerItems;
    public static CronPickerRulers GetDaysOfMonthRulerItems(CronExpressionEditor o) => o.DaysOfMonthField.RulerItems;
    public static CronPickerRulers GetMonthsRulerItems(CronExpressionEditor o) => o.MonthsField.RulerItems;
    public static CronPickerRulers GetDaysOfWeekRulerItems(CronExpressionEditor o) => o.DaysOfWeekField.RulerItems;
    public static CronPickerRulers GetYearsRulerItems(CronExpressionEditor o) => o.YearsField.RulerItems;

    static CronExpressionEditor()
    {
        // CronRulersProperty.Changed.AddClassHandler<CronPicker>((o, e) => o.OnRulersChanged(e));
        // CronFieldContentTemplateProperty.Changed.AddClassHandler<CronExpressionEditor>((o, e) => o.OnCronFieldContentTemplateChanged(e));
        IsSecondEnabledProperty.Changed.AddClassHandler<CronExpressionEditor>((o, e) => o.OnIsSecondEnabledChanged(e));
        IsYearEnabledProperty.Changed.AddClassHandler<CronExpressionEditor>((o, e) => o.OnIsYearEnabledChanged(e));
    }

    public CronExpressionEditor()
    {
        _fieldTypeValueMap = new AvaloniaDictionary<string, string>();
        SecondsField = CronPickerField.Create(CronFieldTypes.Second);
        MinutesField = CronPickerField.Create(CronFieldTypes.Minute);
        HoursField = CronPickerField.Create(CronFieldTypes.Hour);
        DaysOfMonthField = CronPickerField.Create(CronFieldTypes.DayOfMonth);
        MonthsField = CronPickerField.Create(CronFieldTypes.Month);
        DaysOfWeekField = CronPickerField.Create(CronFieldTypes.DayOfWeek);
        YearsField = CronPickerField.Create(CronFieldTypes.Year);
    }

    public event EventHandler<UsualValueChangedEventArgs<string>> CronExpressionChanged
    {
        add => AddHandler(CronExpressionChangedEvent, value);
        remove => RemoveHandler(CronExpressionChangedEvent, value);
    }

    public int SelectedFieldIndex
    {
        get => GetValue(SelectedFieldIndexProperty);
        set => SetValue(SelectedFieldIndexProperty, value);
    }

    public object? SelectedFieldObject
    {
        get => GetValue(SelectedFieldObjectProperty);
        set => SetValue(SelectedFieldObjectProperty, value);
    }

    #region ...秒

    public bool IsSecondEnabled
    {
        get => GetValue(IsSecondEnabledProperty);
        set => SetValue(IsSecondEnabledProperty, value);
    }

    public int SecondsSelectedIndex
    {
        get => GetValue(SecondsSelectedIndexProperty);
        set => SetValue(SecondsSelectedIndexProperty, value);
    }

    public object? SecondsSelectedItem
    {
        get => GetValue(SecondsSelectedItemProperty);
        set => SetValue(SecondsSelectedItemProperty, value);
    }

    public object? SecondsSelectedValue
    {
        get => GetValue(SecondsSelectedValueProperty);
        set => SetValue(SecondsSelectedValueProperty, value);
    }

    public IEnumerable<ICronRuler>? SecondsRulerItemsSource
    {
        get => GetValue(SecondsRulerItemsSourceProperty);
        set => SetValue(SecondsRulerItemsSourceProperty, value);
    }

    #endregion

    #region ...分

    public int MinutesSelectedIndex
    {
        get => GetValue(MinutesSelectedIndexProperty);
        set => SetValue(MinutesSelectedIndexProperty, value);
    }

    public object? MinutesSelectedItem
    {
        get => GetValue(MinutesSelectedItemProperty);
        set => SetValue(MinutesSelectedItemProperty, value);
    }

    public object? MinutesSelectedValue
    {
        get => GetValue(MinutesSelectedValueProperty);
        set => SetValue(MinutesSelectedValueProperty, value);
    }

    public IEnumerable<ICronRuler>? MinutesRulerItemsSource
    {
        get => GetValue(MinutesRulerItemsSourceProperty);
        set => SetValue(MinutesRulerItemsSourceProperty, value);
    }

    #endregion

    #region ...时

    public int HoursSelectedIndex
    {
        get => GetValue(HoursSelectedIndexProperty);
        set => SetValue(HoursSelectedIndexProperty, value);
    }

    public object? HoursSelectedItem
    {
        get => GetValue(HoursSelectedItemProperty);
        set => SetValue(HoursSelectedItemProperty, value);
    }

    public object? HoursSelectedValue
    {
        get => GetValue(HoursSelectedValueProperty);
        set => SetValue(HoursSelectedValueProperty, value);
    }

    public IEnumerable<ICronRuler>? HoursRulerItemsSource
    {
        get => GetValue(HoursRulerItemsSourceProperty);
        set => SetValue(HoursRulerItemsSourceProperty, value);
    }

    #endregion

    #region ...日

    public int DaysOfMonthSelectedIndex
    {
        get => GetValue(DaysOfMonthSelectedIndexProperty);
        set => SetValue(DaysOfMonthSelectedIndexProperty, value);
    }

    public object? DaysOfMonthSelectedItem
    {
        get => GetValue(DaysOfMonthSelectedItemProperty);
        set => SetValue(DaysOfMonthSelectedItemProperty, value);
    }

    public object? DaysOfMonthSelectedValue
    {
        get => GetValue(DaysOfMonthSelectedValueProperty);
        set => SetValue(DaysOfMonthSelectedValueProperty, value);
    }

    public IEnumerable<ICronRuler>? DaysOfMonthRulerItemsSource
    {
        get => GetValue(DaysOfMonthRulerItemsSourceProperty);
        set => SetValue(DaysOfMonthRulerItemsSourceProperty, value);
    }

    #endregion

    #region ...月

    public int MonthsSelectedIndex
    {
        get => GetValue(MonthsSelectedIndexProperty);
        set => SetValue(MonthsSelectedIndexProperty, value);
    }

    public object? MonthsSelectedItem
    {
        get => GetValue(MonthsSelectedItemProperty);
        set => SetValue(MonthsSelectedItemProperty, value);
    }

    public object? MonthsSelectedValue
    {
        get => GetValue(MonthsSelectedValueProperty);
        set => SetValue(MonthsSelectedValueProperty, value);
    }

    public IEnumerable<ICronRuler>? MonthsRulerItemsSource
    {
        get => GetValue(MonthsRulerItemsSourceProperty);
        set => SetValue(MonthsRulerItemsSourceProperty, value);
    }

    #endregion

    #region ...周

    public int DaysOfWeekSelectedIndex
    {
        get => GetValue(DaysOfWeekSelectedIndexProperty);
        set => SetValue(DaysOfWeekSelectedIndexProperty, value);
    }

    public object? DaysOfWeekSelectedItem
    {
        get => GetValue(DaysOfWeekSelectedItemProperty);
        set => SetValue(DaysOfWeekSelectedItemProperty, value);
    }

    public object? DaysOfWeekSelectedValue
    {
        get => GetValue(DaysOfWeekSelectedValueProperty);
        set => SetValue(DaysOfWeekSelectedValueProperty, value);
    }

    public IEnumerable<ICronRuler>? DaysOfWeekRulerItemsSource
    {
        get => GetValue(DaysOfWeekRulerItemsSourceProperty);
        set => SetValue(DaysOfWeekRulerItemsSourceProperty, value);
    }

    #endregion

    #region ...年

    public bool IsYearEnabled
    {
        get => GetValue(IsYearEnabledProperty);
        set => SetValue(IsYearEnabledProperty, value);
    }

    public int YearsSelectedIndex
    {
        get => GetValue(YearsSelectedIndexProperty);
        set => SetValue(YearsSelectedIndexProperty, value);
    }

    public object? YearsSelectedItem
    {
        get => GetValue(YearsSelectedItemProperty);
        set => SetValue(YearsSelectedItemProperty, value);
    }

    public object? YearsSelectedValue
    {
        get => GetValue(YearsSelectedValueProperty);
        set => SetValue(YearsSelectedValueProperty, value);
    }

    public IEnumerable<ICronRuler>? YearsRulerItemsSource
    {
        get => GetValue(YearsRulerItemsSourceProperty);
        set => SetValue(YearsRulerItemsSourceProperty, value);
    }

    #endregion

    public CronPickerField SecondsField { get; }
    public CronPickerField MinutesField { get; }
    public CronPickerField HoursField { get; }
    public CronPickerField DaysOfMonthField { get; }
    public CronPickerField MonthsField { get; }
    public CronPickerField DaysOfWeekField { get; }
    public CronPickerField YearsField { get; }

    public DataTemplates? CronRulerItemDataTemplates
    {
        get => this.GetValue(CronRulerItemDataTemplatesProperty);
        set => this.SetValue(CronRulerItemDataTemplatesProperty, value);
    }

    #region 属性

    public string? Seconds
    {
        get => GetValue(SecondsProperty);
        private set => SetValue(SecondsProperty, value);
    }

    public string? Minutes
    {
        get => GetValue(MinutesProperty);
        private set => SetValue(MinutesProperty, value);
    }

    public string? Hours
    {
        get => GetValue(HoursProperty);
        private set => SetValue(HoursProperty, value);
    }

    public string? DaysOfMonth
    {
        get => GetValue(DaysOfMonthProperty);
        private set => SetValue(DaysOfMonthProperty, value);
    }

    public string? Months
    {
        get => GetValue(MonthsProperty);
        private set => SetValue(MonthsProperty, value);
    }

    public string? DaysOfWeek
    {
        get => GetValue(DaysOfWeekProperty);
        private set => SetValue(DaysOfWeekProperty, value);
    }

    public string? Years
    {
        get => GetValue(YearsProperty);
        private set => SetValue(YearsProperty, value);
    }

    public string? CronExpression
    {
        get => GetValue(CronExpressionProperty);
        set => SetValue(CronExpressionProperty, value);
    }

    /// <summary>
    /// The count of the next run time.
    /// </summary>
    public int NextRunTimeCount
    {
        get => GetValue(NextRunTimeCountProperty);
        set => SetValue(NextRunTimeCountProperty, value);
    }

    /// <summary>
    /// The cron expression calculation result.
    /// </summary>
    public CronExpressionParseResult? CronExpressionCalculationResult
    {
        get => GetValue(CronExpressionCalculationResultProperty);
        private set => SetValue(CronExpressionCalculationResultProperty, value);
    }

    /// <summary>
    /// The format of the date time.
    /// </summary>
    public string? DateTimeFormat
    {
        get => GetValue(DateTimeFormatProperty);
        set => SetValue(DateTimeFormatProperty, value);
    }

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

    /// <summary>
    /// 预计运行日期标题
    /// </summary>
    public string? RecentRunTimesTitle
    {
        get => GetValue(RecentRunTimesTitleProperty);
        private set => SetValue(RecentRunTimesTitleProperty, value);
    }

    public CronExpressionStringFormatter? CronExpressionFormatter
    {
        get => GetValue(CronExpressionFormatterProperty);
        set => SetValue(CronExpressionFormatterProperty, value);
    }
    
    #endregion

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

    /// <summary>
    /// cron 表达式解析失败格式.
    /// </summary>
    /// <remarks>
    /// 提供参数（msg, source, part）：
    /// <list type="bullet">
    /// <item>msg: 错误信息</item>
    /// <item>source: 错误源</item>
    /// <item>part: 错误部分名称</item>
    /// </list>
    /// </remarks>
    public IAvaloniaStringFormatter? CronPartExpressionParseErrorFormat
    {
        get => GetValue(CronPartExpressionParseErrorFormatProperty);
        set => SetValue(CronPartExpressionParseErrorFormatProperty, value);
    }

    /// <summary>
    /// cron 表达式验证失败格式.
    /// </summary>
    /// <remarks>
    /// 提供参数（msg, part）：
    /// <list type="bullet">
    /// <item>msg: 错误信息</item>
    /// <item>part: 错误部分名称</item>
    /// </list>
    /// </remarks>
    public IAvaloniaStringFormatter? CronPartVerifyErrorFormat
    {
        get => GetValue(CronPartVerifyErrorFormatProperty);
        set => SetValue(CronPartVerifyErrorFormatProperty, value);
    }

    public IAvaloniaStringFormatter? RecentRunTimesTitleFormat
    {
        get => GetValue(RecentRunTimesTitleFormatProperty);
        set => SetValue(RecentRunTimesTitleFormatProperty, value);
    }

    /// <summary>
    /// The message of the not find second field.(不提供参数)
    /// </summary>
    public IAvaloniaStringFormatter? NotFindSecondFieldMessage
    {
        get => GetValue(NotFindSecondFieldMessageProperty);
        set => SetValue(NotFindSecondFieldMessageProperty, value);
    }
    
    /// <summary>
    /// The message of the not find minute field.(不提供参数)
    /// </summary>
    public IAvaloniaStringFormatter? NotFindMinuteFieldMessage
    {
        get => GetValue(NotFindMinuteFieldMessageProperty);
        set => SetValue(NotFindMinuteFieldMessageProperty, value);
    }
    
    /// <summary>
    /// The message of the not find hour field.(不提供参数)
    /// </summary>
    public IAvaloniaStringFormatter? NotFindHourFieldMessage
    {
        get => GetValue(NotFindHourFieldMessageProperty);
        set => SetValue(NotFindHourFieldMessageProperty, value);
    }
    
    /// <summary>
    /// The message of the not find days of month field.(不提供参数)
    /// </summary>
    public IAvaloniaStringFormatter? NotFindDaysOfMonthFieldMessage
    {
        get => GetValue(NotFindDaysOfMonthFieldMessageProperty);
        set => SetValue(NotFindDaysOfMonthFieldMessageProperty, value);
    }
    
     /// <summary>
     /// The message of the not find months field.(不提供参数)
     /// </summary>
    public IAvaloniaStringFormatter? NotFindMonthsFieldMessage
    {
        get => GetValue(NotFindMonthsFieldMessageProperty);
        set => SetValue(NotFindMonthsFieldMessageProperty, value);
    }
    
     /// <summary>
     /// The message of the not find days of week field.(不提供参数)
     /// </summary>
    public IAvaloniaStringFormatter? NotFindDaysOfWeekFieldMessage
    {
        get => GetValue(NotFindDaysOfWeekFieldMessageProperty);
        set => SetValue(NotFindDaysOfWeekFieldMessageProperty, value);
    }
    
    /// <summary>
    /// The message of the not find years field.(不提供参数)
    /// </summary>
    public IAvaloniaStringFormatter? NotFindYearsFieldMessage
    {
        get => GetValue(NotFindYearsFieldMessageProperty);
        set => SetValue(NotFindYearsFieldMessageProperty, value);
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

    public void ReSetSecondsRulerItems(IEnumerable<ICronRuler>? value, bool isClear = true)
    {
        ResetCronRulerCollection(this.SecondsField, value, isClear);
    }

    public void AddSecondsRulerItems(IEnumerable<ICronRuler>? value)
    {
        if (value is not null)
        {
            this.SecondsField.RulerItems.AddRange(value);
        }
    }

    public void RemoveSecondsRulerItems(IEnumerable<ICronRuler>? value)
    {
        value?.ToList().ForEach(x => this.SecondsField.RulerItems.Remove(x));
    }

    public void ReSetMinutesRulerItems(IEnumerable<ICronRuler>? value, bool isClear = true)
    {
        ResetCronRulerCollection(this.MinutesField, value, isClear);
    }
    public void AddMinutesRulerItems(IEnumerable<ICronRuler>? value)
    {
        if (value is not null)
        {
            this.MinutesField.RulerItems.AddRange(value);
        }
    }

    public void RemoveMinutesRulerItems(IEnumerable<ICronRuler>? value)
    {
        value?.ToList().ForEach(x => this.MinutesField.RulerItems.Remove(x));
    }

    public void ReSetHoursRulerItems(IEnumerable<ICronRuler>? value, bool isClear = true)
    {
        ResetCronRulerCollection(this.HoursField, value, isClear);
    }
    public void AddHoursRulerItems(IEnumerable<ICronRuler>? value)
    {
        if (value is not null)
        {
            this.HoursField.RulerItems.AddRange(value);
        }
    }

    public void RemoveHoursRulerItems(IEnumerable<ICronRuler>? value)
    {
        value?.ToList().ForEach(x => this.HoursField.RulerItems.Remove(x));
    }

    public void ReSetDaysOfMonthRulerItems(IEnumerable<ICronRuler>? value, bool isClear = true)
    {
        ResetCronRulerCollection(this.DaysOfMonthField, value, isClear);
    }
    public void AddDaysOfMonthRulerItems(IEnumerable<ICronRuler>? value)
    {
        if (value is not null)
        {
            this.DaysOfMonthField.RulerItems.AddRange(value);
        }
    }

    public void RemoveDaysOfMonthRulerItems(IEnumerable<ICronRuler>? value)
    {
        value?.ToList().ForEach(x => this.DaysOfMonthField.RulerItems.Remove(x));
    }

    public void ReSetMonthsRulerItems(IEnumerable<ICronRuler>? value, bool isClear = true)
    {
        ResetCronRulerCollection(this.MonthsField, value, isClear);
    }
    public void AddMonthsRulerItems(IEnumerable<ICronRuler>? value)
    {
        if (value is not null)
        {
            this.MonthsField.RulerItems.AddRange(value);
        }
    }

    public void RemoveMonthsRulerItems(IEnumerable<ICronRuler>? value)
    {
        value?.ToList().ForEach(x => this.MonthsField.RulerItems.Remove(x));
    }

    public void ReSetDaysOfWeekRulerItems(IEnumerable<ICronRuler>? value, bool isClear = true)
    {
        ResetCronRulerCollection(this.DaysOfWeekField, value, isClear);
    }
    public void AddDaysOfWeekRulerItems(IEnumerable<ICronRuler>? value)
    {
        if (value is not null)
        {
            this.DaysOfWeekField.RulerItems.AddRange(value);
        }
    }

    public void RemoveDaysOfWeekRulerItems(IEnumerable<ICronRuler>? value)
    {
        value?.ToList().ForEach(x => this.DaysOfWeekField.RulerItems.Remove(x));
    }

    public void ReSetYearsRulerItems(IEnumerable<ICronRuler>? value, bool isClear = true)
    {
        ResetCronRulerCollection(this.YearsField, value, isClear);
    }
    public void AddYearsRulerItems(IEnumerable<ICronRuler>? value)
    {
        if (value is not null)
        {
            this.YearsField.RulerItems.AddRange(value);
        }
    }

    public void RemoveYearsRulerItems(IEnumerable<ICronRuler>? value)
    {
        value?.ToList().ForEach(x => this.YearsField.RulerItems.Remove(x));
    }
 
    public CronExpressionParseResult? ParseCronExpressionBy(string? cronExpression)
    {
        this.ClearTextBoxErrorTips();
        this.CronExpressionCalculationResult = null;
        string[] args;
        try
        {
            args = ThrowGetCronExpressionParts(cronExpression);
        }
        catch (Exception e)
        {
            return CronExpressionParseResult.CreateError(e.Message);
        }

        this.CronExpression = cronExpression;
        if (this.IsInitialized)
        {
            CronExpressionParseResult result = InnerParsePicker(args, cronExpression);
            // this.ShowOperationResult(result.Status, "", result.Message);
            if (result.Status == NotificationType.Success)
            {
                this.CronExpressionCalculationResult = result;
            }
            else if (result.Status == NotificationType.Error)
            {
                this.SetTextBoxErrorTip(new List<string> { result.Message });
            }
            
            return result;
        }

        return null;
    }

    public void Clear()
    {
        this._secondsField?.UnselectAll();
        this._minutesField?.UnselectAll();
        this._hoursField?.UnselectAll();
        this._daysOfMonthField?.UnselectAll();
        this._monthsField?.UnselectAll();
        this._daysOfWeekField?.UnselectAll();
        this._yearsField?.UnselectAll();
        this.CronExpression = null;
        this.HideOperationResult(true);
        this.TryRefreshRecentRunTimes(null);
    }

    public bool AllowTryParse(string? source)
    {
        try
        {
           var args = ThrowGetCronExpressionParts(source);
           return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _tb_cronExpression = e.NameScope.Find<TextBox>(PART_CronExpression);
        CronPickerFieldView.ValueChangedEvent.RemoveHandler(OnCronFieldRulerValueChanged, _secondsField);
        _secondsField = e.NameScope.Find<CronPickerFieldView>(PART_SecondsField);
        if (_secondsField is not null)
        {
            _secondsField.ItemsSource = this.SecondsField.RulerItems;
        }
        CronPickerFieldView.ValueChangedEvent.AddHandler(OnCronFieldRulerValueChanged, _secondsField);
        CronPickerFieldView.ValueChangedEvent.RemoveHandler(OnCronFieldRulerValueChanged, _minutesField);
        _minutesField = e.NameScope.Find<CronPickerFieldView>(PART_MinutesField);
        if (_minutesField is not null)
        {
            _minutesField.ItemsSource = this.MinutesField.RulerItems;
        }
        CronPickerFieldView.ValueChangedEvent.AddHandler(OnCronFieldRulerValueChanged, _minutesField);
        CronPickerFieldView.ValueChangedEvent.RemoveHandler(OnCronFieldRulerValueChanged, _hoursField);
        _hoursField = e.NameScope.Find<CronPickerFieldView>(PART_HoursField);
        if (_hoursField is not null)
        {
            _hoursField.ItemsSource = this.HoursField.RulerItems;
        }
        CronPickerFieldView.ValueChangedEvent.AddHandler(OnCronFieldRulerValueChanged, _hoursField);
        CronPickerFieldView.ValueChangedEvent.RemoveHandler(OnCronFieldRulerValueChanged, _daysOfMonthField);
        _daysOfMonthField = e.NameScope.Find<CronPickerFieldView>(PART_DaysOfMonthField);
        if (_daysOfMonthField is not null)
        {
            _daysOfMonthField.ItemsSource = this.DaysOfMonthField.RulerItems;
        }
        CronPickerFieldView.ValueChangedEvent.AddHandler(OnCronFieldRulerValueChanged, _daysOfMonthField);
        CronPickerFieldView.ValueChangedEvent.RemoveHandler(OnCronFieldRulerValueChanged, _monthsField);
        _monthsField = e.NameScope.Find<CronPickerFieldView>(PART_MonthsField);
        if (_monthsField is not null)
        {
            _monthsField.ItemsSource = this.MonthsField.RulerItems;
        }
        CronPickerFieldView.ValueChangedEvent.AddHandler(OnCronFieldRulerValueChanged, _monthsField);
        CronPickerFieldView.ValueChangedEvent.RemoveHandler(OnCronFieldRulerValueChanged, _daysOfWeekField);
        _daysOfWeekField = e.NameScope.Find<CronPickerFieldView>(PART_DaysOfWeekField);
        if (_daysOfWeekField is not null)
        {
            _daysOfWeekField.ItemsSource = this.DaysOfWeekField.RulerItems;
        }
        CronPickerFieldView.ValueChangedEvent.AddHandler(OnCronFieldRulerValueChanged, _daysOfWeekField);
        CronPickerFieldView.ValueChangedEvent.RemoveHandler(OnCronFieldRulerValueChanged, _yearsField);
        _yearsField = e.NameScope.Find<CronPickerFieldView>(PART_YearsField);
        if (_yearsField is not null)
        {
            _yearsField.ItemsSource = this.YearsField.RulerItems;
        }
        CronPickerFieldView.ValueChangedEvent.AddHandler(OnCronFieldRulerValueChanged, _yearsField);
        _fieldsHost= e.NameScope.Find<CronPickerFieldsContainer>(PART_FieldsHost);
        Button.ClickEvent.RemoveHandler(ParsePicker, _buttonParse);
        _buttonParse = e.NameScope.Find<Button>(PART_ParseButton);
        Button.ClickEvent.AddHandler(ParsePicker, _buttonParse);
        Button.ClickEvent.RemoveHandler(CopyPicker, _buttonCopy);
        _buttonCopy = e.NameScope.Find<Button>(PART_CopyButton);
        Button.ClickEvent.AddHandler(CopyPicker, _buttonCopy);
        if (!this.IsYearEnabled && this.SelectedFieldIndex == 6)
        {
            this.SelectedFieldIndex = 5;
        }
        if (!this.IsSecondEnabled && this.SelectedFieldIndex == 0)
        {
            this.SelectedFieldIndex = 1;
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        this.Seconds = _secondsField!.Value;
        this.Minutes = _minutesField!.Value;
        this.Hours = _hoursField!.Value;
        this.DaysOfMonth = _daysOfMonthField!.Value;
        this.Months = _monthsField!.Value;
        this.DaysOfWeek = _daysOfWeekField!.Value;
        this.Years = _yearsField!.Value;
        if (!this.IsYearEnabled && this.SelectedFieldIndex == 6)
        {
            this.SelectedFieldIndex = 5;
        }
        if (!this.IsSecondEnabled && this.SelectedFieldIndex == 0)
        {
            this.SelectedFieldIndex = 1;
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SecondsProperty || change.Property == MinutesProperty || change.Property == HoursProperty || change.Property == DaysOfMonthProperty || change.Property == MonthsProperty || change.Property == DaysOfWeekProperty || change.Property == YearsProperty)
        {
            _isPartChangedFlag = true;
            this.ClearTextBoxErrorTips();
            this.RefreshCronExpression();
            if (_isCronExpressionParsed == false)
            {
                HideOperationResult();
                this.TryRefreshRecentRunTimes(this.CronExpression);
            }
        }
        else if (change.Property == CronExpressionProperty)
        {
            if (_isPartChangedFlag)
            {
                _isPartChangedFlag = false;
            }
            else
            {
                this.ClearTextBoxErrorTips();
                this.CronExpressionCalculationResult = null;
            }

            this.OnCronExpressionChanged(change.GetOldValue<string>(), change.GetNewValue<string>());
        }
        else if (change.Property == NextRunTimeCountProperty || change.Property == RecentRunTimesTitleFormatProperty)
        {
            this.RecentRunTimesTitle = RecentRunTimesTitleFormat?.Format(("count", NextRunTimeCount));
        }
        else if (change.Property == SecondsRulerItemsSourceProperty)
        {
            if (change.OldValue is IEnumerable<ICronRuler> oldRulers)
            {
                this.RemoveSecondsRulerItems(oldRulers);
                if (oldRulers is INotifyCollectionChanged notifyCollection)
                {
                    notifyCollection.CollectionChanged -= SecondsRulerItemCollectionChanged;
                }
            }

            if (change.NewValue is IEnumerable<ICronRuler> rulers)
            {
                this.ReSetSecondsRulerItems(rulers, IsInitialized);
                this.SecondsSelectedIndex = 0;
                if (rulers is INotifyCollectionChanged notifyCollection)
                {
                    notifyCollection.CollectionChanged += SecondsRulerItemCollectionChanged;
                }
            }
        }
        else if (change.Property == MinutesRulerItemsSourceProperty)
        {
            if (change.OldValue is IEnumerable<ICronRuler> oldRulers)
            {
                this.RemoveMinutesRulerItems(oldRulers);
                if (oldRulers is INotifyCollectionChanged notifyCollection)
                {
                    notifyCollection.CollectionChanged -= MinutesRulerItemCollectionChanged;
                }
            }

            if (change.NewValue is IEnumerable<ICronRuler> rulers)
            {
                this.ReSetMinutesRulerItems(rulers, IsInitialized);
                this.MinutesSelectedIndex = 0;
                if (rulers is INotifyCollectionChanged notifyCollection)
                {
                    notifyCollection.CollectionChanged += MinutesRulerItemCollectionChanged;
                }
            }
        }
        else if (change.Property == HoursRulerItemsSourceProperty)
        {
            if (change.OldValue is IEnumerable<ICronRuler> oldRulers)
            {
                this.RemoveHoursRulerItems(oldRulers);
                if (oldRulers is INotifyCollectionChanged notifyCollection)
                {
                    notifyCollection.CollectionChanged -= HoursRulerItemCollectionChanged;
                }
            }

            if (change.NewValue is IEnumerable<ICronRuler> rulers)
            {
                this.ReSetHoursRulerItems(rulers, IsInitialized);
                this.HoursSelectedIndex = 0;
                if (rulers is INotifyCollectionChanged notifyCollection)
                {
                    notifyCollection.CollectionChanged += HoursRulerItemCollectionChanged;
                }
            }
        }
        else if (change.Property == DaysOfMonthRulerItemsSourceProperty)
        {
            if (change.OldValue is IEnumerable<ICronRuler> oldRulers)
            {
                this.RemoveDaysOfMonthRulerItems(oldRulers);
                if (oldRulers is INotifyCollectionChanged notifyCollection)
                {
                    notifyCollection.CollectionChanged -= DaysOfMonthRulerItemCollectionChanged;
                }
            }

            if (change.NewValue is IEnumerable<ICronRuler> rulers)
            {
                this.ReSetDaysOfMonthRulerItems(rulers, IsInitialized);
                this.DaysOfMonthSelectedIndex = 0;
                if (rulers is INotifyCollectionChanged notifyCollection)
                {
                    notifyCollection.CollectionChanged += DaysOfMonthRulerItemCollectionChanged;
                }
            }
        }
        else if (change.Property == MonthsRulerItemsSourceProperty)
        {
            if (change.OldValue is IEnumerable<ICronRuler> oldRulers)
            {
                this.RemoveMonthsRulerItems(oldRulers);
                if (oldRulers is INotifyCollectionChanged notifyCollection)
                {
                    notifyCollection.CollectionChanged -= MonthsRulerItemCollectionChanged;
                }
            }

            if (change.NewValue is IEnumerable<ICronRuler> rulers)
            {
                this.ReSetMonthsRulerItems(rulers, IsInitialized);
                this.MonthsSelectedIndex = 0;
                if (rulers is INotifyCollectionChanged notifyCollection)
                {
                    notifyCollection.CollectionChanged += MonthsRulerItemCollectionChanged;
                }
            }
        }
        else if (change.Property == DaysOfWeekRulerItemsSourceProperty)
        {
            if (change.OldValue is IEnumerable<ICronRuler> oldRulers)
            {
                this.RemoveDaysOfWeekRulerItems(oldRulers);
                if (oldRulers is INotifyCollectionChanged notifyCollection)
                {
                    notifyCollection.CollectionChanged -= DaysOfWeekRulerItemCollectionChanged;
                }
            }

            if (change.NewValue is IEnumerable<ICronRuler> rulers)
            {
                this.ReSetDaysOfWeekRulerItems(rulers, IsInitialized);
                this.DaysOfWeekSelectedIndex = 0;
                if (rulers is INotifyCollectionChanged notifyCollection)
                {
                    notifyCollection.CollectionChanged += DaysOfWeekRulerItemCollectionChanged;
                }
            }
        }
        else if (change.Property == YearsRulerItemsSourceProperty)
        {
            if (change.OldValue is IEnumerable<ICronRuler> oldRulers)
            {
                this.RemoveYearsRulerItems(oldRulers);
                if (oldRulers is INotifyCollectionChanged notifyCollection)
                {
                    notifyCollection.CollectionChanged -= YearsRulerItemCollectionChanged;
                }
            }

            if (change.NewValue is IEnumerable<ICronRuler> rulers)
            {
                this.ReSetYearsRulerItems(rulers, IsInitialized);
                this.YearsSelectedIndex = 0;
                if (rulers is INotifyCollectionChanged notifyCollection)
                {
                    notifyCollection.CollectionChanged += YearsRulerItemCollectionChanged;
                }
            }
        }
        else if (change.Property == CronRulerItemDataTemplatesProperty)
        {
            this.UpdateCronRulerItemDataTemplates(change.OldValue as DataTemplates, change.NewValue as DataTemplates);
        }
        else if (change.Property == SelectedFieldIndexProperty)
        {
        }
    }

    protected void SetCronPartValue(CronFieldTypes type, string? value)
    {
        if (type is CronFieldTypes.Second)
        {
            this.Seconds = value;
        }
        else if (type is CronFieldTypes.Minute)
        {
            this.Minutes = value;
        }
        else if (type is CronFieldTypes.Hour)
        {
            this.Hours = value;
        }
        else if (type is CronFieldTypes.DayOfMonth)
        {
            this.DaysOfMonth = value;
        }
        else if (type is CronFieldTypes.Month)
        {
            this.Months = value;
        }
        else if (type is CronFieldTypes.DayOfWeek)
        {
            this.DaysOfWeek = value;
        }
        else if (type is CronFieldTypes.Year)
        {
            this.Years = value;
        }
    }

    private static void ResetCronRulerCollection(CronPickerField field, IEnumerable<ICronRuler>? value, bool isClear)
    {
        if (isClear)
        {
            field.RulerItems.Clear();
        }

        if (value is not null)
        {
            field.RulerItems.AddRange(value);
        }
    }

    private void ClearTextBoxErrorTips()
    {
        if (_tb_cronExpression != null)
        {
            DataValidationErrors.ClearErrors(_tb_cronExpression);
        }
    }

    private void SetTextBoxErrorTip(List<string> errors)
    {
        if (_tb_cronExpression != null)
        {
            DataValidationErrors.SetErrors(_tb_cronExpression, errors);
        }
    }
    
    private void UpdateCronRulerItemDataTemplates(DataTemplates? oldValue, DataTemplates? newValue)
    {
        if (oldValue is not null)
        {
            foreach (IDataTemplate? item in oldValue)
            {
                this.DataTemplates.Remove(item);
            }
        }

        if (newValue is not null)
        {
            foreach (IDataTemplate? item in newValue)
            {
                this.DataTemplates.Add(item);
            }
        }
    }

    private void OnIsYearEnabledChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (!this.IsYearEnabled && this.SelectedFieldIndex == 6)
        {
            this.SelectedFieldIndex = 5;
        }
        this.RefreshCronExpression();
    }

    private void OnIsSecondEnabledChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (!this.IsSecondEnabled && this.SelectedFieldIndex == 0)
        {
            this.SelectedFieldIndex = 1;
        }
        this.RefreshCronExpression();
    }

    private void OnCronFieldRulerValueChanged(object sender, UsualValueChangedEventArgs<string> e)
    {
        if (sender is CronPickerFieldView field)
        {
            this.SetCronPartValue(field.GetFieldType(), e.NewValue);
        }
    }

    private void SecondsRulerItemCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            this.AddSecondsRulerItems(e.NewItems.Cast<ICronRuler>());
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove)
        {
            this.RemoveSecondsRulerItems(e.OldItems.Cast<ICronRuler>());
        }
        else if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            this.RemoveSecondsRulerItems(e.OldItems.Cast<ICronRuler>());
        }
    }

    private void MinutesRulerItemCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            this.AddMinutesRulerItems(e.NewItems.Cast<ICronRuler>());
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove)
        {
            this.RemoveMinutesRulerItems(e.OldItems.Cast<ICronRuler>());
        }
        else if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            this.RemoveMinutesRulerItems(e.OldItems.Cast<ICronRuler>());
        }
    }

    private void HoursRulerItemCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            this.AddHoursRulerItems(e.NewItems.Cast<ICronRuler>());
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove)
        {
            this.RemoveHoursRulerItems(e.OldItems.Cast<ICronRuler>());
        }
        else if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            this.RemoveHoursRulerItems(e.OldItems.Cast<ICronRuler>());
        }
    }

    private void DaysOfMonthRulerItemCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            this.AddDaysOfMonthRulerItems(e.NewItems.Cast<ICronRuler>());
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove)
        {
            this.RemoveDaysOfMonthRulerItems(e.OldItems.Cast<ICronRuler>());
        }
        else if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            this.RemoveDaysOfMonthRulerItems(e.OldItems.Cast<ICronRuler>());
        }
    }

    private void MonthsRulerItemCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            this.AddMonthsRulerItems(e.NewItems.Cast<ICronRuler>());
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove)
        {
            this.RemoveMonthsRulerItems(e.OldItems.Cast<ICronRuler>());
        }
        else if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            this.RemoveMonthsRulerItems(e.OldItems.Cast<ICronRuler>());
        }
    }

    private void DaysOfWeekRulerItemCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            this.AddDaysOfWeekRulerItems(e.NewItems.Cast<ICronRuler>());
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove)
        {
            this.RemoveDaysOfWeekRulerItems(e.OldItems.Cast<ICronRuler>());
        }
        else if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            this.RemoveDaysOfWeekRulerItems(e.OldItems.Cast<ICronRuler>());
        }
    }

    private void YearsRulerItemCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            this.AddYearsRulerItems(e.NewItems.Cast<ICronRuler>());
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove)
        {
            this.RemoveYearsRulerItems(e.OldItems.Cast<ICronRuler>());
        }
        else if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            this.RemoveYearsRulerItems(e.OldItems.Cast<ICronRuler>());
        }
    }
    
    private void OnCronFieldContentTemplateChanged(AvaloniaPropertyChangedEventArgs args)
    {
        if (args.OldValue is IDataTemplate oldDataTemplate && _fieldsHost is not null && _fieldsHost.DataTemplates.Contains(oldDataTemplate))
        {
            _fieldsHost.DataTemplates.Remove(oldDataTemplate);
        }

        if (args.NewValue is IDataTemplate dataTemplate && _fieldsHost is not null && !_fieldsHost.DataTemplates.Contains(dataTemplate))
        {
            _fieldsHost.DataTemplates.Add(dataTemplate);
        }
    }

    private void OnCronExpressionChanged(string oldValue, string value)
    {
        this.RaiseEvent(new UsualValueChangedEventArgs<string>(CronExpressionChangedEvent, oldValue, value));
    }

    private void RefreshCronExpression()
    {
        CronExpression = CombineCronExpression();
    }

    private string CombineCronExpression()
    {
        CronExpressionStringFormatter format = CronExpressionFormatter ?? new CronExpressionStringFormatter();
        return format.Format(this.Seconds, this.Minutes, this.Hours, this.DaysOfMonth, this.Months, this.DaysOfWeek, this.Years, this.IsSecondEnabled, this.IsYearEnabled);
    }
    
    private void ChangeCronExpression(string? expression)
    {
        CronExpression = expression;
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
        ClearTextBoxErrorTips();
        CronExpressionParseResult result = InnerParsePicker(this.CronExpression);
        this.ShowOperationResult(result.Status, "", result.Message);
        if (result.Status == NotificationType.Success)
        {
            this.CronExpressionCalculationResult = result;
        }
    }

    private CronExpressionParseResult InnerParsePicker(string[] args, string cronString)
    {
        try
        {
            _isCronExpressionParsed = false;
            ThrowIfFieldsNotExists();
            int index_run = -1;
            if (this.IsSecondEnabled)
            {
                try
                {
                    index_run++;
                    _secondsField!.ParsetoValue(args[index_run]); // 秒~解析
                }
                catch (Exception ex)
                {
                    return CronExpressionParseResult.CreateError(InnerCronPartParseErrorFormat(ex.Message, args[index_run], _secondsField!.FieldName));
                }
            }

            try
            {
                index_run++;
                _minutesField!.ParsetoValue(args[index_run]); // 分~解析
            }
            catch (Exception ex)
            {
                return CronExpressionParseResult.CreateError(InnerCronPartParseErrorFormat(ex.Message, args[index_run], _minutesField!.FieldName));
            }

            try
            {
                index_run++;
                _hoursField!.ParsetoValue(args[index_run]); // 时~解析
            }
            catch (Exception ex)
            {
                return CronExpressionParseResult.CreateError(InnerCronPartParseErrorFormat(ex.Message, args[index_run], _hoursField!.FieldName));
            }

            try
            {
                index_run++;
                _daysOfMonthField!.ParsetoValue(args[index_run]); // 日~解析
            }
            catch (Exception ex)
            {
                return CronExpressionParseResult.CreateError(InnerCronPartParseErrorFormat(ex.Message, args[index_run], _daysOfMonthField!.FieldName));
            }

            try
            {
                index_run++;
                _monthsField!.ParsetoValue(args[index_run]); // 月~解析
            }
            catch (Exception ex)
            {
                return CronExpressionParseResult.CreateError(InnerCronPartParseErrorFormat(ex.Message, args[index_run], _monthsField!.FieldName));
            }

            try
            {
                index_run++;
                _daysOfWeekField!.ParsetoValue(args[index_run]); // 周~解析
            }
            catch (Exception ex)
            {
                return CronExpressionParseResult.CreateError(InnerCronPartParseErrorFormat(ex.Message, args[index_run], _daysOfWeekField!.FieldName));
            }

            if (this.IsYearEnabled)
            {
                try
                {
                    index_run++;
                    CronPickerFieldView? field = _yearsField;
                    if (args.Length > index_run)
                    {
                        field!.ParsetoValue(args[index_run]); // 年~解析
                    }
                    else
                    {
                        field!.ParsetoValue(string.Empty); // 年~清空
                    }
                }
                catch (Exception ex)
                {
                    return CronExpressionParseResult.CreateError(InnerCronPartParseErrorFormat(ex.Message, args[index_run], _yearsField!.FieldName));
                }
            }

            // 外部解析器解析并获取指定次数运行时间
            DateTime[] times = GetNextRunTimes(cronString) ?? Enumerable.Empty<DateTime>().ToArray();
            return CronExpressionParseResult.CreateSuccess(times, CronExpressionParsedMessage?.Format());
        }
        catch (Exception exception)
        {
            return CronExpressionParseResult.CreateError(exception.Message);
        }
        finally
        {
            _isCronExpressionParsed = true;
        }
    }

    private CronExpressionParseResult InnerParsePicker(string? cronString)
    {
        string[] args;
        try
        {
            args = ThrowGetCronExpressionParts(cronString);
        }
        catch (Exception e)
        {
            return CronExpressionParseResult.CreateError(e.Message);
        }

        return InnerParsePicker(args, cronString);
    }

    private string[] ThrowGetCronExpressionParts(string? cronString)
    {
        string[]? args = cronString?.Trim().Split(new char[] { PARTS_SEPARATOR }, options: StringSplitOptions.RemoveEmptyEntries);
        int allowLength = 5;
        if (this.IsSecondEnabled)
        {
            allowLength = 6;
        }

        if (args is null || args.Length < allowLength)
        {
            throw new ArgumentException(CronExpressionParamsNotEnoughErrorMessage?.Format());
        }
        
        return args;
    }

    private void TryRefreshRecentRunTimes(string? cronString)
    {
        if (string.IsNullOrWhiteSpace(Minutes) || string.IsNullOrWhiteSpace(Hours) || string.IsNullOrWhiteSpace(DaysOfMonth) || string.IsNullOrWhiteSpace(Months) || string.IsNullOrWhiteSpace(DaysOfWeek)
            || (IsSecondEnabled && string.IsNullOrWhiteSpace(Seconds)) || string.IsNullOrWhiteSpace(cronString))
        {
            return;
        }

        try
        {
            ThrowIfFieldsNotExists();
            if (this.IsSecondEnabled)
            {
                try
                {
                    _secondsField!.VerifyCurrentCronValue(); // 秒~ 合法性验证
                }
                catch (Exception exception)
                {
                    throw new Exception(CronPartVerifyErrorFormat?.Format(("msg", exception.Message), ("part", _secondsField!.FieldName)), exception);
                }
            }

            try
            {
                _secondsField!.VerifyCurrentCronValue(); // 分~ 合法性验证
            }
            catch (Exception exception)
            {
                throw new Exception(CronPartVerifyErrorFormat?.Format(("msg", exception.Message), ("part", _minutesField!.FieldName)), exception);
            }

            try
            {
                _hoursField!.VerifyCurrentCronValue(); // 时~ 合法性验证
            }
            catch (Exception exception)
            {
                throw new Exception(CronPartVerifyErrorFormat?.Format(("msg", exception.Message), ("part", _hoursField!.FieldName)), exception);
            }

            try
            {
                _daysOfMonthField!.VerifyCurrentCronValue(); // 日~ 合法性验证
            }
            catch (Exception exception)
            {
                throw new Exception(CronPartVerifyErrorFormat?.Format(("msg", exception.Message), ("part", _daysOfMonthField!.FieldName)), exception);
            }

            try
            {
                _monthsField!.VerifyCurrentCronValue(); // 月~ 合法性验证
            }
            catch (Exception exception)
            {
                throw new Exception(CronPartVerifyErrorFormat?.Format(("msg", exception.Message), ("part", _monthsField!.FieldName)), exception);
            }

            try
            {
                _daysOfWeekField!.VerifyCurrentCronValue(); // 周~ 合法性验证
            }
            catch (Exception exception)
            {
                throw new Exception(CronPartVerifyErrorFormat?.Format(("msg", exception.Message), ("part", _daysOfWeekField!.FieldName)), exception);
            }

            if (this.IsYearEnabled)
            {
                try
                {
                    _yearsField!.VerifyCurrentCronValue(); // 年~ 合法性验证
                }
                catch (Exception exception)
                {
                    throw new Exception(CronPartVerifyErrorFormat?.Format(("msg", exception.Message), ("part", _yearsField!.FieldName)), exception);
                }
            }
            
            RefreshRecentRunTimes(cronString);
        }
        catch (Exception e)
        {
            this.CronExpressionCalculationResult = CronExpressionParseResult.CreateError(e.Message);
            // this.ShowErrorNotification("Error", e.Message);
        }
    }

    private void RefreshRecentRunTimes(string cronString)
    {
        DateTime[]? times = GetNextRunTimes(cronString);
        if (times is not null)
        {
            this.CronExpressionCalculationResult = CronExpressionParseResult.CreateSuccess(times);
        }
        else
        {
            this.CronExpressionCalculationResult = CronExpressionParseResult.CreateSuccess(Enumerable.Empty<DateTime>());
        }
    }
    
    private DateTime[]? GetNextRunTimes(string cronString)
    {
        ICronExpressionParser? parser = this.CronExpressionParser;
        if (parser is not null)
        {
            DateTime start = DateTime.Now;
            List<DateTime> runTimes = new List<DateTime>();
            for (int i = 0; i < NextRunTimeCount; i++)
            {
                try
                {
                    DateTime nextTime = parser.GetNextTime(cronString, this.IsSecondEnabled, this.IsYearEnabled, start);
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

            return runTimes.ToArray();
        }
        
        return null;
    }

    private IEnumerable<ICronPickerRulersContainer>? GetCronRulersContainerControls()
    {
        if (_secondsField is not null)
        {
            yield return _secondsField;
        }
        if (_minutesField is not null)
        {
            yield return _minutesField;
        }
        if (_hoursField is not null)
        {
            yield return _hoursField;
        }
        if (_daysOfMonthField is not null)
        {
            yield return _daysOfMonthField;
        }
        if (_monthsField is not null)
        {
            yield return _monthsField;
        }
        if (_daysOfWeekField is not null)
        {
            yield return _daysOfWeekField;
        }
        if (_yearsField is not null)
        {
            yield return _yearsField;
        }
    }

    private void ThrowIfFieldsNotExists()
    {
        if (this.IsSecondEnabled && _secondsField is null)
        { // 未提供秒支持部件，请联系管理员
            throw new Exception(NotFindSecondFieldMessage?.Format());
        }

        if (_minutesField is null)
        { // 未提供分支持部件，请联系管理员
            throw new Exception(NotFindMinuteFieldMessage?.Format());
        }
            
        if (_hoursField is null)
        { // 未提供时支持部件，请联系管理员
            throw new Exception(NotFindHourFieldMessage?.Format());
        }

        if (_daysOfMonthField is null)
        { // 未提供日支持部件，请联系管理员
            throw new Exception(NotFindDaysOfMonthFieldMessage?.Format());
        }

        if (_monthsField is null)
        { // 未提供月支持部件，请联系管理员
            throw new Exception(NotFindMonthsFieldMessage?.Format());
        }

        if (_daysOfWeekField is null)
        { // 未提供周支持部件，请联系管理员
            throw new Exception(NotFindDaysOfWeekFieldMessage?.Format());
        }
        
        if (this.IsYearEnabled && _yearsField is null)
        { // 未提供年支持部件，请联系管理员
            throw new Exception(NotFindYearsFieldMessage?.Format());
        }
    }
    
    private string InnerCronPartParseErrorFormat(string? msg, string? source, string? part)
    {
        return CronPartExpressionParseErrorFormat?.Format((nameof(msg), msg), (nameof(source), source), (nameof(part), part));
    }
}

public class CronExpressionParseToTimesResult : CronExpressionParseResult
{
    public CronExpressionParseToTimesResult(IEnumerable<DateTime> runTimes, string? message = null) : base(NotificationType.Success, message)
    {
        this.RunTimes = runTimes;
    }

    public IEnumerable<DateTime> RunTimes { get; }
}

public class CronExpressionParseResult
{
    protected CronExpressionParseResult(NotificationType status, string? message)
    {
        Status = status;
        Message = message;
    }

    public NotificationType Status { get; }

    public string? Message { get; }

    public static CronExpressionParseResult CreateSuccess(IEnumerable<DateTime> runTimes)
    {
        return new CronExpressionParseToTimesResult(runTimes);
    }

    public static CronExpressionParseResult CreateSuccess(IEnumerable<DateTime> runTimes, string message)
    {
        return new CronExpressionParseToTimesResult(runTimes, message);
    }

    public static CronExpressionParseResult CreateError(string message)
    {
        return new CronExpressionParseResult(NotificationType.Error, message);
    }
    
    public static CronExpressionParseResult CreateWarning(string message)
    {
        return new CronExpressionParseResult(NotificationType.Warning, message);
    }
}

public class CronPickerFieldsContainer : TabControl, ICronPickerFieldsContainer
{
    public IEnumerable<ICronPickerRulersContainer> GetCronPickerFields()
    {
        return this.LogicalChildren.OfType<ICronPickerRulersContainer>();
    }

    public void Select(int index)
    {
        this.SelectedIndex = index;
    }

    protected override void LogicalChildrenCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        base.LogicalChildrenCollectionChanged(sender, e);
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            AddControlItemsToLogicalChildren(e.NewItems);
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove)
        {
            RemoveControlItemsFromLogicalChildren(e.OldItems);
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        this.UpdateTabItemContents();
    }
    
    private void AddControlItemsToLogicalChildren(IList items)
    {
        int old = this.SelectedIndex;
        foreach (object? item in items)
        {
            if (item is TabItem tabItem)
            {
                tabItem.IsSelected = true;
            }
        }
        
        this.SelectedIndex = old;
    }
    
    private void RemoveControlItemsFromLogicalChildren(IList items)
    {
    }

    private void UpdateTabItemContents()
    {
        TabItem[] tabItems = this.LogicalChildren.OfType<TabItem>().ToArray();
        if (tabItems == null)
            return;
        foreach (TabItem item in tabItems)
        {
            item.IsSelected = true;
        }
        
        this.SelectedIndex = 0;
    }

    private ContentPresenter CreateContentPresenter(TabItem tabItem)
    {
        ContentPresenter contentPresenter = CreateContentPresenter();
        IDisposable disposable = tabItem.GetObservable<object>((AvaloniaProperty<object>)ContentControl.ContentProperty).Subscribe<object>((Action<object>)(v =>
        {
            try
            {
                contentPresenter.Content = v;
            }
            catch (Exception e)
            {
                throw;
            }
        }));
        IDisposable disposable2 = tabItem.GetObservable<IDataTemplate>((AvaloniaProperty<IDataTemplate>)ContentControl.ContentTemplateProperty).Subscribe<IDataTemplate>((Action<IDataTemplate>)(v =>
        {
            try
            {
                contentPresenter.ContentTemplate = v ?? this.ContentTemplate;
            }
            catch (Exception e)
            {
                throw;
            }
        }));
        return contentPresenter;
    }

    private ContentPresenter CreateContentPresenter()
    {
        return new ContentPresenter();
    }

    private void AddTabContent(TabItem tabItem)
    {
    }
}