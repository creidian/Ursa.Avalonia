using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Data;

namespace Ursa.Controls;

/// <summary>
/// A control that represents the seconds field in a cron expression.
/// </summary>
public class SecondsCronPickerFieldItem : CronPickerFieldItem
{
    public static readonly DirectProperty<SecondsCronPickerFieldItem, bool> IsEverySecondModeProperty = AvaloniaProperty.RegisterDirect<SecondsCronPickerFieldItem, bool>(nameof(IsEverySecondMode), o => o.IsEverySecondMode, (o, v) => o.IsEverySecondMode = v, defaultBindingMode: BindingMode.TwoWay);
    public static readonly DirectProperty<SecondsCronPickerFieldItem, bool> IsRangeLimitModeProperty = AvaloniaProperty.RegisterDirect<SecondsCronPickerFieldItem, bool>(nameof(IsRangeLimitMode), o => o.IsRangeLimitMode, (o, v) => o.IsRangeLimitMode = v, defaultBindingMode: BindingMode.TwoWay);
    public static readonly DirectProperty<SecondsCronPickerFieldItem, bool> IsIntervalModeProperty = AvaloniaProperty.RegisterDirect<SecondsCronPickerFieldItem, bool>(nameof(IsIntervalMode), o => o.IsIntervalMode, (o, v) => o.IsIntervalMode = v, defaultBindingMode: BindingMode.TwoWay);
    public static readonly DirectProperty<SecondsCronPickerFieldItem, bool> IsListModeProperty = AvaloniaProperty.RegisterDirect<SecondsCronPickerFieldItem, bool>(nameof(IsListMode), o => o.IsListMode, (o, v) => o.IsListMode = v, defaultBindingMode: BindingMode.TwoWay);
    public static readonly StyledProperty<byte> RangeStartProperty = AvaloniaProperty.Register<SecondsCronPickerFieldItem, byte>(nameof(RangeStart), defaultBindingMode: BindingMode.TwoWay);
    public static readonly StyledProperty<byte> RangeEndProperty = AvaloniaProperty.Register<SecondsCronPickerFieldItem, byte>(nameof(RangeEnd), defaultBindingMode: BindingMode.TwoWay);
    public static readonly StyledProperty<int> IntervalStartSecondProperty = AvaloniaProperty.Register<SecondsCronPickerFieldItem, int>(nameof(IntervalStartSecond), defaultBindingMode: BindingMode.TwoWay);
    public static readonly StyledProperty<int> IntervalValueProperty = AvaloniaProperty.Register<SecondsCronPickerFieldItem, int>(nameof(IntervalValue), defaultBindingMode: BindingMode.TwoWay);
    public static readonly DirectProperty<SecondsCronPickerFieldItem, ObservableCollection<int>> AllowedSecondsProperty = AvaloniaProperty.RegisterDirect<SecondsCronPickerFieldItem, ObservableCollection<int>>(nameof(AllowedSeconds), o => o.AllowedSeconds, (o, v) => { o.AllowedSeconds = v; });
    public static readonly DirectProperty<SecondsCronPickerFieldItem, ObservableCollection<int>> SelectedSecondsProperty = AvaloniaProperty.RegisterDirect<SecondsCronPickerFieldItem, ObservableCollection<int>>(nameof(SelectedSeconds), o => o.SelectedSeconds, (o, v) => { o.SelectedSeconds = v; });
    
    private ObservableCollection<int> _selectedSeconds;
    private ObservableCollection<int> _allowedSeconds;

    public SecondsCronPickerFieldItem()
    {
        _allowedSeconds = new ObservableCollection<int>(Enumerable.Range(0, 60));
        _selectedSeconds = new ObservableCollection<int>();
        WeakReference<SecondsCronPickerFieldItem> weakReference = new WeakReference<SecondsCronPickerFieldItem>(this);
        _selectedSeconds.CollectionChanged += (sender, args) =>
        {
            if (weakReference.TryGetTarget(out SecondsCronPickerFieldItem? target))
            {
                target.OnSelectedSecondsChanged(args);
            }
        };
        IsAllowSpecialValue = true; // 允许指定值
    }

