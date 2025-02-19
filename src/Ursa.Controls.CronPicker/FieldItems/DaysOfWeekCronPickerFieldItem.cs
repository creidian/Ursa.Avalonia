using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Data;

namespace Ursa.Controls;

/// <summary>
/// A control that represents the days of week field in a cron expression.
/// </summary>
public class DaysOfWeekCronPickerFieldItem : CronPickerFieldItem
{
    public static readonly DirectProperty<DaysOfWeekCronPickerFieldItem, bool> IsEveryDayOfWeekModeProperty = AvaloniaProperty.RegisterDirect<DaysOfWeekCronPickerFieldItem, bool>(nameof(IsEveryDayOfWeekMode), o => o.IsEveryDayOfWeekMode, (o, v) => o.IsEveryDayOfWeekMode = v);
    public static readonly DirectProperty<DaysOfWeekCronPickerFieldItem, bool> IsListModeProperty = AvaloniaProperty.RegisterDirect<DaysOfWeekCronPickerFieldItem, bool>(nameof(IsListMode), o => o.IsListMode, (o, v) => o.IsListMode = v);
    public static readonly DirectProperty<DaysOfWeekCronPickerFieldItem, bool> IsRangeLimitModeProperty = AvaloniaProperty.RegisterDirect<DaysOfWeekCronPickerFieldItem, bool>(nameof(IsRangeLimitMode), o => o.IsRangeLimitMode, (o, v) => o.IsRangeLimitMode = v);
    public static readonly DirectProperty<DaysOfWeekCronPickerFieldItem, bool> IsSpecialWeekdayModeProperty = AvaloniaProperty.RegisterDirect<DaysOfWeekCronPickerFieldItem, bool>(nameof(IsSpecialWeekdayMode), o => o.IsSpecialWeekdayMode, (o, v) => o.IsSpecialWeekdayMode = v);
    public static readonly DirectProperty<DaysOfWeekCronPickerFieldItem, bool> IsPlaceholderModeProperty = AvaloniaProperty.RegisterDirect<DaysOfWeekCronPickerFieldItem, bool>(nameof(IsPlaceholderMode), o => o.IsPlaceholderMode, (o, v) => o.IsPlaceholderMode = v);
    public static readonly DirectProperty<DaysOfWeekCronPickerFieldItem, bool> IsLastModeProperty = AvaloniaProperty.RegisterDirect<DaysOfWeekCronPickerFieldItem, bool>(nameof(IsLastMode), o => o.IsLastMode, (o, v) => o.IsLastMode = v);
    public static readonly StyledProperty<int> RangeStartProperty = AvaloniaProperty.Register<DaysOfWeekCronPickerFieldItem, int>(nameof(RangeStart), defaultBindingMode: BindingMode.TwoWay);
    public static readonly StyledProperty<int> RangeEndProperty = AvaloniaProperty.Register<DaysOfWeekCronPickerFieldItem, int>(nameof(RangeEnd), defaultBindingMode: BindingMode.TwoWay);
    public static readonly StyledProperty<int> SpecialWeekProperty = AvaloniaProperty.Register<DaysOfWeekCronPickerFieldItem, int>(nameof(SpecialWeek), defaultBindingMode: BindingMode.TwoWay);
    public static readonly StyledProperty<int> SpecialWeekDayProperty = AvaloniaProperty.Register<DaysOfWeekCronPickerFieldItem, int>(nameof(SpecialWeekDay), defaultBindingMode: BindingMode.TwoWay);
    public static readonly StyledProperty<int> LastWeekdayProperty = AvaloniaProperty.Register<DaysOfWeekCronPickerFieldItem, int>(nameof(LastWeekday), defaultBindingMode: BindingMode.TwoWay);
    public static readonly DirectProperty<DaysOfWeekCronPickerFieldItem, ObservableCollection<int>> AllowedWeekDaysProperty = AvaloniaProperty.RegisterDirect<DaysOfWeekCronPickerFieldItem, ObservableCollection<int>>(nameof(AllowedWeekDays), o => o.AllowedWeekDays, (o, v) => o.AllowedWeekDays = v);
    public static readonly DirectProperty<DaysOfWeekCronPickerFieldItem, ObservableCollection<int>> SelectedWeekDaysProperty = AvaloniaProperty.RegisterDirect<DaysOfWeekCronPickerFieldItem, ObservableCollection<int>>(nameof(SelectedWeekDays), o => o.SelectedWeekDays, (o, v) => o.SelectedWeekDays = v);
    
