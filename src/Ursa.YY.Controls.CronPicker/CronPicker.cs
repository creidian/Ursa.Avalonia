using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Metadata;
using Irihi.Avalonia.Shared.Contracts;
using Irihi.Avalonia.Shared.Helpers;

namespace Ursa.Controls;

[TemplatePart(PART_Button, typeof(Button))]
[TemplatePart(PART_EditorOKButton, typeof(Button))]
[TemplatePart(PART_EditorCancelButton, typeof(Button))]
[TemplatePart(PART_Popup, typeof(Popup))]
[TemplatePart(PART_TextBox, typeof(TextBox))]
[TemplatePart(PART_CronExpressionView, typeof(CronExpressionEditor))]
public class CronPicker: CronPickerBase, IClearControl
{
    public const string PART_Button = "PART_Button";
    public const string PART_Popup = "PART_Popup";
    public const string PART_TextBox = "PART_TextBox";
    public const string PART_CronExpressionView = "PART_CronExpressionView";
    public const string PART_EditorCancelButton = "PART_EditorCancelButton";
    public const string PART_EditorOKButton = "PART_EditorOKButton";
    public const string DefaultCronExpression = "* * * * *";
    public const string DefaultCronExpressionWithSeconds = "* * * * * *";
    
    public static readonly StyledProperty<string?> ExpressionStringProperty = AvaloniaProperty.Register<CronPicker, string?>(nameof(ExpressionString));
    public static readonly StyledProperty<string?> WatermarkProperty = AvaloniaProperty.Register<CronPicker, string?>(nameof(Watermark));
    public static readonly StyledProperty<string> DefaultExpressionStringProperty = AvaloniaProperty.Register<CronPicker, string>(nameof(DefaultExpressionString), defaultValue: "* * * * *");
    public static readonly StyledProperty<bool> IsSecondEnabledProperty = AvaloniaProperty.Register<CronPicker, bool>(nameof(IsSecondEnabled), defaultValue: true);
    public static readonly StyledProperty<bool> IsYearEnabledProperty = AvaloniaProperty.Register<CronPicker, bool>(nameof(IsYearEnabled), defaultValue: true);
    public static readonly StyledProperty<ICronExpressionParser?> CronExpressionParserProperty = AvaloniaProperty.Register<CronPicker, ICronExpressionParser?>(nameof(CronExpressionParser));
    public static readonly StyledProperty<IEnumerable<CronPickerRuler>?> SecondsRulerItemsSourceProperty = AvaloniaProperty.Register<CronPicker, IEnumerable<CronPickerRuler>?>(nameof(SecondsRulerItemsSource));
    public static readonly StyledProperty<IEnumerable<CronPickerRuler>?> MinutesRulerItemsSourceProperty = AvaloniaProperty.Register<CronPicker, IEnumerable<CronPickerRuler>?>(nameof(MinutesRulerItemsSource));
    public static readonly StyledProperty<IEnumerable<CronPickerRuler>?> HoursRulerItemsSourceProperty = AvaloniaProperty.Register<CronPicker, IEnumerable<CronPickerRuler>?>(nameof(HoursRulerItemsSource));
    public static readonly StyledProperty<IEnumerable<CronPickerRuler>?> DaysOfMonthRulerItemsSourceProperty = AvaloniaProperty.Register<CronPicker, IEnumerable<CronPickerRuler>?>(nameof(DaysOfMonthRulerItemsSource));
    public static readonly StyledProperty<IEnumerable<CronPickerRuler>?> MonthsRulerItemsSourceProperty = AvaloniaProperty.Register<CronPicker, IEnumerable<CronPickerRuler>?>(nameof(MonthsRulerItemsSource));
    public static readonly StyledProperty<IEnumerable<CronPickerRuler>?> DaysOfWeekRulerItemsSourceProperty = AvaloniaProperty.Register<CronPicker, IEnumerable<CronPickerRuler>?>(nameof(DaysOfWeekRulerItemsSource));
    public static readonly StyledProperty<IEnumerable<CronPickerRuler>?> YearsRulerItemsSourceProperty = AvaloniaProperty.Register<CronPicker, IEnumerable<CronPickerRuler>?>(nameof(YearsRulerItemsSource));
    public static readonly StyledProperty<DataTemplates?> CronRulerItemDataTemplatesProperty = AvaloniaProperty.Register<CronPicker, DataTemplates?>(nameof(CronRulerItemDataTemplates));
    public static readonly StyledProperty<string?> TextProperty = AvaloniaProperty.Register<CronPicker, string>(nameof (Text), defaultBindingMode: BindingMode.TwoWay, enableDataValidation: true);
    public static readonly StyledProperty<CronExpressionInputVerifySteps> VerifyStepsProperty = AvaloniaProperty.Register<CronPicker, CronExpressionInputVerifySteps>(nameof(VerifySteps));
    public static readonly StyledProperty<CronExpressionParseResult?> CronExpressionCalculationResultProperty = AvaloniaProperty.Register<CronPicker, CronExpressionParseResult?>(nameof(CronExpressionCalculationResult));
    public static readonly StyledProperty<int> NextRunTimeCountProperty = AvaloniaProperty.Register<CronPicker, int>(nameof(NextRunTimeCount), defaultValue: 10);
    public static readonly StyledProperty<string?> DateTimeFormatProperty = AvaloniaProperty.Register<CronPicker, string?>(nameof(DateTimeFormat), defaultValue: "yyyy-MM-dd HH:mm:ss");

