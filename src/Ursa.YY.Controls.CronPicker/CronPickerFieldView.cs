using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Ursa.Controls;

public class CronPickerFieldView : ListBox/*SelectingItemsControl*/, ISelectableRulerItemContainer, ICronPickerRulersContainer
{
    public static readonly RoutedEvent<UsualValueChangedEventArgs<string>> ValueChangedEvent = RoutedEvent.Register<CronPickerFieldView, UsualValueChangedEventArgs<string>>(nameof(ValueChanged), RoutingStrategies.Bubble);
    public static readonly StyledProperty<CronFieldTypes> FieldTypeProperty = AvaloniaProperty.Register<CronPickerFieldView, CronFieldTypes>(nameof(FieldType));
    // public static readonly StyledProperty<DataTemplates?> CronRulerItemDataTemplatesProperty = AvaloniaProperty.Register<CronPickerFieldView, DataTemplates?>(nameof(CronRulerItemDataTemplates));
    public static readonly StyledProperty<string> FieldNameProperty = AvaloniaProperty.Register<CronPickerFieldView, string>(nameof(FieldName));
    public static readonly StyledProperty<IDataTemplate> RulerItemContentTemplateProperty = AvaloniaProperty.Register<CronPickerFieldView, IDataTemplate>(nameof(RulerItemContentTemplate));
    private string _value;

    static CronPickerFieldView()
    {
        // KeyboardNavigation.TabNavigationProperty.OverrideDefaultValue(typeof (CronPickerFieldView), KeyboardNavigationMode.Once);
    }

    public CronPickerFieldView()
    {
        this.SelectionMode = SelectionMode.Single;
    }

    public event EventHandler<UsualValueChangedEventArgs<string>>? ValueChanged
    {
        add => AddHandler(ValueChangedEvent, value);
        remove => RemoveHandler(ValueChangedEvent, value);
    }
    
    public CronFieldTypes FieldType
    {
        get => GetValue(FieldTypeProperty);
        set => SetValue(FieldTypeProperty, value);
    }

    public string FieldName 
    {
        get => GetValue(FieldNameProperty);
        set => SetValue(FieldNameProperty, value);
    }
    
    public string Value => _value;

    public IDataTemplate RulerItemContentTemplate
    {
        get => GetValue(RulerItemContentTemplateProperty);
        set => SetValue(RulerItemContentTemplateProperty, value);
    }
    
    /*public DataTemplates? CronRulerItemDataTemplates
    {
        get => this.GetValue(CronRulerItemDataTemplatesProperty);
        set => this.SetValue(CronRulerItemDataTemplatesProperty, value);
    }*/

    internal CronPickerRulerSelector? SelectedRuler { get; private set; }

    public CronFieldTypes GetFieldType() => this.FieldType;

