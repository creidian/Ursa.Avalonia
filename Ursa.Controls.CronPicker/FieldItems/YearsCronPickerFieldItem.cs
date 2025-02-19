using Avalonia;
using Avalonia.Data;

namespace Ursa.Controls;

/// <summary>
/// A control that represents the years field in a cron expression.
/// </summary>
public class YearsCronPickerFieldItem : CronPickerFieldItem
{
    public static readonly DirectProperty<YearsCronPickerFieldItem, bool> IsPlaceholderModeProperty = AvaloniaProperty.RegisterDirect<YearsCronPickerFieldItem, bool>(nameof(IsPlaceholderMode), o => o.IsPlaceholderMode, (o, v) => o.IsPlaceholderMode = v);
    public static readonly DirectProperty<YearsCronPickerFieldItem, bool> IsEveryYearModeProperty = AvaloniaProperty.RegisterDirect<YearsCronPickerFieldItem, bool>(nameof(IsEveryYearMode), o => o.IsEveryYearMode, (o, v) => o.IsEveryYearMode = v);
    public static readonly DirectProperty<YearsCronPickerFieldItem, bool> IsRangeLimitModeProperty = AvaloniaProperty.RegisterDirect<YearsCronPickerFieldItem, bool>(nameof(IsRangeLimitMode), o => o.IsRangeLimitMode, (o, v) => o.IsRangeLimitMode = v);
    public static readonly StyledProperty<int> RangeStartProperty = AvaloniaProperty.Register<YearsCronPickerFieldItem, int>(nameof(RangeStart), defaultBindingMode: BindingMode.TwoWay);
    public static readonly StyledProperty<int> RangeEndProperty = AvaloniaProperty.Register<YearsCronPickerFieldItem, int>(nameof(RangeEnd), defaultBindingMode: BindingMode.TwoWay);
    
    public YearsCronPickerFieldItem()
    {
        IsAllowSpecialValue = true; // 允许指定值
    }

    /// <summary>
    /// Whether the field is in placeholder mode.
    /// </summary>
    public bool IsPlaceholderMode
    {
        get => IsModeSelected(1);
        private set => SwitchCurrentModeTo(IsPlaceholderModeProperty, 1, value);
    }

    /// <summary>
    /// Whether the field is in every year mode.
    /// </summary>
    public bool IsEveryYearMode
    {
        get => IsModeSelected(2);
        private set => SwitchCurrentModeTo(IsEveryYearModeProperty, 2, value);
    }

    /// <summary>
    /// Whether the field is in range limit mode.
    /// </summary>
    public bool IsRangeLimitMode
    {
        get => IsModeSelected(3);
        private set => SwitchCurrentModeTo(IsRangeLimitModeProperty, 3, value);
    }

    /// <summary>
    /// The start of the range.
    /// </summary>
    public int RangeStart
    {
        get => GetValue(RangeStartProperty);
        set => SetValue(RangeStartProperty, value);
    }
    
    /// <summary>
    /// The end of the range.
    /// </summary>
    public int RangeEnd
    {
        get => GetValue(RangeEndProperty);
        set => SetValue(RangeEndProperty, value);
    }

    protected override string DictKey { get; } = nameof(YearsCronPickerFieldItem);

    public override void ParsetoValue(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            this.IsPlaceholderMode = true;
            return;
        }
        
        base.ParsetoValue(text);
    }

    protected override void OnCurrentModeSelected(AvaloniaProperty<bool> property)
    {
        if (property == IsPlaceholderModeProperty)
        {
            this.IsAllowSpecialValue = false;
        }
        else if (property == IsEveryYearModeProperty)
        {
            this.IsAllowSpecialValue = true;
            this.SetValueAnyMode();
        }
        else if (property == IsRangeLimitModeProperty)
        {
            this.IsAllowSpecialValue = true;
            this.SetValueRange(this.RangeStart, this.RangeEnd);
        }
    }

    protected override void UnSafeChangeCurrentModeToRange(int leftParam, int rightParam)
    {
        ThrowIfRangeValueInvalid(leftParam, rightParam);
        RangeStart = leftParam;
        RangeEnd = rightParam;
        IsRangeLimitMode = true;
    }

    protected override void ChangeCurrentModeToAnyMode() => this.IsEveryYearMode = true;

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == RangeStartProperty || change.Property == RangeEndProperty)
        {
            if (IsRangeLimitMode)
            {
                this.SetValueRange(this.RangeStart, this.RangeEnd);
            }
        }
    }
    
    protected override void ThrowIfRangeValueInvalid(int start, int end)
    {
        if (start < DateTime.MinValue.Year || start > DateTime.MaxValue.Year)
        {
            throw GetParseException_RangeStartOrEndOutOfRange(DateTime.MinValue.Year, DateTime.MaxValue.Year);
        }

        if (start > end)
        {
            throw GetParseException_RangeEndLessThenStart();
        }
    }
}