    private Button? _button;
    private TextBox? _textBox;
    private CronExpressionEditor? _cronPickerView;
    private Popup? _popup;
    private bool _isFocused;
    private Button? _button_eok;
    private Button? _button_ecancel;

    /// <summary>
    /// The expression string property.默认："* * * * *"
    /// </summary>
    public string DefaultExpressionString
    {
        get => GetValue(DefaultExpressionStringProperty);
        set => SetValue(DefaultExpressionStringProperty, value);
    }

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
    
    public string? ExpressionString
    {
        get => GetValue(ExpressionStringProperty);
        set => SetValue(ExpressionStringProperty, value);
    }

    public string? Watermark
    {
        get => GetValue(WatermarkProperty);
        set => SetValue(WatermarkProperty, value);
    }

    /// <summary>
    /// Whether the second field is enabled.
    /// </summary>
    public bool IsSecondEnabled
    {
        get => GetValue(IsSecondEnabledProperty);
        set => SetValue(IsSecondEnabledProperty, value);
    }

    /// <summary>
    /// Whether the year field is enabled.
    /// </summary>
    public bool IsYearEnabled
    {
        get => GetValue(IsYearEnabledProperty);
        set => SetValue(IsYearEnabledProperty, value);
    }

    /// <summary>
    /// The cron expression parser.
    /// </summary>
    public ICronExpressionParser? CronExpressionParser
    {
        get => GetValue(CronExpressionParserProperty);
        set => SetValue(CronExpressionParserProperty, value);
    }

    public IEnumerable<CronPickerRuler>? SecondsRulerItemsSource
    {
        get => GetValue(SecondsRulerItemsSourceProperty);
        set => SetValue(SecondsRulerItemsSourceProperty, value);
    }
    
    public IEnumerable<CronPickerRuler>? MinutesRulerItemsSource
    {
        get => GetValue(MinutesRulerItemsSourceProperty);
        set => SetValue(MinutesRulerItemsSourceProperty, value);
    }
    
    public IEnumerable<CronPickerRuler>? HoursRulerItemsSource
    {
        get => GetValue(HoursRulerItemsSourceProperty);
        set => SetValue(HoursRulerItemsSourceProperty, value);
    }
    
    public IEnumerable<CronPickerRuler>? DaysOfMonthRulerItemsSource
    {
        get => GetValue(DaysOfMonthRulerItemsSourceProperty);
        set => SetValue(DaysOfMonthRulerItemsSourceProperty, value);
    }
    
    public IEnumerable<CronPickerRuler>? MonthsRulerItemsSource
    {
        get => GetValue(MonthsRulerItemsSourceProperty);
        set => SetValue(MonthsRulerItemsSourceProperty, value);
    }
    
