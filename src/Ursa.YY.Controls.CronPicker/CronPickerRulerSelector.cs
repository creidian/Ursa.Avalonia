using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Irihi.Avalonia.Shared.Helpers;

namespace Ursa.Controls;

[Avalonia.Controls.Metadata.PseudoClasses(new string[] {":pressed", ":selected"})]
[TemplatePart(PART_SelectRootBorder, typeof (Border))]
[TemplatePart(PART_HeaderPresenter, typeof(ContentPresenter))]
public class CronPickerRulerSelector : /*HeaderedContentControl*/ ListBoxItem, ISelectable, ICronPickerRulerItemParent, ICronPickerRulerSelector
{
    public const string PART_SelectRootBorder = "PART_LayoutRoot";
    public const string PART_HeaderPresenter = "PART_HeaderPresenter";
    
    public static readonly StyledProperty<int> PriorityProperty = AvaloniaProperty.Register<CronPickerRulerSelector, int>(nameof(Priority));
    public static readonly StyledProperty<string> RulerNameProperty = AvaloniaProperty.Register<CronPickerRulerSelector, string>(nameof(RulerName));
    public static readonly DirectProperty<CronPickerRulerSelector, string?> DescriptionProperty = AvaloniaProperty.RegisterDirect<CronPickerRulerSelector, string?>(nameof(Description), o => o.Description, (o, v) => o.Description = v);
    public static readonly DirectProperty<CronPickerRulerSelector, string> SymbolProperty = AvaloniaProperty.RegisterDirect<CronPickerRulerSelector, string>(nameof(Symbol), o => o.Symbol, (o, v) => o.Symbol = v);
    public static readonly DirectProperty<CronPickerRulerSelector, string> ValueStringProperty = AvaloniaProperty.RegisterDirect<CronPickerRulerSelector, string>(nameof(ValueString), o => o.ValueString);
    public static readonly DirectProperty<CronPickerRulerSelector, CronFieldTypes> FieldTypeProperty = AvaloniaProperty.RegisterDirect<CronPickerRulerSelector, CronFieldTypes>(nameof(FieldType), o => o.FieldType, (o, v) => o.FieldType = v);
    public static readonly DirectProperty<CronPickerRulerSelector, Dock> StripPlacementProperty = AvaloniaProperty.RegisterDirect<CronPickerRulerSelector, Dock>(nameof (StripPlacement), (Func<CronPickerRulerSelector, Dock>) (o => o.StripPlacement), (o, v) => o.StripPlacement = v);
    public static readonly StyledProperty<bool> IsSelectedProperty = ListBoxItem.IsSelectedProperty;/*SelectingItemsControl.IsSelectedProperty.AddOwner<CronPickerRulerSelector>();*//*AvaloniaProperty.Register<CronPickerRulerSelector, bool>(nameof(IsSelected));*/
    public static readonly RoutedEvent<TextChangingEventArgs> ValueChangingEvent = RoutedEvent.Register<CronPickerRulerSelector, TextChangingEventArgs>(nameof(ValueChanging), RoutingStrategies.Bubble);
    public static readonly RoutedEvent<TextChangedEventArgs> ValueChangedEvent = RoutedEvent.Register<CronPickerRulerSelector, TextChangedEventArgs>(nameof(ValueChanged), RoutingStrategies.Bubble);
    public static readonly RoutedEvent<CronPickerRulerSelectStatusChangingEventArgs> SelectionChangingEvent = RoutedEvent.Register<CronPickerRulerSelector, CronPickerRulerSelectStatusChangingEventArgs>(nameof(SelectionChanging), RoutingStrategies.Bubble);
    public static readonly RoutedEvent<RoutedEventArgs> SelectionChangedEvent = RoutedEvent.Register<CronPickerRulerSelector, RoutedEventArgs>(nameof(SelectionChanged), RoutingStrategies.Bubble);
    public static readonly StyledProperty<int> RulerCodeProperty = AvaloniaProperty.Register<CronPickerRulerSelector, int>(nameof(RulerCode));
    public static readonly StyledProperty<DataTemplates?> CronRulerItemDataTemplatesProperty = AvaloniaProperty.Register<CronPickerRulerSelector, DataTemplates?>(nameof(CronRulerItemDataTemplates));
    public static readonly StyledProperty<IDataTemplate?> HeaderTemplateProperty = AvaloniaProperty.Register<CronPickerRulerSelector, IDataTemplate?>(nameof(HeaderTemplate));
    public static readonly StyledProperty<object?> HeaderProperty = AvaloniaProperty.Register<CronPickerRulerSelector, object?>(nameof(Header));
    private Dock _stripPlacement;
    private Border? _selectBorder;
    private static readonly Point s_invalidPoint = new (double.NaN, double.NaN);
    private Point _pointerDownPoint = s_invalidPoint;
    private string? _description;
    private string _symbol;
    private string _valueString;
    private CronFieldTypes _fieldType;