    public void ParsetoValue(string cronFieldString)
    {
        if (this.SelectedRuler is not null)
        {
            ICronPickerRulerItem? ruler = this.SelectedRuler.GetRuler();
            if (ruler is not null)
            {
                try
                {
                    CronParseResult result= ruler.ParseTo(cronFieldString);
                    if (result.IsSuccessed)
                    {
                        return;
                    }
                }
                catch (Exception e)
                {
                    // 忽略其他异常
                }
            }
        }
        
        (ICronPickerRulerItem Ruler, CronPickerRulerSelector Selector)[] array = this.RulerItems().OrderBy(x => x.Item1.Priority).ToArray();
        foreach ((ICronPickerRulerItem Ruler, CronPickerRulerSelector Selector) item in array)
        {
            try
            {
                CronParseResult result= item.Ruler.ParseTo(cronFieldString);
                if (result.IsSuccessed)
                {
                    item.Selector.IsSelected = true;
                    return;
                }

                if (result.Code is ParseResultTypeCodes.UnSupported)
                {
                    continue;
                }

                if (result.Code is ParseResultTypeCodes.FormatError)
                {
                    throw new FormatException(result.Message);
                }
            }
            catch (NotSupportedException e)
            {
                // 忽略不支持的表达式
            }
            catch (FormatException)
            {
                throw;
            }
            catch (Exception e)
            {
                // 忽略其他异常
            }
        }
        
        throw new FormatException("Unsupport cron field type.");
    }
    
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return this.NeedsContainer<CronPickerRulerSelector>(item, out recycleKey);
    }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        if (item is CronPickerRuler ruler)
        {
            object? rulerContent = CreateRulerItemContent(ruler, RulerItemContentTemplate) /*ruler*/;
            return new CronPickerRulerSelector
            {
                Content = rulerContent/*ruler*/ /*RulerItemContentTemplate.Build(ruler)*/ ,
                // [!CronPickerRulerSelector.CronRulerItemDataTemplatesProperty] = this[!CronPickerFieldView.CronRulerItemDataTemplatesProperty],
                [!CronPickerRulerSelector.FieldTypeProperty] = ruler[!CronPickerRuler.FieldTypeProperty],
                [!CronPickerRulerSelector.PriorityProperty] = ruler[!CronPickerRuler.PriorityProperty],
                [!CronPickerRulerSelector.RulerCodeProperty] = ruler[!CronPickerRuler.CodeProperty],
                [!CronPickerRulerSelector.StripPlacementProperty] = ruler[!CronPickerRuler.HeaderPlacementProperty],
                [!CronPickerRulerSelector.SymbolProperty] = ruler[!CronPickerRuler.SymbolProperty],
                [!CronPickerRulerSelector.HeaderProperty] = ruler[!CronPickerRuler.HeaderProperty],
                [!CronPickerRulerSelector.RulerNameProperty] = ruler[!CronPickerRuler.RulerNameProperty],
                // [!CronPickerRulerSelector.ContentTemplateProperty] = ruler[!CronPickerRuler.ContentTemplateProperty],
                [!CronPickerRulerSelector.ContentTemplateProperty] = this[!RulerItemContentTemplateProperty],
            };
        }

        return base.CreateContainerForItemOverride(item, index, recycleKey);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ItemsSourceProperty)
        {
            this.SelectedIndex = 0;
        }
    }

    bool ISelectableRulerItemContainer.UpdateSelectionFromPointerEvent(Control source, PointerEventArgs e)
    {
        Control containerFromEventSource = this.GetContainerFromEventSource(source);
        if (containerFromEventSource == null)
        {
            return false;
        }
        int index = this.IndexFromContainer(containerFromEventSource);
        this.Selection.Clear();
        this.Selection.Select(index);
        return true;
    }

    public void OnValueChange(bool isSelected, string rulerValue)
    {
        if (isSelected)
        {
            this.SetValue(rulerValue);
        }
    }

    public void OnSelectedStatusChanged(CronPickerRulerSelector ruler, bool isSelected, string rulerValue)
    {
        if (isSelected)
        {
            this.SetValue(rulerValue);
            this.SelectedRuler = ruler;
        }
        else if (this.SelectedIndex < 0)
        {
            this.SetValue(string.Empty);
            this.SelectedRuler = null;
        }
    }

    public void VerifyCurrentCronValue()
    {
        CronPickerRulerSelector? ruler = this.SelectedRuler;
        if (ruler is not null)
        {
            ruler.VerifyCurrentCronValue();
        }
    }

    protected override void LogicalChildrenCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        base.LogicalChildrenCollectionChanged(sender, e);
    }

    private IEnumerable<(ICronPickerRulerItem Ruler, CronPickerRulerSelector Selector)> RulerItems()
    {
        CronPickerRulerSelector[] array = this.LogicalChildren.OfType<CronPickerRulerSelector>().ToArray();
        foreach (CronPickerRulerSelector item in array)
        {
            ICronPickerRulerItem? ruler = item.GetRuler();
            if (ruler is not null)
            {
                yield return (ruler, item);
            }
        }
    }

    private void SetValue(string value)
    {
        if (!string.Equals(value, _value))
        {
            string oldValue = _value;
            this._value = value;
            this.RaiseEvent(new UsualValueChangedEventArgs<string>(ValueChangedEvent, oldValue, value));
        }
    }
    
    private Control? CreateRulerItemContent(object? content, IDataTemplate? template)
    {
        Control? newChild = content as Control;
        if ((newChild == null && (content != null || template != null)) || (newChild is { } && template is { }))
        {
            IDataTemplate dataTemplate = this.FindDataTemplate(content, template)?? FuncDataTemplate.Default;
            newChild = dataTemplate.Build(content);
        }
        
        return newChild;
    }
}