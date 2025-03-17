using Avalonia.Interactivity;

namespace Ursa.Controls;

public class UsualValueChangedEventArgs : RoutedEventArgs
{
    public UsualValueChangedEventArgs(RoutedEvent routedEvent, object? oldValue, object? newValue) : base(routedEvent)
    {
        OldValue = oldValue;
        NewValue = newValue;
    }

    public object? OldValue { get; }
    public object? NewValue { get; }
}
public class UsualValueChangedEventArgs<T> : RoutedEventArgs
{
    public UsualValueChangedEventArgs(RoutedEvent routedEvent, T? oldValue, T? newValue) : base(routedEvent)
    {
        OldValue = oldValue;
        NewValue = newValue;
    }

    public T? OldValue { get; }
    public T? NewValue { get; }
}