    /// <summary>
    /// Whether the field is in every second mode.
    /// </summary>
    public bool IsEverySecondMode
    {
        get => IsModeSelected(1);
        private set => SwitchCurrentModeTo(IsEverySecondModeProperty, 1, value);
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
    public byte RangeStart
    {
        get => GetValue(RangeStartProperty);
        set => SetValue(RangeStartProperty, value);
    }
    
    /// <summary>
    /// The end of the range.
    /// </summary>
    public byte RangeEnd
    {
        get => GetValue(RangeEndProperty);
        set => SetValue(RangeEndProperty, value);
    }
    
    /// <summary>
    /// The start of the interval.
    /// </summary>
    public int IntervalStartSecond
    {
        get => GetValue(IntervalStartSecondProperty);
        set => SetValue(IntervalStartSecondProperty, value);
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
    /// The selected seconds.
    /// </summary>
    public ObservableCollection<int> SelectedSeconds
    {
        get => _selectedSeconds;
        private set => SetAndRaise(SelectedSecondsProperty, ref _selectedSeconds, value);
    }

    /// <summary>
    /// The allowed values for the seconds field.
    /// </summary>
    public ObservableCollection<int> AllowedSeconds
    {
        get => _allowedSeconds;
        private set => SetAndRaise(AllowedSecondsProperty, ref _allowedSeconds, value);
    }

    protected override string DictKey { get; } = nameof(SecondsCronPickerFieldItem);

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
            this.SetValueInterval(this.IntervalStartSecond, this.IntervalValue);
        }
        else if (property == IsListModeProperty)
        {
            this.RefreshByListMode();
        }
        else if (property == IsEverySecondModeProperty)
        {
            this.SetValueAnyMode();
        }
    }

    protected override void ChangeCurrentModeToAnyMode() => /*IsEverySecondMode = true*/this.SetValue(IsEverySecondModeProperty, true);

    protected override void UnSafeChangeCurrentModeToRange(int leftParam, int rightParam)
    {
        ThrowIfRangeValueInvalid(leftParam, rightParam);
        RangeStart = (byte)leftParam;
        RangeEnd = (byte)rightParam;
        this.SetValue(IsRangeLimitModeProperty, true) /*IsRangeLimitMode = true*/;
    }
    
    protected override void UnSafeChangeCurrentModeToInterval(int startParam, int intervalParam)
    {
        ThrowIfIntervalValueInvalid(startParam, intervalParam);
        IntervalStartSecond = startParam;
        IntervalValue = intervalParam;
        this.SetValue(IsIntervalModeProperty, true) /*IsIntervalMode = true*/;
    }
    
    protected override void UnSafeChangeCurrentModeToList(int[] valuesParam)
    {
        ThrowIfListValueInvalid(valuesParam);
        SelectedSeconds.Clear();
        foreach (int value in valuesParam)
        {
            SelectedSeconds.Add(value);
        }
        this.SetValue(IsListModeProperty, true) /*IsListMode = true*/;
    }
    
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SelectedSecondsProperty)
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
        else if (change.Property == IntervalStartSecondProperty || change.Property == IntervalValueProperty)
        {
            if (IsIntervalMode)
            {
                this.SetValueInterval(this.IntervalStartSecond, this.IntervalValue);
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

        int min = this.AllowedSeconds.Min();
        int max = this.AllowedSeconds.Max();
        if (arr.Min() < min || arr.Max() > max)
        {
            throw GetParseException_EnumeratorOutOfRange(min, max);
        }
    }

    protected override void ThrowIfRangeValueInvalid(int start, int end)
    {
        int min = this.AllowedSeconds.Min();
        int max = this.AllowedSeconds.Max();
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
        int min = this.AllowedSeconds.Min();
        int max = this.AllowedSeconds.Max();
        if (start < min || start >= max)
        {
            throw GetParseException_IntervalLeftOutOfRange(min, max);
        }
        
        if (interval < 1 || interval > max)
        {
            throw GetParseException_IntervalRightOutOfRange(1, max);
        }
    }
    
    private void OnSelectedSecondsChanged(NotifyCollectionChangedEventArgs args) => this.RefreshByListMode();
    
    private void OnIsEverySecondModeChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is bool isEverySecondMode)
        {
            
        }
    }
    
    private void RefreshByListMode()
    {
        if (IsListMode)
        {
            int[]? arr = SelectedSeconds?.ToArray();
            if (arr != null && arr.Length > 0)
            {
                if (arr.Length == this.AllowedSeconds.Count && arr.Distinct().Count() == this.AllowedSeconds.Count)
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