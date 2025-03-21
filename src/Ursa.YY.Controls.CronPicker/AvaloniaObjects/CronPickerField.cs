using System.Collections.Specialized;
using Avalonia;
using Avalonia.Collections;

namespace Ursa.Controls;

/// <summary>
/// Cron 子项内容
/// </summary>
public class CronPickerField : AvaloniaObject
{
    public static readonly StyledProperty<CronFieldTypes> FieldTypeProperty =
        AvaloniaProperty.Register<CronPickerField, CronFieldTypes>(nameof(FieldType));

    public static readonly StyledProperty<CronPickerRulers> RulerItemsProperty =
        AvaloniaProperty.Register<CronPickerField, CronPickerRulers>(nameof(RulerItems));

    private CronPickerField(CronFieldTypes fieldType)
    {
        FieldType = fieldType;
        RulerItems = new CronPickerRulers();
        RulerItems.CollectionChanged += (sender, e) =>
        {
            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems is not null)
            {
                foreach (object item in e.NewItems)
                {
                    if (item is ICronRuler ruler)
                    {
                        ruler.SetFieldType(fieldType);
                    }
                }
            }
        };
    }

    /// <summary>
    /// 规则类型
    /// </summary>
    public CronFieldTypes FieldType
    {
        get => GetValue(FieldTypeProperty);
        private set => SetValue(FieldTypeProperty, value);
    }

    /// <summary>
    /// 规则列表
    /// </summary>
    public CronPickerRulers RulerItems
    {
        get => GetValue(RulerItemsProperty);
        private set => SetValue(RulerItemsProperty, value);
    }

    public static CronPickerField Create(CronFieldTypes fieldType) => new(fieldType);
}
/// <summary>
/// Cron 规则内容集合
/// </summary>
public class CronPickerRulers : AvaloniaList<ICronRuler>
{
    public CronPickerRulers()
    {
        this.ResetBehavior = ResetBehavior.Remove;
    }
}