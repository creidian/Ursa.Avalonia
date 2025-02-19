using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Data;

namespace Ursa.Controls;

/// <summary>
/// A control that represents the days of month field in a cron expression.
/// </summary>
public class DaysOfMonthCronPickerFieldItem : CronPickerFieldItem
{
    public static readonly DirectProperty<DaysOfMonthCronPickerFieldItem, bool> IsEveryDayOfMonthModeProperty = AvaloniaProperty.RegisterDirect<DaysOfMonthCronPickerFieldItem, bool>(nameof(IsEveryDayOfMonthMode), o => o.IsEveryDayOfMonthMode, (o, v) => o.IsEveryDayOfMonthMode = v);
    public static readonly DirectProperty<DaysOfMonthCronPickerFieldItem, bool> IsRangeLimitModeProperty = AvaloniaProperty.RegisterDirect<DaysOfMonthCronPickerFieldItem, bool>(nameof(IsRangeLimitMode), o => o.IsRangeLimitMode, (o, v) => o.IsRangeLimitMode = v);
    public static readonly DirectProperty<DaysOfMonthCronPickerFieldItem, bool> IsIntervalModeProperty = AvaloniaProperty.RegisterDirect<DaysOfMonthCronPickerFieldItem, bool>(nameof(IsIntervalMode), o => o.IsIntervalMode, (o, v) => o.IsIntervalMode = v);
    public static readonly DirectProperty<DaysOfMonthCronPickerFieldItem, bool> IsListModeProperty = AvaloniaProperty.RegisterDirect<DaysOfMonthCronPickerFieldItem, bool>(nameof(IsListMode), o => o.IsListMode, (o, v) => o.IsListMode = v);
    public static readonly DirectProperty<DaysOfMonthCronPickerFieldItem, bool> IsPlaceholderModeProperty = AvaloniaProperty.RegisterDirect<DaysOfMonthCronPickerFieldItem, bool>(nameof(IsPlaceholderMode), o => o.IsPlaceholderMode, (o, v) => o.IsPlaceholderMode = v);
    public static readonly DirectProperty<DaysOfMonthCronPickerFieldItem, bool> IsRecentlyWeekdayModeProperty = AvaloniaProperty.RegisterDirect<DaysOfMonthCronPickerFieldItem, bool>(nameof(IsRecentlyWeekdayMode), o => o.IsRecentlyWeekdayMode, (o, v) => o.IsRecentlyWeekdayMode = v);
    public static readonly DirectProperty<DaysOfMonthCronPickerFieldItem, bool> IsLastdayModeProperty = AvaloniaProperty.RegisterDirect<DaysOfMonthCronPickerFieldItem, bool>(nameof(IsLastdayMode), o => o.IsLastdayMode, (o, v) => o.IsLastdayMode = v);
    public static readonly StyledProperty<int> RangeStartProperty = AvaloniaProperty.Register<DaysOfMonthCronPickerFieldItem, int>(nameof(RangeStart), defaultBindingMode: BindingMode.TwoWay);
    public static readonly StyledProperty<int> RangeEndProperty = AvaloniaProperty.Register<DaysOfMonthCronPickerFieldItem, int>(nameof(RangeEnd), defaultBindingMode: BindingMode.TwoWay);
    public static readonly StyledProperty<int> IntervalStartDayProperty = AvaloniaProperty.Register<DaysOfMonthCronPickerFieldItem, int>(nameof(IntervalStartDay), defaultBindingMode: BindingMode.TwoWay);
    public static readonly StyledProperty<int> IntervalValueProperty = AvaloniaProperty.Register<DaysOfMonthCronPickerFieldItem, int>(nameof(IntervalValue), defaultBindingMode: BindingMode.TwoWay);
    public static readonly StyledProperty<int> RecentlyWeekdayProperty = AvaloniaProperty.Register<DaysOfMonthCronPickerFieldItem, int>(nameof(RecentlyWeekday), defaultBindingMode: BindingMode.TwoWay);
    public static readonly DirectProperty<DaysOfMonthCronPickerFieldItem, ObservableCollection<int>> SelectedDaysProperty = AvaloniaProperty.RegisterDirect<DaysOfMonthCronPickerFieldItem, ObservableCollection<int>>(nameof(SelectedDays), o => o.SelectedDays, (o, v) => { o.SelectedDays = v; });
    public static readonly DirectProperty<DaysOfMonthCronPickerFieldItem, ObservableCollection<int>> AllowedDaysOfMonthProperty = AvaloniaProperty.RegisterDirect<DaysOfMonthCronPickerFieldItem, ObservableCollection<int>>(nameof(AllowedDaysOfMonth), o => o.AllowedDaysOfMonth, (o, v) => { o.AllowedDaysOfMonth = v; });