    static CronPickerRulerSelector()
    {
        /*SelectableMixin.Attach<CronPickerRulerSelector>(IsSelectedProperty);
        PressedMixin.Attach<CronPickerRulerSelector>();
        FocusableProperty.OverrideDefaultValue(typeof(CronPickerRulerSelector), true);
        IsSelectedProperty.AffectsPseudoClass<CronPickerRulerSelector>(PseudoClassName.PC_Selected);
        */
        IsSelectedProperty.Changed.AddClassHandler<CronPickerRulerSelector, bool>((item, args) => item.OnSelectionChanged(args));
        HeaderProperty.Changed.AddClassHandler<CronPickerRulerSelector>((x, e) => x.HeaderChanged(e));
    }

    public event EventHandler<CronPickerRulerSelectStatusChangingEventArgs>? SelectionChanging
    {
        add => this.AddHandler(SelectionChangingEvent, value);
        remove => this.RemoveHandler(SelectionChangingEvent, value);
    }
    
    public event EventHandler<RoutedEventArgs>? SelectionChanged
    {
        add => this.AddHandler(SelectionChangedEvent, value);
        remove => this.RemoveHandler(SelectionChangedEvent, value);
    }
    
    public event EventHandler<TextChangingEventArgs>? ValueChanging
    {
        add => this.AddHandler(ValueChangingEvent, value);
        remove => this.RemoveHandler(ValueChangingEvent, value);
    }
    
    public event EventHandler<TextChangedEventArgs>? ValueChanged
    {
        add => this.AddHandler(ValueChangedEvent, value);
        remove => this.RemoveHandler(ValueChangedEvent, value);
    }

