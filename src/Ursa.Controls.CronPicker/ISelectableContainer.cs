using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Ursa.Controls;

public interface ISelectableContainer : ISelectable
{
    event EventHandler<RoutedEventArgs>? SelectedChanged;
}