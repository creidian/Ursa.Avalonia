using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Data;

namespace Ursa.Controls;

/// <summary>
/// A control that represents the months field in a cron expression.
/// </summary>
public class MonthsCronPickerFieldItem : CronPickerFieldItem
{
    public static readonly DirectProperty<MonthsCronPickerFieldItem, bool> IsEveryMonthModeProperty = AvaloniaProperty.RegisterDirect<MonthsCronPickerFieldItem, bool>(nameof(IsEveryMonthMode), o => o.IsEveryMonthMode, (o, v) => o.IsEveryMonthMode = v);
    public static readonly DirectProperty<MonthsCronPickerFieldItem, bool> IsRangeLimitModeProperty = AvaloniaProperty.RegisterDirect<MonthsCronPickerFieldItem, bool>(nameof(IsRangeLimitMode), o => o.IsRangeLimitMode, (o, v) => o.IsRangeLimitMode = v);
    public static readonly DirectProperty<MonthsCronPickerFieldItem, bool> IsIntervalModeProperty = AvaloniaProperty.RegisterDirect<MonthsCronPickerFieldItem, bool>(nameof(IsIntervalMode), o => o.IsIntervalMode, (o, v) => o.IsIntervalMode = v);
    public static readonly DirectProperty<MonthsCronPickerFieldItem, bool> IsListModeProperty = AvaloniaProperty.RegisterDirect<MonthsCronPickerFieldItem, bool>(nameof(IsListMode), o => o.IsListMode, (o, v) => o.IsListMode = v);
    public static readonly StyledProperty<int> RangeStartProperty = AvaloniaProperty.Register<MonthsCronPickerFieldItem, int>(nameof(RangeStart), defaultBindingMode: BindingMode.TwoWay);
    public static readonly StyledProperty<int> RangeEndProperty = AvaloniaProperty.Register<MonthsCronPickerFieldItem, int>(nameof(RangeEnd), defaultBindingMode: BindingMode.TwoWay);
    public static readonly StyledProperty<int> IntervalStartMonthProperty = AvaloniaProperty.Register<MonthsCronPickerFieldItem, int>(nameof(IntervalStartMonth), defaultBindingMode: BindingMode.TwoWay);
    public static readonly StyledProperty<int> IntervalValueProperty = AvaloniaProperty.Register<MonthsCronPickerFieldItem, int>(nameof(IntervalValue), defaultBindingMode: BindingMode.TwoWay);
    public static readonly DirectProperty<MonthsCronPickerFieldItem, ObservableCollection<int>> AllowedMonthsProperty = AvaloniaProperty.RegisterDirect<MonthsCronPickerFieldItem, ObservableCollection<int>>(nameof(AllowedMonths), o => o.AllowedMonths, (o, v) => o.AllowedMonths = v);
    public static readonly DirectProperty<MonthsCronPickerFieldItem, ObservableCollection<int>> SelectedMonthsProperty = AvaloniaProperty.RegisterDirect<MonthsCronPickerFieldItem, ObservableCollection<int>>(nameof(SelectedMonths), o => o.SelectedMonths, (o, v) => o.SelectedMonths = v);
    private ObservableCollection<int> _selectedMonths;
    private ObservableCollection<int> _allowedMonths;

    public MonthsCronPickerFieldItem()
    {
        _selectedMonths = new ObservableCollection<int>();
        _allowedMonths = new ObservableCollection<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
        WeakReference<MonthsCronPickerFieldItem> weakReference = new WeakReference<MonthsCronPickerFieldItem>(this);
        _selectedMonths.CollectionChanged += (sender, args) =>
        {
            if (weakReference.TryGetTarget(out MonthsCronPickerFieldItem? target))
            {
                target.OnSelectedMonthsChanged(args);
            }
        };
        IsAllowSpecialValue = true; // 允许指定值
    }

