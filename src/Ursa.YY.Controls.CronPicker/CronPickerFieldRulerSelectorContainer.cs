using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;

namespace Ursa.Controls;

public class CronPickerFieldRulerSelectorContainer : ListBoxItem /*ContentPresenter*/, ISelectableContainer
{
    public static readonly RoutedEvent<RoutedEventArgs> SelectedChangedEvent = RoutedEvent.Register<CronPickerFieldRulerSelectorContainer, RoutedEventArgs>(nameof(SelectedChanged), RoutingStrategies.Bubble);

    public event EventHandler<RoutedEventArgs>? SelectedChanged
    {
        add => AddHandler(SelectedChangedEvent, value);
        remove => RemoveHandler(SelectedChangedEvent, value);
    }
    
    internal ICronPickerRulerItem? GetRuler()
    {
        if (this.Content is ICronPickerRulerItem ruler)
        {
            return ruler;
        }

        if (this.Content is Control control)
        {
            ICronPickerRulerItem? re = GetChild<ICronPickerRulerItem>(control);
            if (re is not null)
            {
                return re;
            }
        }

        IAvaloniaList<ILogical> logicalChildren = this.LogicalChildren;
        foreach (ILogical child in logicalChildren)
        {
            if (child is ICronPickerRulerItem ruler2)
            {
                return ruler2;
            }

            if (child is Control control2)
            {
                ICronPickerRulerItem? re = GetChild<ICronPickerRulerItem>(control2);
                if (re is not null)
                {
                    return re;
                }
            }
        }
        return null;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsSelectedProperty)
        {
            this.OnSelectedChanged();
        }
    }

    private void OnSelectedChanged()
    {
        ICronPickerRulerSelector? selector = GetRulerSelector();
        selector?.Select(this.IsSelected);
        this.RaiseEvent(new RoutedEventArgs(SelectedChangedEvent));
    }

    private ICronPickerRulerSelector? GetRulerSelector()
    {
        if (this.Content is ICronPickerRulerSelector ruler)
        {
            return ruler;
        }

        if (this.Content is Control control)
        {
            ICronPickerRulerSelector? re = GetChild<ICronPickerRulerSelector>(control);
            if (re is not null)
            {
                return re;
            }
        }

        IAvaloniaList<ILogical> logicalChildren = this.LogicalChildren;
        foreach (ILogical child in logicalChildren)
        {
            if (child is ICronPickerRulerSelector ruler2)
            {
                return ruler2;
            }

            if (child is Control control2)
            {
                ICronPickerRulerSelector? re = GetChild<ICronPickerRulerSelector>(control2);
                if (re is not null)
                {
                    return re;
                }
            }
        }
        return null;
    }

    private CronPickerFieldView? GetParentField()
    {
        StyledElement? parent = this.Parent;
        while (parent is not null)
        {
            if (parent is CronPickerFieldView field)
            {
                return field;
            }

            parent = parent.Parent;
        }

        return null;
    }
    
    private T? GetChild<T>(Control control)
    {
        var child = control.GetLogicalChildren().ToArray();
        foreach (var item in child)
        {
            if (item is T t)
            {
                return t;
            }
        }
        
        foreach (ILogical? item in child)
        {
            if (item is Control childControl)
            {
                var result = GetChild<T>(childControl);
                if (result is not null)
                {
                    return result;
                }
            }
        }
        
        return default;
    }
}