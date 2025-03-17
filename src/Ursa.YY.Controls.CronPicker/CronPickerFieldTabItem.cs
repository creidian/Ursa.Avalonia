using Avalonia;
using Avalonia.Controls;

namespace Ursa.Controls;

public class CronPickerFieldTabItem : TabItem
{
    public static readonly StyledProperty<CronFieldTypes> FieldTypeProperty = AvaloniaProperty.Register<CronPickerFieldTabItem, CronFieldTypes>(nameof(FieldType));
    
    public CronFieldTypes FieldType
    {
        get => GetValue(FieldTypeProperty);
        set => SetValue(FieldTypeProperty, value);
    }

    public CronPickerFieldTabItem()
    {
    }
}