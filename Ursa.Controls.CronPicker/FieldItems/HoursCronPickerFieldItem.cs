using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Data;

namespace Ursa.Controls;

/// <summary>
/// A control that represents the hours field in a cron expression.
/// </summary>
public class HoursCronPickerFieldItem : CronPickerFieldItem
{
    public static readonly DirectProperty<HoursCronPickerFieldItem, bool> IsEveryHourModeProperty = AvaloniaProperty.RegisterDirect<HoursCronPickerFieldItem, bool>(nameof(IsEveryHourMode), o => o.IsEveryHourMode, (o, v) => o.IsEveryHourMode = v);
    public static readonly DirectProperty<HoursCronPickerFieldItem, bool> IsRangeLimitModeProperty = AvaloniaProperty.RegisterDirect<HoursCronPickerFieldItem, bool>(nameof(IsRangeLimitMode), o => o.IsRangeLimitMode, (o, v) => o.IsRangeLimitMode = v);
    public static readonly DirectProperty<HoursCronPickerFieldItem, bool> IsIntervalModeProperty = AvaloniaProperty.RegisterDirect<HoursCronPickerFieldItem, bool>(nameof(IsIntervalMode), o => o.IsIntervalMode, (o, v) => o.IsIntervalMode = v);
    public static readonly DirectProperty<HoursCronPickerFieldItem, bool> IsListModeProperty = AvaloniaProperty.RegisterDirect<HoursCronPickerFieldItem, bool>(nameof(IsListMode), o => o.IsListMode, (o, v) => o.IsListMode = v);
    public static readonly StyledProperty<int> RangeStartProperty = AvaloniaProperty.Register<HoursCronPickerFieldItem, int>(nameof(RangeStart), defaultBindingMode: BindingMode.TwoWay);
    public static readonly StyledProperty<int> RangeEndProperty = AvaloniaProperty.Register<HoursCronPickerFieldItem, int>(nameof(RangeEnd), defaultBindingMode: BindingMode.TwoWay);
    public static readonly StyledProperty<int> IntervalStartHourProperty = AvaloniaProperty.Register<HoursCronPickerFieldItem, int>(nameof(IntervalStartHour), defaultBindingMode: BindingMode.TwoWay);
    public static readonly StyledProperty<int> IntervalValueProperty = AvaloniaProperty.Register<HoursCronPickerFieldItem, int>(nameof(IntervalValue), defaultBindingMode: BindingMode.TwoWay);
    public static readonly DirectProperty<HoursCronPickerFieldItem, ObservableCollection<int>> SelectedHoursProperty = AvaloniaProperty.RegisterDirect<HoursCronPickerFieldItem, ObservableCollection<int>>(nameof(SelectedHours), o => o.SelectedHours, (o, v) => { o.SelectedHours = v; });
    public static readonly DirectProperty<HoursCronPickerFieldItem, ObservableCollection<int>> AllowedHoursProperty = AvaloniaProperty.RegisterDirect<HoursCronPickerFieldItem, ObservableCollection<int>>(nameof(AllowedHours), o => o.AllowedHours, (o, v) => { o.AllowedHours = v; });
    
    private ObservableCollection<int> _selectedHours;
    private ObservableCollection<int> _allowedHours;

    public HoursCronPickerFieldItem()
    {
        _allowedHours = new ObservableCollection<int>(Enumerable.Range(0, 24));
        _selectedHours = new ObservableCollection<int>();
        WeakReference<HoursCronPickerFieldItem> weakReference = new WeakReference<HoursCronPickerFieldItem>(this);
        _selectedHours.CollectionChanged += (sender, args) =>
        {
            if (weakReference.TryGetTarget(out HoursCronPickerFieldItem? target))
            {
                target.OnSelectedHoursChanged(args);
            }
        };
        IsAllowSpecialValue = true; // 允许指定值
    }
    
    /// <summary>
    /// Whether the field is in every hour mode.
    /// </summary>
    public bool IsEveryHourMode
    {
        get => IsModeSelected(1);
        private set => SwitchCurrentModeTo(IsEveryHourModeProperty, 1, value);
    }

    /// <summary>
    /// Whether the field is in range limit mode.
    /// </summary>
    public bool IsRangeLimitMode
    {
        get => IsModeSelected(2);
        private set => SwitchCurrentModeTo(IsRangeLimitModeProperty, 2, value);
    }

    /// <summary>
    /// Whether the field is in interval mode.
    /// </summary>
    public bool IsIntervalMode
    {
        get => IsModeSelected(3);
        private set => SwitchCurrentModeTo(IsIntervalModeProperty, 3, value);
    }