    private ObservableCollection<int> _selectedDays;
    private ObservableCollection<int> _allowedDaysOfMonth;

    public DaysOfMonthCronPickerFieldItem()
    {
        _allowedDaysOfMonth = new ObservableCollection<int>(Enumerable.Range(1, 31));
        _selectedDays = new ObservableCollection<int>();
        WeakReference<DaysOfMonthCronPickerFieldItem> weakReference = new WeakReference<DaysOfMonthCronPickerFieldItem>(this);
        _selectedDays.CollectionChanged += (sender, args) =>
        {
            if (weakReference.TryGetTarget(out DaysOfMonthCronPickerFieldItem? target))
            {
                target.OnSelectedDaysChanged(args);
            }
        };
        IsAllowSpecialValue = true; // 允许指定值
    }

    /// <summary>
    /// Whether the field is in every day of month mode.
    /// </summary>
    public bool IsEveryDayOfMonthMode
    {
        get => IsModeSelected(1);
        private set => SwitchCurrentModeTo(IsEveryDayOfMonthModeProperty, 1, value);
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
    /// Whether the field is in placeholder mode.
    /// </summary>
    public bool IsPlaceholderMode
    {
        get => IsModeSelected(5);
        private set => SwitchCurrentModeTo(IsPlaceholderModeProperty, 5, value);
    }

    /// <summary>
    /// Whether the field is in recently weekday mode.
    /// </summary>
    public bool IsRecentlyWeekdayMode
    {
        get => IsModeSelected(6);
        private set => SwitchCurrentModeTo(IsRecentlyWeekdayModeProperty, 6, value);
    }

    /// <summary>
    /// Whether the field is in lastday mode.
    /// </summary>
    public bool IsLastdayMode
    {
        get => IsModeSelected(7);
        private set => SwitchCurrentModeTo(IsLastdayModeProperty, 7, value);
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
    public int IntervalStartDay
    {
        get => GetValue(IntervalStartDayProperty);
        set => SetValue(IntervalStartDayProperty, value);
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
    /// The recently weekday.
    /// </summary>
    public int RecentlyWeekday
    {
        get => GetValue(RecentlyWeekdayProperty);
        set => SetValue(RecentlyWeekdayProperty, value);
    }

    /// <summary>
    /// The selected days.
    /// </summary>
    public ObservableCollection<int> SelectedDays
    {
        get => _selectedDays;
        private set => SetAndRaise(SelectedDaysProperty, ref _selectedDays, value);
    }

    /// <summary>
    /// The allowed values for the days of month field.
    /// </summary>
    public ObservableCollection<int> AllowedDaysOfMonth
    {
        get => _allowedDaysOfMonth;
        private set => SetAndRaise(AllowedDaysOfMonthProperty, ref _allowedDaysOfMonth, value);
    }

    protected override string DictKey { get; } = nameof(DaysOfMonthCronPickerFieldItem);

    protected override bool IsSupportedPlaceholderMode => true;

    protected override bool IsSupportedLastMode => true;

    protected override bool IsSupportedIntervalMode => true;

    protected override bool IsSupportedEnumerableMode => true;

    protected override bool IsSupportedSpecialWeekdayMode => true;

    /// <summary>
    /// 设定为月最后一天
    /// </summary>
    public void SetValueLastday() => this.OnValueChange(FIELD_CHAR_LAST);

    /// <summary>
    /// 设定为最近工作日
    /// </summary>
    /// <param name="weekday"> 本月第几号 </param>
    public void SetValueRecentlyWeekday(int weekday)
    {
        try
        {
            ThrowIfWeekdayInvalid(weekday);
        }
        catch (Exception e)
        {
            return;
        }
        this.OnValueChange(GetRecentlyWeekdayExpression(weekday));
    }

    protected override void OnCurrentModeSelected(AvaloniaProperty<bool> property)
    {
        if (property == IsRangeLimitModeProperty)
        {
            this.SetValueRange(this.RangeStart, this.RangeEnd);
        }
        else if (property == IsIntervalModeProperty)
        {
            this.SetValueInterval(this.IntervalStartDay, this.IntervalValue);
        }
        else if (property == IsListModeProperty)
        {
            this.RefreshByListMode();
        }
        else if (property == IsEveryDayOfMonthModeProperty)
        {
            this.SetValueAnyMode();
        }
        else if (property == IsPlaceholderModeProperty)
        {
            this.SetValuePlaceholder();
        }
        else if (property == IsLastdayModeProperty)
        {
            this.SetValueLastday();
        }
        else if (property == IsRecentlyWeekdayModeProperty)
        {
            this.SetValueRecentlyWeekday(this.RecentlyWeekday);
        }
    }

    protected override void ChangeCurrentModeToAnyMode() => this.IsEveryDayOfMonthMode = true;

    protected override void ChangeCurrentModeToPlaceholderMode() => this.IsPlaceholderMode = true;

    protected override void UnSafeChangeCurrentModeToRange(int leftParam, int rightParam)
    {
        ThrowIfRangeValueInvalid(leftParam, rightParam);
        this.RangeStart = leftParam;
        this.RangeEnd = rightParam;
        this.IsRangeLimitMode = true;
    }

    protected override void UnSafeChangeCurrentModeToInterval(int leftParam, int rightParam)
    {
        ThrowIfIntervalValueInvalid(leftParam, rightParam);
        this.IntervalStartDay = leftParam;
        this.IntervalValue = rightParam;
        this.IsIntervalMode = true;
    }

    protected override void UnSafeChangeCurrentModeToWeekday(int time)
    {
        ThrowIfWeekdayInvalid(time);
        this.RecentlyWeekday = time;
        this.IsRecentlyWeekdayMode = true;
    }

    protected override void UnSafeChangeCurrentModeToLast(int time) => throw GetParseException_LastLeftOutOfRange(0, 0);

    protected override void UnSafeChangeCurrentModeToLast() => this.IsLastdayMode = true;

    protected override void UnSafeChangeCurrentModeToList(int[] values)
    {
        ThrowIfListValueInvalid(values);
        this.SelectedDays.Clear();
        foreach (int value in values)
        {
            this.SelectedDays.Add(value);
        }
        this.IsListMode = true;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SelectedDaysProperty)
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
        else if (change.Property == IntervalStartDayProperty || change.Property == IntervalValueProperty)
        {
            if (IsIntervalMode)
            {
                this.SetValueInterval(this.IntervalStartDay, this.IntervalValue);
            }
        }
        else if (change.Property == RecentlyWeekdayProperty)
        {
            if (IsRecentlyWeekdayMode)
            {
                this.SetValueRecentlyWeekday(this.RecentlyWeekday);
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

        int min = this.AllowedDaysOfMonth.Min();
        int max = this.AllowedDaysOfMonth.Max();
        if (arr.Min() < min || arr.Max() > max)
        {
            throw GetParseException_EnumeratorOutOfRange(min, max);
        }
    }

    protected override void ThrowIfRangeValueInvalid(int start, int end)
    {
        int min = this.AllowedDaysOfMonth.Min();
        int max = this.AllowedDaysOfMonth.Max();
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
        int min = this.AllowedDaysOfMonth.Min();
        int max = this.AllowedDaysOfMonth.Max();
        if (start < min || start > max)
        {
            throw GetParseException_IntervalLeftOutOfRange(min, max);
        }

        if (interval <= 0 || interval > max)
        {
            throw GetParseException_IntervalRightOutOfRange(1, max);
        }
    }
    
    protected virtual void ThrowIfWeekdayInvalid(int weekday)
    {
        if (weekday < 1 || weekday > 31)
        {
            throw GetParseException_WModeLeftOutOfRange(1, 31);
        }
    }
    
    private void OnSelectedDaysChanged(NotifyCollectionChangedEventArgs args) => this.RefreshByListMode();

    private void RefreshByListMode()
    {
        if (IsListMode)
        {
            int[]? arr = SelectedDays?.ToArray();
            if (arr != null && arr.Length > 0)
            {
                if (arr.Length == this.AllowedDaysOfMonth.Count && arr.Distinct().Count() == this.AllowedDaysOfMonth.Count)
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