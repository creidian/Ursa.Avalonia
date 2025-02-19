using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Data;

namespace Ursa.Controls;

/// <summary>
/// A control that represents the minutes field in a cron expression.
/// </summary>
public class MinutesCronPickerFieldItem : CronPickerFieldItem
{
    public static readonly DirectProperty<MinutesCronPickerFieldItem, bool> IsEveryMinuteModeProperty = AvaloniaProperty.RegisterDirect<MinutesCronPickerFieldItem, bool>(nameof(IsEveryMinuteMode), o => o.IsEveryMinuteMode, (o, v) => o.IsEveryMinuteMode = v);
    public static readonly DirectProperty<MinutesCronPickerFieldItem, bool> IsRangeLimitModeProperty = AvaloniaProperty.RegisterDirect<MinutesCronPickerFieldItem, bool>(nameof(IsRangeLimitMode), o => o.IsRangeLimitMode, (o, v) => o.IsRangeLimitMode = v);
    public static readonly DirectProperty<MinutesCronPickerFieldItem, bool> IsIntervalModeProperty = AvaloniaProperty.RegisterDirect<MinutesCronPickerFieldItem, bool>(nameof(IsIntervalMode), o => o.IsIntervalMode, (o, v) => o.IsIntervalMode = v);
    public static readonly DirectProperty<MinutesCronPickerFieldItem, bool> IsListModeProperty = AvaloniaProperty.RegisterDirect<MinutesCronPickerFieldItem, bool>(nameof(IsListMode), o => o.IsListMode, (o, v) => o.IsListMode = v);
    public static readonly StyledProperty<int> RangeStartProperty = AvaloniaProperty.Register<MinutesCronPickerFieldItem, int>(nameof(RangeStart), defaultBindingMode: BindingMode.TwoWay);
    public static readonly StyledProperty<int> RangeEndProperty = AvaloniaProperty.Register<MinutesCronPickerFieldItem, int>(nameof(RangeEnd), defaultBindingMode: BindingMode.TwoWay);
    public static readonly StyledProperty<int> IntervalStartMinuteProperty = AvaloniaProperty.Register<MinutesCronPickerFieldItem, int>(nameof(IntervalStartMinute), defaultBindingMode: BindingMode.TwoWay);
    public static readonly StyledProperty<int> IntervalValueProperty = AvaloniaProperty.Register<MinutesCronPickerFieldItem, int>(nameof(IntervalValue), defaultBindingMode: BindingMode.TwoWay);
    public static readonly DirectProperty<MinutesCronPickerFieldItem, ObservableCollection<int>> SelectedMinutesProperty = AvaloniaProperty.RegisterDirect<MinutesCronPickerFieldItem, ObservableCollection<int>>(nameof(SelectedMinutes), o => o.SelectedMinutes, (o, v) => { o.SelectedMinutes = v; });
    public static readonly DirectProperty<MinutesCronPickerFieldItem, ObservableCollection<int>> AllowedMinutesProperty = AvaloniaProperty.RegisterDirect<MinutesCronPickerFieldItem, ObservableCollection<int>>(nameof(AllowedMinutes), o => o.AllowedMinutes, (o, v) => { o.AllowedMinutes = v; });
    
    private ObservableCollection<int> _selectedMinutes;
    private ObservableCollection<int> _allowedMinutes;

    public MinutesCronPickerFieldItem()
    {
        _allowedMinutes = new ObservableCollection<int>(Enumerable.Range(0, 60));
        _selectedMinutes = new ObservableCollection<int>();
        WeakReference<MinutesCronPickerFieldItem> weakReference = new WeakReference<MinutesCronPickerFieldItem>(this);
        _selectedMinutes.CollectionChanged += (sender, args) =>
        {
            if (weakReference.TryGetTarget(out MinutesCronPickerFieldItem? target))
            {
                target.OnSelectedMinutesChanged(args);
            }
        };
        IsAllowSpecialValue = true; // 允许指定值
    }