    public IEnumerable<CronPickerRuler>? DaysOfWeekRulerItemsSource
    {
        get => GetValue(DaysOfWeekRulerItemsSourceProperty);
        set => SetValue(DaysOfWeekRulerItemsSourceProperty, value);
    }
    
    public IEnumerable<CronPickerRuler>? YearsRulerItemsSource
    {
        get => GetValue(YearsRulerItemsSourceProperty);
        set => SetValue(YearsRulerItemsSourceProperty, value);
    }

    public DataTemplates? CronRulerItemDataTemplates
    {
        get => this.GetValue(CronRulerItemDataTemplatesProperty);
        set => this.SetValue(CronRulerItemDataTemplatesProperty, value);
    }

    public CronExpressionInputVerifySteps VerifySteps 
    {
        get => this.GetValue(VerifyStepsProperty);
        set => this.SetValue(VerifyStepsProperty, value);
    }
    
    public CronExpressionParseResult? CronExpressionCalculationResult
    {
        get => GetValue(CronExpressionCalculationResultProperty);
        private set => SetValue(CronExpressionCalculationResultProperty, value);
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
    /// The format of the date time.
    /// </summary>
    public string? DateTimeFormat
    {
        get => GetValue(DateTimeFormatProperty);
        set => SetValue(DateTimeFormatProperty, value);
    }

    static CronPicker()
    {
        FocusableProperty.OverrideDefaultValue<CronPicker>(true);
        ExpressionStringProperty.Changed.AddClassHandler<CronPicker, string?>((picker, args) => picker.OnExpressionStringChanged(args));
        TextProperty.Changed.Subscribe(new Action<AvaloniaPropertyChangedEventArgs<string>>(OnTextChanged));
    }

    public void Clear()
    {
        SetCurrentValue(ExpressionStringProperty, null);
        SetCurrentValue(VerifyStepsProperty, CronExpressionInputVerifySteps.None);
    }

    public void Enter()
    {
        SetCurrentValue(IsDropdownOpenProperty, false);
        CommitInput(false);
    }
    
    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        GotFocusEvent.RemoveHandler(OnTextBoxGetFocus, _textBox);
        TextBox.TextChangedEvent.RemoveHandler(OnTextChanged, _textBox);
        Button.ClickEvent.RemoveHandler(OnButtonClick, _button);
        Button.ClickEvent.RemoveHandler(OnButtonClick, _button_eok);
        Button.ClickEvent.RemoveHandler(OnButtonClick, _button_ecancel);
        CronExpressionEditor.CronExpressionChangedEvent.RemoveHandler(OnCronExpressionChanged, _cronPickerView);
        _button = e.NameScope.Find<Button>(PART_Button);
        _button_eok = e.NameScope.Find<Button>(PART_EditorOKButton);
        _button_ecancel = e.NameScope.Find<Button>(PART_EditorCancelButton);
        _popup = e.NameScope.Find<Popup>(PART_Popup);
        _textBox = e.NameScope.Find<TextBox>(PART_TextBox);
        if (this._textBox != null)
        {
            this._textBox.Text = this.Text;
        }
        _cronPickerView = e.NameScope.Find<CronExpressionEditor>(PART_CronExpressionView);
        Button.ClickEvent.AddHandler(OnButtonClick, RoutingStrategies.Bubble, false, _button);
        Button.ClickEvent.AddHandler(OnButtonClick, RoutingStrategies.Bubble, false, _button_eok);
        Button.ClickEvent.AddHandler(OnButtonClick, RoutingStrategies.Bubble, false, _button_ecancel);
        GotFocusEvent.AddHandler(OnTextBoxGetFocus, _textBox);
        TextBox.TextChangedEvent.AddHandler(OnTextChanged, _textBox);
        CronExpressionEditor.CronExpressionChangedEvent.AddHandler(OnCronExpressionChanged, RoutingStrategies.Bubble, true, _cronPickerView);
        SyncExpressionStringTo(ExpressionString);
    }