    /// <summary>
    /// Whether the field is in list mode.
    /// </summary>
    public bool IsListMode
    {
        get => IsModeSelected(4);
        private set => SwitchCurrentModeTo(IsListModeProperty, 4, value);
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

    /// <summary>
    /// The start of the interval.
    /// </summary>
    public int IntervalStartHour
    {
        get => GetValue(IntervalStartHourProperty);
        set => SetValue(IntervalStartHourProperty, value);
    }

    /// <summary>
    /// The interval value.
    /// </summary>
    public int IntervalValue
    {
        get => GetValue(IntervalValueProperty);
        set => SetValue(IntervalValueProperty, value);
    }

    /// <summary>
    /// The selected hours.
    /// </summary>
    public ObservableCollection<int> SelectedHours
    {
        get => _selectedHours;
        private set => SetAndRaise(SelectedHoursProperty, ref _selectedHours, value);
    }

    /// <summary>
    /// The allowed values for the hours field.
    /// </summary>
    public ObservableCollection<int> AllowedHours
    {
        get => _allowedHours;
        private set => SetAndRaise(AllowedHoursProperty, ref _allowedHours, value);
    }

    protected override string DictKey { get; } = nameof(HoursCronPickerFieldItem);

    protected override bool IsSupportedIntervalMode => true;

    protected override bool IsSupportedEnumerableMode => true;

    protected override void ChangeCurrentModeToAnyMode() => IsEveryHourMode = true;

    protected override void OnCurrentModeSelected(AvaloniaProperty<bool> property)
    {
        if (property == IsRangeLimitModeProperty)
        {
            this.SetValueRange(this.RangeStart, this.RangeEnd);
        }
        else if (property == IsIntervalModeProperty)
        {
            this.SetValueInterval(this.IntervalStartHour, this.IntervalValue);
        }
        else if (property == IsListModeProperty)
        {
            this.RefreshByListMode();
        }
        else if (property == IsEveryHourModeProperty)
        {
            this.SetValueAnyMode();
        }
    }

    protected override void UnSafeChangeCurrentModeToRange(int leftParam, int rightParam)
    {
        ThrowIfRangeValueInvalid(leftParam, rightParam);
        RangeStart = leftParam;
        RangeEnd = rightParam;
        IsRangeLimitMode = true;
    }

    protected override void UnSafeChangeCurrentModeToInterval(int leftParam, int rightParam)
    {
        ThrowIfIntervalValueInvalid(leftParam, rightParam);
        IntervalStartHour = leftParam;
        IntervalValue = rightParam;
        IsIntervalMode = true;
    }

    protected override void UnSafeChangeCurrentModeToList(int[] values)
    {
        ThrowIfListValueInvalid(values);
        SelectedHours.Clear();
        foreach (int value in values)
        {
            SelectedHours.Add(value);
        }
        IsListMode = true;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SelectedHoursProperty)
        {
            this.RefreshByListMode();
        }
        else if (change.Property == RangeStartProperty || change.Property == RangeEndProperty)
        {
            if (IsRangeLimitMode)
            {
                this.SetValueRange(this.RangeStart, this.RangeEnd);
            }
        }
        else if (change.Property == IntervalStartHourProperty || change.Property == IntervalValueProperty)
        {
            if (IsIntervalMode)
            {
                this.SetValueInterval(this.IntervalStartHour, this.IntervalValue);
            }
        }
    }

    protected override void ThrowIfListValueInvalid(IEnumerable<int> values)
    {
        int[] arr = values.ToArray();
        if (arr.Length == 0)
        {
            throw GetParseException_EnumeratorEmpty();
        }

        int min = this.AllowedHours.Min();
        int max = this.AllowedHours.Max();
        if (arr.Min() < min || arr.Max() > max)
        {
            throw GetParseException_EnumeratorOutOfRange(min, max);
        }
    }

    protected override void ThrowIfRangeValueInvalid(int start, int end)
    {
        int min = this.AllowedHours.Min();
        int max = this.AllowedHours.Max();
        if (start < min || start > max || end < min || end > max)
        {
            throw GetParseException_RangeStartOrEndOutOfRange(min, max);
        }

        if (start >= end)
        {
            throw GetParseException_RangeEndLessThenStart();
        }
    }

    protected override void ThrowIfIntervalValueInvalid(int start, int interval)
    {
        int min = this.AllowedHours.Min();
        int max = this.AllowedHours.Max();
        if (start < min || start >= max)
        {
            throw GetParseException_IntervalLeftOutOfRange(min, max);
        }

        if (interval <= 0 || interval > max)
        {
            throw GetParseException_IntervalRightOutOfRange(0, max);
        }
    }
    
    private void OnSelectedHoursChanged(NotifyCollectionChangedEventArgs args) => this.RefreshByListMode();

    private void RefreshByListMode()
    {
        if (IsListMode)
        {
            int[]? arr = SelectedHours?.ToArray();
            if (arr != null && arr.Length > 0)
            {
                if (arr.Length == this.AllowedHours.Count && arr.Distinct().Count() == this.AllowedHours.Count)
                {
                    this.SetValueAnyMode();
                }
                else
                {
                    this.SetValueList(arr);
                }
            }
            else
            {
                this.SetValueAnyMode();
            }
        }
    }
}