    /// <summary>
    /// Whether the field is in every minute mode.
    /// </summary>
    public bool IsEveryMinuteMode
    {
        get => IsModeSelected(1);
        private set => SwitchCurrentModeTo(IsEveryMinuteModeProperty, 1, value);
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
    public int IntervalStartMinute
    {
        get => GetValue(IntervalStartMinuteProperty);
        set => SetValue(IntervalStartMinuteProperty, value);
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
    /// The selected minutes.
    /// </summary>
    public ObservableCollection<int> SelectedMinutes
    {
        get => _selectedMinutes;
        private set => SetAndRaise(SelectedMinutesProperty, ref _selectedMinutes, value);
    }
    
    /// <summary>
    /// The allowed values for the minutes field.
    /// </summary>
    public ObservableCollection<int> AllowedMinutes
    {
        get => _allowedMinutes;
        private set => SetAndRaise(AllowedMinutesProperty, ref _allowedMinutes, value);
    }

    protected override string DictKey { get; } = nameof(MinutesCronPickerFieldItem);

    protected override bool IsSupportedIntervalMode => true;

    protected override bool IsSupportedEnumerableMode => true;

    protected override void ChangeCurrentModeToAnyMode() => IsEveryMinuteMode = true;

    protected override void OnCurrentModeSelected(AvaloniaProperty<bool> property)
    {
        if (property == IsRangeLimitModeProperty)
        {
            this.SetValueRange(this.RangeStart, this.RangeEnd);
        }
        else if (property == IsIntervalModeProperty)
        {
            this.SetValueInterval(this.IntervalStartMinute, this.IntervalValue);
        }
        else if (property == IsListModeProperty)
        {
            this.RefreshByListMode();
        }
        else if (property == IsEveryMinuteModeProperty)
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
    
    protected override void UnSafeChangeCurrentModeToInterval(int startParam, int intervalParam)
    {
        ThrowIfIntervalValueInvalid(startParam, intervalParam);
        IntervalStartMinute = startParam;
        IntervalValue = intervalParam;
        IsIntervalMode = true;
    }

    protected override void UnSafeChangeCurrentModeToList(int[] valuesParam)
    {
        ThrowIfListValueInvalid(valuesParam);
        SelectedMinutes.Clear();
        foreach (int value in valuesParam)
        {
            SelectedMinutes.Add(value);
        }
        IsListMode = true;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SelectedMinutesProperty)
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
        else if (change.Property == IntervalStartMinuteProperty || change.Property == IntervalValueProperty)
        {
            if (IsIntervalMode)
            {
                this.SetValueInterval(this.IntervalStartMinute, this.IntervalValue);
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

        int min = this.AllowedMinutes.Min();
        int max = this.AllowedMinutes.Max();
        if (arr.Min() < min || arr.Max() > max)
        {
            throw GetParseException_EnumeratorOutOfRange(min, max);
        }
    }

    protected override void ThrowIfRangeValueInvalid(int start, int end)
    {
        int min = this.AllowedMinutes.Min();
        int max = this.AllowedMinutes.Max();
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
        int min = this.AllowedMinutes.Min();
        int max = this.AllowedMinutes.Max();
        if (start < min || start >= max)
        {
            throw GetParseException_IntervalLeftOutOfRange(min, max);
        }

        if (interval < 1 || interval > max)
        {
            throw GetParseException_IntervalRightOutOfRange(0, max);
        }
    }
    
    private void OnSelectedMinutesChanged(NotifyCollectionChangedEventArgs args) => this.RefreshByListMode();

    private void RefreshByListMode()
    {
        if (IsListMode)
        {
            int[]? arr = SelectedMinutes?.ToArray();
            if (arr != null && arr.Length > 0)
            {
                if (arr.Length == this.AllowedMinutes.Count && arr.Distinct().Count() == this.AllowedMinutes.Count)
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