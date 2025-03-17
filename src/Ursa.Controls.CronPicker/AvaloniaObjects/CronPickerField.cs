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

    public static readonly StyledProperty<IEnumerable<string>> TestDataProperty =
        AvaloniaProperty.Register<CronPickerField, IEnumerable<string>>(nameof(TestData));
    
    private CronPickerField(CronFieldTypes fieldType)
    {
        FieldType = fieldType;
        RulerItems = new CronPickerRulers();
        WeakReference<CronPickerField> weakReference = new WeakReference<CronPickerField>(this);
        RulerItems.CollectionChanged += (sender, e) =>
        {
            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems is not null)
            {
                foreach (CronPickerRuler item in e.NewItems)
                {
                    item.SetFieldType(FieldType);
                }
            }
        };
        switch (fieldType)
        {
            case CronFieldTypes.Minute:
            case CronFieldTypes.Second:
                TestData = new List<string>() { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "40", "41", "42", "43", "44", "45", "46", "47", "48", "49", "50", "51", "52", "53", };
                break;
            case CronFieldTypes.Hour:
                TestData = new List<string>() { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", };
                break;
            case CronFieldTypes.DayOfMonth:
                TestData = new List<string>() { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31", };
                break;
            case CronFieldTypes.Month:
                TestData = new List<string>() { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", };
                break;
            case CronFieldTypes.DayOfWeek:
                TestData = new List<string>() { "1", "2", "3", "4", "5", "6", "7", };
                break;
            case CronFieldTypes.Year:
                TestData = new List<string>() { "2022", "2023", "2024", "2025", "2026", "2027", "2028", "2029", "2030", "2031", "2032", "2033", "2034", "2035", "2036", "2037", "2038", "2039", "2040", "2041", "2042", "2043", "2044", "2045", "2046", "2047", "2048", "2049", "2050", "2051", "2052", "2053" };
                break;
            default:
                break;
        }
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

    public IEnumerable<string> TestData
    {
        get => GetValue(TestDataProperty);
        private set => SetValue(TestDataProperty, value);
    }

    public static CronPickerField Create(CronFieldTypes fieldType) => new(fieldType);
}
/// <summary>
/// Cron 规则内容集合
/// </summary>
public class CronPickerRulers : AvaloniaList<CronPickerRuler>
{
    public CronPickerRulers()
    {
        this.ResetBehavior = ResetBehavior.Remove;
    }
}