    public DataTemplates? CronRulerItemDataTemplates
    {
        get => this.GetValue(CronRulerItemDataTemplatesProperty);
        set => this.SetValue(CronRulerItemDataTemplatesProperty, value);
    }
    
    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }
    
    public IDataTemplate? HeaderTemplate
    {
        get => GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }
    
    public int RulerCode
    {
        get => GetValue(RulerCodeProperty);
        set => SetValue(RulerCodeProperty, value);
    }
    
    public CronFieldTypes FieldType
    {
        get => _fieldType;
        set => SetAndRaise(FieldTypeProperty, ref _fieldType, value);
    }

    public int Priority
    {
        get => GetValue(PriorityProperty);
        set => SetValue(PriorityProperty, value);
    }

    public new bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => /*this.OnSelectionChange(value)*/SetValue(IsSelectedProperty, value);
    }

    public Dock StripPlacement
    {
        get => _stripPlacement; 
        set => SetAndRaise(StripPlacementProperty, ref _stripPlacement, value);
    }

    public string RulerName
    {
        get => GetValue(RulerNameProperty);
        set => SetValue(RulerNameProperty, value);
    }

    public string? Description
    {
        get => _description;
        set => SetAndRaise(DescriptionProperty, ref _description, value);
    }

    public string Symbol
    {
        get => _symbol;
        set => SetAndRaise(SymbolProperty, ref _symbol, value);
    }

    public string ValueString
    {
        get => _valueString;
        private set => SetAndRaise(ValueStringProperty, ref _valueString, value);
    }
    
    public ContentPresenter? HeaderPresenter
    {
        get;
        private set;
    }

    public string GetExpression() => ValueString;
    
    public void Select(bool isSelected)
    {
        this.IsSelected = isSelected;
    }
    
    public void VerifyCurrentCronValue()
    {
        this.GetRuler()?.VerifyCurrentCronValue();
    }
    
    void ICronPickerRulerItemParent.ValueChanged(string value) => this.ValueString = value;

    internal ICronPickerRulerItem? GetRuler() => this.LogicalChildren?.OfType<ICronPickerRulerItem>()?.FirstOrDefault();

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        Border.PointerPressedEvent.RemoveHandler(OnSelectPartPointerPressed, _selectBorder);
        Border.PointerReleasedEvent.RemoveHandler(OnSelectPartPointerReleased, _selectBorder);
        _selectBorder = e.NameScope.Find<Border>(PART_SelectRootBorder);
        Border.PointerPressedEvent.AddHandler(OnSelectPartPointerPressed, _selectBorder);
        Border.PointerReleasedEvent.AddHandler(OnSelectPartPointerReleased, _selectBorder);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ValueStringProperty)
        {
            ISelectableRulerItemContainer? owner = GetSelectableOwner();
            if (owner is not null)
            {
                owner.OnValueChange(this.IsSelected, change.GetNewValue<string>());
            }

            RaiseValueChangeEvents();
        }
        else if (change.Property == CronRulerItemDataTemplatesProperty)
        {
            this.UpdateCronRulerItemDataTemplates(change.OldValue as DataTemplates, change.NewValue as DataTemplates);
        }
    }

    protected override void LogicalChildrenCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        base.LogicalChildrenCollectionChanged(sender, e);
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            ICronPickerRulerItem? newItem = e.NewItems?.OfType<ICronPickerRulerItem>()?.FirstOrDefault();
            if (newItem is not null)
            {
                newItem.ValueChanged += ChildOnValueChanged;
                this.ValueString = newItem.Value;
            }
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Reset)
        {
            ICronPickerRulerItem? oldItem = e.OldItems?.OfType<ICronPickerRulerItem>()?.FirstOrDefault();
            if (oldItem is not null)
            {
                oldItem.ValueChanged -= ChildOnValueChanged;
            }
        }
    }

    /// <inheritdoc/>
    protected override bool RegisterContentPresenter(ContentPresenter presenter)
    {
        bool result = base.RegisterContentPresenter(presenter);
        if (presenter.Name == PART_HeaderPresenter)
        {
            HeaderPresenter = presenter;
            result = true;
        }

        return result;
    }

    private void HeaderChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.OldValue is ILogical oldChild)
        {
            LogicalChildren.Remove(oldChild);
        }

        if (e.NewValue is ILogical newChild)
        {
            LogicalChildren.Add(newChild);
        }
    }

    private void ChildOnValueChanged(object sender, UsualValueChangedEventArgs<string> e)
    {
        this.ValueString = e.NewValue;
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
        else
        {
            this.DataTemplates.Clear();
        }
    }

    /*private void OnValueChange(object sender, TextChangedEventArgs e)
    {
        this.ValueString = _cronRuler?.ValueString ?? string.Empty;
    }*/

    private void RaiseValueChangeEvents()
    {
        this.RaiseEvent((RoutedEventArgs) new TextChangingEventArgs((RoutedEvent) ValueChangingEvent));
        Dispatcher.UIThread.Post((Action) (() => this.RaiseEvent((RoutedEventArgs) new TextChangedEventArgs((RoutedEvent) ValueChangedEvent))), DispatcherPriority.Normal);
    }
    
    private void OnSelectPartPointerReleased(object sender, PointerReleasedEventArgs e)
    {
        if (!e.Handled && !double.IsNaN(_pointerDownPoint.X) && e.InitialPressMouseButton is MouseButton.Left or MouseButton.Right)
        {
            var point = e.GetCurrentPoint(this);
            var settings = TopLevel.GetTopLevel(e.Source as Visual)?.PlatformSettings;
            var tapSize = settings?.GetTapSize(point.Pointer.Type) ?? new Size(4, 4);
            var tapRect = new Rect(_pointerDownPoint, new Size())
                .Inflate(new Thickness(tapSize.Width, tapSize.Height));

            ISelectableRulerItemContainer? owner = GetSelectableOwner();
            if (new Rect(Bounds.Size).ContainsExclusive(point.Position) &&
                tapRect.ContainsExclusive(point.Position) &&
                owner is not null)
            {
                if (owner.UpdateSelectionFromPointerEvent(this, e))
                {
                    // As we only update selection from touch/pen on pointer release, we need to raise
                    // the pointer event on the owner to trigger a commit.
                    if (e.Pointer.Type != PointerType.Mouse)
                    {
                        object? sourceBackup = e.Source;
                        // owner.RaiseEvent(e);
                        e.Source = sourceBackup;
                    }

                    e.Handled = true;
                }
            }
        }

        _pointerDownPoint = s_invalidPoint;
        /*if (sender is Border control &&  !e.Handled && !double.IsNaN(_pointerDownPoint.X) && e.InitialPressMouseButton is MouseButton.Left or MouseButton.Right)
        {
            var point = e.GetCurrentPoint(control);
            if (new Rect(Bounds.Size).ContainsExclusive(point.Position) && e.Pointer.Type == PointerType.Touch)
            {
                // this.OnSelectionChange(!this.IsSelected);
                e.Handled = true;
            }
        }*/
    }

    private void OnSelectPartPointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (sender is Border control)
        {
            _pointerDownPoint = e.GetPosition(control);
            if (e.Handled)
            {
                return;
            }
            if (e.GetCurrentPoint(control).Properties.IsLeftButtonPressed)
            {
                PointerPoint p = e.GetCurrentPoint(control);
                if (p.Properties.PointerUpdateKind is PointerUpdateKind.LeftButtonPressed or PointerUpdateKind.RightButtonPressed)
                {
                    if (p.Pointer.Type == PointerType.Mouse)
                    {   
                        ISelectableRulerItemContainer? owner = GetSelectableOwner();
                        if (owner is not null)
                        {
                            e.Handled = owner.UpdateSelectionFromPointerEvent(this, e);
                        }

                        // this.OnSelectionChange(!this.IsSelected);
                        // e.Handled = true;
                    }
                    else
                    {
                        _pointerDownPoint = p.Position;
                    }
                }
            }
        }
    }

    private void OnSelectionChange(bool value)
    {
        if (IsSelected == value)
        {
            return;
        }

        CronPickerRulerSelectStatusChangingEventArgs args = new CronPickerRulerSelectStatusChangingEventArgs(SelectionChangingEvent, IsSelected, value);
        OnSelectionChanging(args);
        if (args.Cancel)
        {
            return;
        }
     
        SetValue(IsSelectedProperty, value);
    }
    
    private void OnSelectionChanging(CronPickerRulerSelectStatusChangingEventArgs args)
    {
        this.RaiseEvent(args);
    }
    
    private void OnSelectionChanged(AvaloniaPropertyChangedEventArgs<bool> args)
    {
        ISelectableRulerItemContainer? owner = GetSelectableOwner();
        if (owner is not null)
        {
            owner.OnSelectedStatusChanged(this, args.GetNewValue<bool>(), this.ValueString);
        }
        
        this.RaiseEvent(new RoutedEventArgs(SelectionChangedEvent));
    }
    
    private ISelectableRulerItemContainer? GetSelectableOwner() => GetParent<ISelectableRulerItemContainer>();

    private ISelectableContainer? GetSelectableContainer() => GetParent<ISelectableContainer>();

    private T? GetParent<T>()
    {
        Control? c = (Control?)this.Parent;
        while (c is not null)
        {
            if (c is T t)
            {
                return t;
            }
            c = (Control?)c.Parent;
        }

        return default;
    }
}

public class CronPickerRulerSelectStatusChangingEventArgs : RoutedEventArgs
{
    public CronPickerRulerSelectStatusChangingEventArgs(RoutedEvent routedEvent, bool oldValue, bool newValue) : base(routedEvent)
    {
        OldValue = oldValue;
        NewValue = newValue;
    }

    public bool Cancel { get; set; }
    public bool OldValue { get; }
    public bool NewValue { get; }
}