    /// <inheritdoc/>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if(!e.Handled && e.Source is Visual source)
        {
            if (_popup?.IsInsidePopup(source) == true)
            {
                e.Handled = true;
            }
        }
    }

    /// <inheritdoc/>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Escape:
                SetCurrentValue(IsDropdownOpenProperty, false);
                e.Handled = true;
                return;
            case Key.Down:
                SetCurrentValue(IsDropdownOpenProperty, true);
                e.Handled = true;
                return;
            case Key.Tab:
                SetCurrentValue(IsDropdownOpenProperty, false);
                return;
            case Key.Enter:
            {
                Enter();
                e.Handled = true;
                return;
            }
            default:
                base.OnKeyDown(e);
                break;
        }
    }

    /// <inheritdoc/>
    protected override void UpdateDataValidation(AvaloniaProperty property, BindingValueType state, Exception? error)
    {
        base.UpdateDataValidation(property, state, error);
        if (property == ExpressionStringProperty || property == TextProperty)
        {
            DataValidationErrors.SetError(this, error);
        }
    }

    /// <inheritdoc/>
    protected override void OnGotFocus(GotFocusEventArgs e)
    {
        base.OnGotFocus(e);
        FocusChanged(IsKeyboardFocusWithin);
    }

    /// <inheritdoc/>
    protected override void OnLostFocus(RoutedEventArgs e)
    {
        base.OnLostFocus(e);
        FocusChanged(IsKeyboardFocusWithin);
        var top = TopLevel.GetTopLevel(this);
        var element = top?.FocusManager?.GetFocusedElement();
        if (element is Visual v && _popup?.IsInsidePopup(v) == true)return;
        if (Equals(element, _textBox))return;
        // CommitInput(false);
        SetCurrentValue(IsDropdownOpenProperty, false);
    }
    
    protected virtual void OnTextChanged(string? oldValue, string? newValue)
    {
        if (IsInitialized)
        {
        }
    }
    
    private static void OnTextChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (!(e.Sender is CronPicker sender))
            return;
        string oldValue = (string) e.OldValue;
        string newValue = (string) e.NewValue;
        sender.OnTextChanged(oldValue, newValue);
    }
    
    private void OnExpressionStringChanged(AvaloniaPropertyChangedEventArgs<string?> args)
    {
        SyncExpressionStringTo(args.NewValue.Value);
    }

    private void OnCronExpressionChanged(object? sender, UsualValueChangedEventArgs<string> e)
    {
        // SetCurrentValue(ExpressionStringProperty, e.NewValue);
    }

    private void OnButtonClick(object? sender, RoutedEventArgs e)
    {
        if(sender == _button && IsFocused)
        {
            SetCurrentValue(IsDropdownOpenProperty, !IsDropdownOpen);
            return;
        }
        
        if(sender == _button_eok)
        {
            ReadEditorValueToLocal();
            SetCurrentValue(IsDropdownOpenProperty, false);
            return;
        }
        
        if(sender == _button_ecancel)
        {
            SetCurrentValue(IsDropdownOpenProperty, false);
            return;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        // SetExpressionString(true);
        if (_cronPickerView is not null)
        {
            if (_cronPickerView.CronExpression == _textBox?.Text && VerifySteps == CronExpressionInputVerifySteps.VerifyToSuccessed)
            {
                return;
            }
            
            CronExpressionCalculationResult = null;
            if (_cronPickerView.AllowTryParse(_textBox?.Text))
            {
                SetCurrentValue(VerifyStepsProperty, CronExpressionInputVerifySteps.InputToVerifyAllowed);
            }
            else
            {
                SetCurrentValue(VerifyStepsProperty, CronExpressionInputVerifySteps.Input);
            }
        }
        else
        {
            CronExpressionCalculationResult = null;
            SetCurrentValue(VerifyStepsProperty, CronExpressionInputVerifySteps.Input);
        }
    }

    private void SyncExpressionStringTo(string? expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            _textBox?.SetValue(TextBox.TextProperty, null);
            // _cronPickerView?.Clear();
        }
        else
        {
            _textBox?.SetValue(TextBox.TextProperty, expression);
            // TryRefreshExpression(expression, out _);
        }
    }

    private void SetExpressionString(bool fromText = false)
    {
        string? text = _textBox?.Text;
        if (string.IsNullOrWhiteSpace(text))
        {
            SetCurrentValue(ExpressionStringProperty, null);
            // _cronPickerView?.Clear();
        }
        else
        {
            SetCurrentValue(ExpressionStringProperty, text);
            // TryRefreshExpression(text, out _);
        }
    }

    private void OnTextBoxGetFocus(object? sender, GotFocusEventArgs e)
    {
        if (!GetValue(IsDropdownOpenProperty))
        {
            SetCurrentValue(IsDropdownOpenProperty, true);
            if (!String.IsNullOrWhiteSpace(ExpressionString))
            {
                // TryRefreshExpression(ExpressionString, out _);
            }
            else
            {
                // TryRefreshExpression(DefaultExpressionString, out _);
            }
        }
    }
    
    private void FocusChanged(bool hasFocus)
    {
        bool wasFocused = _isFocused;
        _isFocused = hasFocus;
        if (hasFocus)
        {
            if (!wasFocused && _textBox != null)
            {
                _textBox.Focus();
            }
        }
    }

    private void ReadEditorValueToLocal()
    {
        if (_cronPickerView is not null)
        {
            string? expressionString = _cronPickerView.CronExpression;
            CronExpressionParseResult? result = _cronPickerView.CronExpressionCalculationResult;
            if (result is not null)
            {
                if (result.Status == NotificationType.Success)
                {
                    CronExpressionCalculationResult = result;
                    SetCurrentValue(VerifyStepsProperty, CronExpressionInputVerifySteps.VerifyToSuccessed);
                }
                else
                {
                    CronExpressionCalculationResult = null;
                    SetError(result.Message);
                    SetCurrentValue(VerifyStepsProperty, CronExpressionInputVerifySteps.VerifyError);
                }
            }
            else
            {
                CronExpressionCalculationResult = null;
                SetCurrentValue(VerifyStepsProperty, CronExpressionInputVerifySteps.None);
            }
            
            if (string.IsNullOrWhiteSpace(expressionString))
            {
                SetCurrentValue(ExpressionStringProperty, null);
                this._textBox?.SetCurrentValue(TextBox.TextProperty, null);
            }
            else
            {
                SetCurrentValue(ExpressionStringProperty, expressionString);
                this._textBox?.SetCurrentValue(TextBox.TextProperty, expressionString);
            }
        }
    }

    private void CommitInput(bool clearWhenInvalid)
    {
        CronExpressionCalculationResult = null;
        SetCurrentValue(VerifyStepsProperty, CronExpressionInputVerifySteps.Verify);
        if (TryRefreshExpression(_textBox?.Text, out string expressionString))
        {
            SetCurrentValue(ExpressionStringProperty, _textBox?.Text);
            SetCurrentValue(VerifyStepsProperty, CronExpressionInputVerifySteps.VerifyToSuccessed);
        }
        else
        {
            SetCurrentValue(VerifyStepsProperty, CronExpressionInputVerifySteps.VerifyError);
            if (clearWhenInvalid)
            {
                SetCurrentValue(ExpressionStringProperty, null);
                _cronPickerView?.Clear();
            }
        }
    }

    private bool TryRefreshExpression(string? expression, out string expressionString)
    {
        expressionString = string.Empty;
        if (_cronPickerView is not null)
        {
            ClearError();
            try
            {
                Console.WriteLine($"expression: {expression}");
                CronExpressionParseResult? result = _cronPickerView.ParseCronExpressionBy(expression);
                if (result is not null)
                {
                    if (result.Status == NotificationType.Error || result.Status == NotificationType.Warning)
                    {
                        SetError(result.Message);
                        expressionString = expression;
                    }
                    else
                    {
                        expressionString = _cronPickerView.CronExpression;
                        ClearError();
                        CronExpressionCalculationResult = _cronPickerView.CronExpressionCalculationResult;
                    }
                }
                else
                {
                    expressionString = expression;
                }

                return result?.Status == NotificationType.Success;
            }
            catch (Exception ex)
            {
                SetError(ex.Message);
                return false;
            }
        }

        expressionString = expression;
        return true;
    }

    private void ClearError()
    {
        DataValidationErrors.ClearErrors(this);
    }

    private void SetError(string message)
    {
        DataValidationErrors.SetErrors(this, new string[] { message });
    }
}

