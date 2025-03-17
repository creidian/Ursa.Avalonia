namespace Ursa.Controls;

/// <summary>
/// cron表达式字段类型
/// </summary>
public enum CronFieldTypes : byte
{
    /// <summary>
    /// 未知
    /// </summary>
    Unknown = 0,
    /// <summary>
    /// 秒
    /// </summary>
    Second = 1,
    /// <summary>
    /// 分
    /// </summary>
    Minute = 2,
    /// <summary>
    /// 时
    /// </summary>
    Hour = 3,
    /// <summary>
    /// 日
    /// </summary>
    DayOfMonth = 4,
    /// <summary>
    /// 月
    /// </summary>
    Month = 5,
    /// <summary>
    /// 周
    /// </summary>
    DayOfWeek = 6,
    /// <summary>
    /// 年
    /// </summary>
    Year = 7
}