using System.ComponentModel;

namespace Ursa.Controls;

public interface ICronRuler : INotifyPropertyChanged
{
    /// <summary>
    /// 所属规则类型
    /// </summary>
    CronFieldTypes FieldType { get; }

    /// <summary>
    /// 符号(用作表达式构建标识符) 
    /// </summary>
    string Symbol { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    object? Header { get; set; }

    /// <summary>
    /// 标题名称
    /// </summary>
    string RulerName { get; set; }

    /// <summary>
    ///  标题相对于内容的位置关系
    /// </summary>
    Avalonia.Controls.Dock HeaderPlacement { get; set; }

    /// <summary>
    /// 优先级, 用于排序解析，值越小越优先
    /// </summary>
    int Priority { get; set; }
    
    /// <summary>
    /// 规则代码
    /// </summary>
    int Code { get; set; }

    void SetFieldType(CronFieldTypes fieldType);
}

public interface ICornRulerViewModel
{
}