public class CronPickerBase : TemplatedControl, IInnerContentControl, IPopupInnerContent
{
    public static readonly StyledProperty<CronExpressionStringFormatter?> CronExpressionFormatterProperty = AvaloniaProperty.Register<CronPickerBase, CronExpressionStringFormatter?>(nameof(CronExpressionFormatter));

    public static readonly StyledProperty<object?> InnerLeftContentProperty = AvaloniaProperty.Register<CronPickerBase, object?>(nameof(InnerLeftContent));

    public static readonly StyledProperty<object?> InnerRightContentProperty = AvaloniaProperty.Register<CronPickerBase, object?>(nameof(InnerRightContent));
    
    public static readonly StyledProperty<object?> PopupInnerTopContentProperty = AvaloniaProperty.Register<CronPickerBase, object?>(nameof(PopupInnerTopContent));

    public static readonly StyledProperty<object?> PopupInnerBottomContentProperty = AvaloniaProperty.Register<CronPickerBase, object?>(nameof(PopupInnerBottomContent));

    public static readonly StyledProperty<bool> IsDropdownOpenProperty = AvaloniaProperty.Register<CronPickerBase, bool>(nameof(IsDropdownOpen), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<bool> IsReadonlyProperty = AvaloniaProperty.Register<CronPickerBase, bool>(nameof(IsReadonly));

    public bool IsReadonly
    {
        get => GetValue(IsReadonlyProperty);
        set => SetValue(IsReadonlyProperty, value);
    }

    public bool IsDropdownOpen
    {
        get => GetValue(IsDropdownOpenProperty);
        set => SetValue(IsDropdownOpenProperty, value);
    }

    public object? InnerLeftContent
    {
        get => GetValue(InnerLeftContentProperty);
        set => SetValue(InnerLeftContentProperty, value);
    }

    public object? InnerRightContent
    {
        get => GetValue(InnerRightContentProperty);
        set => SetValue(InnerRightContentProperty, value);
    }

    public object? PopupInnerTopContent
    {
        get => GetValue(PopupInnerTopContentProperty);
        set => SetValue(PopupInnerTopContentProperty, value);
    }

    public object? PopupInnerBottomContent
    {
        get => GetValue(PopupInnerBottomContentProperty);
        set => SetValue(PopupInnerBottomContentProperty, value);
    }
    
    public CronExpressionStringFormatter? CronExpressionFormatter
    {
        get => GetValue(CronExpressionFormatterProperty);
        set => SetValue(CronExpressionFormatterProperty, value);
    }
}

public class CronExpressionStringFormatter : AvaloniaObject
{
    public static readonly StyledProperty<string> DefaultFormatProperty = AvaloniaProperty.Register<CronExpressionStringFormatter, string>(nameof(DefaultFormat), defaultValue:"{0} {1} {2} {3} {4}");
    public static readonly StyledProperty<string> WithSecondsFormatProperty = AvaloniaProperty.Register<CronExpressionStringFormatter, string>(nameof(WithSecondsFormat), defaultValue:"{0} {1} {2} {3} {4} {5}");
    public static readonly StyledProperty<string> WithYearsFormatProperty = AvaloniaProperty.Register<CronExpressionStringFormatter, string>(nameof(WithYearsFormat), defaultValue:"{0} {1} {2} {3} {4} {5}");
    public static readonly StyledProperty<string> WithSecondsAndYearsFormatProperty = AvaloniaProperty.Register<CronExpressionStringFormatter, string>(nameof(WithSecondsAndYearsFormat), defaultValue:"{0} {1} {2} {3} {4} {5} {6}");
    public static readonly StyledProperty<IStringFormatter?> StringFormatterProperty = AvaloniaProperty.Register<CronExpressionStringFormatter, IStringFormatter?>(nameof(StringFormatter));
    public static readonly StyledProperty<bool> IsSimpleStringFormatModeProperty = AvaloniaProperty.Register<CronExpressionStringFormatter, bool>(nameof(IsSimpleStringFormatMode));
    public static readonly StyledProperty<ParameterReplaceRulers> ParameterReplaceRulersProperty = AvaloniaProperty.Register<CronExpressionStringFormatter, ParameterReplaceRulers>(nameof(ParameterReplaceRulers));

