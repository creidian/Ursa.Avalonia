namespace Ursa.Controls;

/// <summary>
/// 表达式解析器接口
/// </summary>
public interface ICronExpressionParser
{
    /// <summary>
    /// 解析表达式并返回下一次执行时间
    /// </summary>
    /// <param name="expression"> 表达式 </param>
    /// <param name="startDate"> 开始时间 </param>
    /// <returns> 下一次执行时间 </returns>
    DateTime GetNextTime(string expression, DateTime startDate);
}