    private ObservableCollection<int> _selectedWeekDays;
    private ObservableCollection<int> _allowedWeekDays;
    
    public DaysOfWeekCronPickerFieldItem()
    {
        _selectedWeekDays = new ObservableCollection<int>();
        _allowedWeekDays = new ObservableCollection<int> { 1, 2, 3, 4, 5, 6, 7 };
        WeakReference<DaysOfWeekCronPickerFieldItem> weakReference = new WeakReference<DaysOfWeekCronPickerFieldItem>(this);
        _selectedWeekDays.CollectionChanged += (sender, args) =>
        {
            if (weakReference.TryGetTarget(out DaysOfWeekCronPickerFieldItem? target))
            {
                target.OnSelectedWeekDaysChanged(args);
            }
        };
        IsAllowSpecialValue = true; // 允许指定值
    }

    #region 属性

    /// <summary>
    /// Whether the field is in every day of week mode.
    /// </summary>
    public bool IsEveryDayOfWeekMode
    {
        get => IsModeSelected(1);
        private set => SwitchCurrentModeTo(IsEveryDayOfWeekModeProperty, 1, value);
    }

    /// <summary>
    /// Whether the field is in list mode.
    /// </summary>
    public bool IsListMode
    {
        get => IsModeSelected(2);
        private set => SwitchCurrentModeTo(IsListModeProperty, 2, value);
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
    /// Whether the field is in special weekday mode.
    /// </summary>
    public bool IsSpecialWeekdayMode
    {
        get => IsModeSelected(4);
        private set => SwitchCurrentModeTo(IsSpecialWeekdayModeProperty, 4, value);
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
    /// Whether the field is in last mode.
    /// </summary>
    public bool IsLastMode
    {
        get => IsModeSelected(6);
        private set => SwitchCurrentModeTo(IsLastModeProperty, 6, value);
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
    /// The special week.
    /// </summary>
    public int SpecialWeek
    {
        get => GetValue(SpecialWeekProperty);
        set => SetValue(SpecialWeekProperty, value);
    }

    /// <summary>
    /// The special weekday.
    /// </summary>
    public int SpecialWeekDay
    {
        get => GetValue(SpecialWeekDayProperty);
        set => SetValue(SpecialWeekDayProperty, value);
    }

    /// <summary>
    /// The recently weekday.
    /// </summary>
    public int LastWeekday
    {
        get => GetValue(LastWeekdayProperty);
        set => SetValue(LastWeekdayProperty, value);
    }

    /// <summary>
    /// The selected weekdays.
    /// </summary>
    public ObservableCollection<int> SelectedWeekDays
    {
        get => _selectedWeekDays;
        private set => SetAndRaise(SelectedWeekDaysProperty, ref _selectedWeekDays, value);
    }

    /// <summary>
    /// The allowed values for the days of week field.
    /// </summary>
    public ObservableCollection<int> AllowedWeekDays { get => _allowedWeekDays; private set => SetAndRaise(AllowedWeekDaysProperty, ref _allowedWeekDays, value); }
    
    #endregion

    protected override string DictKey { get; } = nameof(DaysOfWeekCronPickerFieldItem);

    protected override bool IsSupportedLastMode => true;

    protected override bool IsSupportedPlaceholderMode => true;

    protected override bool IsSupportedSharpMode => true;

    protected override bool IsSupportedEnumerableMode => true;

    /// <summary>
    /// 设置指定星期的指定星期几的表达式。
    /// </summary>
    /// <param name="week"> 第几周 </param>
    /// <param name="weekday"> 星期几 </param>
    public void SetValueSpecialWeekday(int week, int weekday)
    {
        try
        {
            ThrowIfSpecialWeekdayValueInvalid(week, weekday);
        }
        catch (Exception e)
        {
            return;
        }
        this.OnValueChange(GetSpecialWeekdayExpression(week, weekday));
    }

    protected override void OnCurrentModeSelected(AvaloniaProperty<bool> property)
    {
        if (property == IsEveryDayOfWeekModeProperty)
        {
            this.SetValueAnyMode();
        }
        else if (property == IsListModeProperty)
        {
            this.RefreshByListMode();
        }
        else if (property == IsRangeLimitModeProperty)
        {
            this.SetValueRange(this.RangeStart, this.RangeEnd);
        }
        else if (property == IsPlaceholderModeProperty)
        {   
            this.SetValuePlaceholder();
        }
        else if (property == IsLastModeProperty)
        {
            this.SetValueLast(this.LastWeekday);
        }
        else if (property == IsSpecialWeekdayModeProperty)
        {
            this.SetValueSpecialWeekday(this.SpecialWeek, this.SpecialWeekDay);
        }
    }

    protected override void ChangeCurrentModeToPlaceholderMode() => this.IsPlaceholderMode = true;

    protected override void ChangeCurrentModeToAnyMode() => this.IsEveryDayOfWeekMode = true;

    protected override void UnSafeChangeCurrentModeToRange(int leftParam, int rightParam)
    {
        ThrowIfRangeValueInvalid(leftParam, rightParam);
        this.RangeStart = leftParam;
        this.RangeEnd = rightParam;
        this.IsRangeLimitMode = true;
    }

    protected override void UnSafeChangeCurrentModeToLast(int time)
    {
        if (time < 1 || time > 31)
        {
            throw GetParseException_LastLeftOutOfRange(1, 31);
        }
        
        this.LastWeekday = time;
        this.IsLastMode = true;
    }

    protected override void UnSafeChangeCurrentModeToLast() => throw GetParseException_LastLeftEmpty();

    protected override void UnSafeChangeCurrentModeToList(int[] values)
    {
        ThrowIfListValueInvalid(values);
        this.SelectedWeekDays.Clear();
        foreach (int value in values)
        {
            this.SelectedWeekDays.Add(value);
        }

        this.IsListMode = true;
    }

    protected override void UnSafeChangeCurrentModeToSharp(int leftParam, int rightParam)
    {
        ThrowIfSpecialWeekdayValueInvalid(rightParam, leftParam);
        this.SpecialWeek = rightParam;
        this.SpecialWeekDay = leftParam;
        this.IsSpecialWeekdayMode = true;
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
        else if (change.Property == SpecialWeekProperty || change.Property == SpecialWeekDayProperty)
        {
            if (IsSpecialWeekdayMode)
            {
                this.SetValueSpecialWeekday(this.SpecialWeek, this.SpecialWeekDay);
            }
        }
        else if (change.Property == LastWeekdayProperty)
        {
            if (IsLastMode)
            {
                this.SetValueLast(this.LastWeekday);
            }
        }
        else if (change.Property == SelectedWeekDaysProperty)
        {
            this.RefreshByListMode();
        }
    }

    protected virtual void ThrowIfListValueInvalid(int[] arr)
    {
        if (arr.Length == 0)
        {
            throw GetParseException_EnumeratorEmpty();
        }

        int min = this.AllowedWeekDays.Min();
        int max = this.AllowedWeekDays.Max();
        if (arr.Min() < min || arr.Max() > max)
        {
            throw GetParseException_EnumeratorOutOfRange(min, max);
        }
    }

    protected override void ThrowIfListValueInvalid(IEnumerable<int> values) => ThrowIfListValueInvalid(values is int[] arr ? arr : values.ToArray());

    protected override void ThrowIfRangeValueInvalid(int start, int end)
    {
        int min = this.AllowedWeekDays.Min();
        int max = this.AllowedWeekDays.Max();
        if (start < min || end < min || start > max || end > max)
        {
            throw GetParseException_RangeStartOrEndOutOfRange(min, max);
        }

        if (start >= end)
        {
            throw GetParseException_RangeEndLessThenStart();
        }
    }
    
    protected void ThrowIfSpecialWeekdayValueInvalid(int week, int weekday)
    {
        if (week < 1 || week > 5)
        {
            throw GetParseException_SharpRightOutOfRange(1, 5);
        }

        if (weekday < 1 || weekday > 7)
        {
            throw GetParseException_SharpLeftOutOfRange(1, 7);
        }
    }
    
    private void OnSelectedWeekDaysChanged(NotifyCollectionChangedEventArgs args) => this.RefreshByListMode();

    private void RefreshByListMode()
    {
        if (IsListMode)
        {
            int[]? arr = SelectedWeekDays?.ToArray();
            if (arr != null && arr.Length > 0)
            {
                if (arr.Length == this.AllowedWeekDays.Count && arr.Distinct().Count() == this.AllowedWeekDays.Count)
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