    /// <summary>
    /// 默认格式：分  时  日  月  周
    /// </summary>
    public string DefaultFormat { get => GetValue(DefaultFormatProperty); set => SetValue(DefaultFormatProperty, value); }
    
    /// <summary>
    /// 包含秒的格式：秒 分  时  秒  日  月  周
    /// </summary>
    public string WithSecondsFormat { get => GetValue(WithSecondsFormatProperty); set => SetValue(WithSecondsFormatProperty, value); }
    
    /// <summary>
    /// 包含年份的格式：分  时  日  月  周  年
    /// </summary>
    public string WithYearsFormat { get => GetValue(WithYearsFormatProperty); set => SetValue(WithYearsFormatProperty, value); }
    
    /// <summary>
    /// 包含秒和年份的格式：秒  分  时  秒  日  月  周  年
    /// </summary>
    public string WithSecondsAndYearsFormat { get => GetValue(WithSecondsAndYearsFormatProperty); set => SetValue(WithSecondsAndYearsFormatProperty, value); }

    public IStringFormatter? StringFormatter { get => GetValue(StringFormatterProperty); set => SetValue(StringFormatterProperty, value); }
    
    [Content]
    public ParameterReplaceRulers ParameterReplaceRulers { get => GetValue(ParameterReplaceRulersProperty); set => SetValue(ParameterReplaceRulersProperty, value); }