    /// <summary>
    /// Whether the field is in every month mode.
    /// </summary>
    public bool IsEveryMonthMode
    {
        get => IsModeSelected(1);
        private set => SwitchCurrentModeTo(IsEveryMonthModeProperty, 1, value);
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
    public int IntervalStartMonth
    {
        get => GetValue(IntervalStartMonthProperty);
        set => SetValue(IntervalStartMonthProperty, value);
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
    /// The selected months.
    /// </summary>
    public ObservableCollection<int> SelectedMonths
    {
        get => _selectedMonths;
        private set => SetAndRaise(SelectedMonthsProperty, ref _selectedMonths, value);
    }

    /// <summary>
    /// The allowed values for the months field.
    /// </summary>
    public ObservableCollection<int> AllowedMonths { get => _allowedMonths; private set => SetAndRaise(AllowedMonthsProperty, ref _allowedMonths, value); }

    protected override string DictKey { get; } = nameof(MonthsCronPickerFieldItem);

    protected override bool IsSupportedIntervalMode => true;

    protected override bool IsSupportedEnumerableMode => true;

    protected override void OnCurrentModeSelected(AvaloniaProperty<bool> property)
    {
        if (property == IsRangeLimitModeProperty)
        {
            this.SetValueRange(this.RangeStart, this.RangeEnd);
        }
        else if (property == IsIntervalModeProperty)
        {
            this.SetValueInterval(this.IntervalStartMonth, this.IntervalValue);
        }
        else if (property == IsListModeProperty)
        {
            this.RefreshByListMode();
        }
        else if (property == IsEveryMonthModeProperty)
        {
            this.SetValueAnyMode();
        }
    }

    protected override void ChangeCurrentModeToAnyMode() => IsEveryMonthMode = true;

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
        IntervalStartMonth = leftParam;
        IntervalValue = rightParam;
        IsIntervalMode = true;
    }

    protected override void UnSafeChangeCurrentModeToList(int[] values)
    {
        ThrowIfListValueInvalid(values);
        SelectedMonths.Clear();
        foreach (int value in values)
        {
            SelectedMonths.Add(value);
        }
        IsListMode = true;
    }

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
        else if (change.Property == IntervalStartMonthProperty || change.Property == IntervalValueProperty)
        {
            if (IsIntervalMode)
            {
                this.SetValueInterval(this.IntervalStartMonth, this.IntervalValue);
            }
        }
        else if (change.Property == SelectedMonthsProperty)
        {
            this.RefreshByListMode();
        }
    }

    protected override void ThrowIfListValueInvalid(IEnumerable<int> values)
    {
        int[] arr = values.ToArray();
        if (arr.Length == 0)
        {
            throw GetParseException_EnumeratorEmpty();
        }

        int min = this.AllowedMonths.Min();
        int max = this.AllowedMonths.Max();
        if (arr.Min() < min || arr.Max() > max)
        {
            throw GetParseException_EnumeratorOutOfRange(min, max);
        }
    }

    protected override void ThrowIfRangeValueInvalid(int start, int end)
    {
        int min = this.AllowedMonths.Min();
        int max = this.AllowedMonths.Max();
        if (start < min || end > max)
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
        int min = this.AllowedMonths.Min();
        int max = this.AllowedMonths.Max();
        if (start < min || start >= max)
        {
            throw GetParseException_IntervalLeftOutOfRange(min, max);
        }

        if (interval <= 0 || interval > max)
        {
            throw GetParseException_IntervalRightOutOfRange(0, max);
        }
    }
    
    private void OnSelectedMonthsChanged(NotifyCollectionChangedEventArgs args) => this.RefreshByListMode();

    private void RefreshByListMode()
    {
        if (IsListMode)
        {
            int[]? arr = SelectedMonths?.ToArray();
            if (arr != null && arr.Length > 0)
            {
                if (arr.Length == this.AllowedMonths.Count && arr.Distinct().Count() == this.AllowedMonths.Count)
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