    /// <summary>
    /// 是否使用简单字符串格式化模式
    /// </summary>
    public bool IsSimpleStringFormatMode { get => GetValue(IsSimpleStringFormatModeProperty); set => SetValue(IsSimpleStringFormatModeProperty, value); }

    public string Format(string secondes, string minutes, string hours, string days, string months, string weekdays, string? years, bool withSeconds, bool withYears)
    {
        string format = DefaultFormat;
        List<(string Key, object? Value)> args = new List<(string Key, object? Value)>()
        {
            ("s", secondes),
            ("m", minutes),
            ("h", hours),
            ("d", days),
            ("M", months),
            ("w", weekdays),
            ("y", years)
        };
        if (withSeconds && withYears)
        {
            if (!String.IsNullOrWhiteSpace(years))
            {
                format = WithSecondsAndYearsFormat;
            }
            else
            {
                format = WithSecondsFormat;
                args.RemoveAt(args.Count - 1);
            }
        }
        else if (withSeconds)
        {
            format = WithSecondsFormat;
            args.RemoveAt(args.Count - 1);
        }
        else if (withYears && !string.IsNullOrWhiteSpace(years))
        {
            format = WithYearsFormat;
            args.RemoveAt(0);
        }
        else
        {
            args.RemoveAt(args.Count - 1);
            args.RemoveAt(0);
        }
        
        return Format(format, args.ToArray());
    }

    private string Format(string format, params (string Key, object? Value)[] args)
    {
        if (string.IsNullOrWhiteSpace(format))
        {
            return string.Empty;
        }

        if (args == null || args.Length == 0)
        {
            return format;
        }

        if (IsSimpleStringFormatMode || StringFormatter == null)
        {
            return string.Format(format, args.Select(x => x.Value).ToArray());
        }
        
        return StringFormatter.Format(format, ParameterReplaceRulers.ToImmutable(), args);
    }
}

public enum CronExpressionInputVerifySteps
{
    None,
    Input,
    InputToVerifyAllowed,
    Verify,
    VerifyToSuccessed,
    VerifyError,
}