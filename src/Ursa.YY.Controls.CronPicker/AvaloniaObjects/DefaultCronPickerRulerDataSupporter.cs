namespace Ursa.Controls;

/// <summary>
/// 规则类型码
/// </summary>
public class DefaultCronPickerRulerDataSupporter
{
    /// <summary>
    /// 任意
    /// </summary>
    public const int RULER_TYPE_ALL = 0;

    /// <summary>
    /// 范围
    /// </summary>
    public const int RULER_TYPE_RANGE = 1;

    /// <summary>
    /// 间隔
    /// </summary>
    public const int RULER_TYPE_INTERVAL = 2;

    /// <summary>
    /// 枚举值
    /// </summary>
    public const int RULER_TYPE_LISTAND = 3;

    /// <summary>
    /// 最后
    /// </summary>
    public const int RULER_TYPE_LAST = 4;

    /// <summary>
    /// 最近有效工作日
    /// </summary>
    public const int RULER_TYPE_RECENTLY_WORKDAY = 5;

    /// <summary>
    /// 指定星期几
    /// </summary>
    public const int RULER_TYPE_SPECIAL_WEEKDAY = 6;
        
    /// <summary>
    /// 占位
    /// </summary>
    public const int RULER_TYPE_PLACEHOLDER = 7;

    /// <summary>
    /// * 表示匹配该域的任意值。假如在Minutes域使用*, 即表示每分钟都会触发事件。
    /// </summary>
    public const string FIELD_CHAR_ALL = "*";

    /// <summary>
    /// ? 只能用在DayofMonth和DayofWeek两个域。它也匹配域的任意值，但实际不会。因为DayofMonth和DayofWeek会相互影响。例如想在每月的20日触发调度，不管20日到底是星期几，则只能使用如下写法： 13 13 15 20 * ?, 其中最后一位只能用？，而不能使用*，如果使用*表示不管星期几都会触发，实际上并不是这样。
    /// </summary>
    public const string FIELD_CHAR_PLACEHOLDER = "?";

    /// <summary>
    /// - 表示范围。例如在Minutes域使用5-20，表示从5分到20分钟每分钟触发一次
    /// </summary>
    public const string FIELD_CHAR_RANGE = "-";

    /// <summary>
    /// / 表示起始时间开始触发，然后每隔固定时间触发一次。例如在Minutes域使用5/20,则意味着5分钟触发一次，而25，45等分别触发一次.
    /// </summary>
    public const string FIELD_CHAR_INTERVAL = "/";

    /// <summary>
    /// , 表示列出枚举值。例如：在Minutes域使用5,20，则意味着在5和20分每分钟触发一次。
    /// </summary>
    public const string FIELD_CHAR_LIST_AND = ",";

    /// <summary>
    /// L 表示最后，只能出现在DayofWeek和DayofMonth域。如果在DayofWeek域使用5L,意味着在最后的一个星期四触发。
    /// </summary>
    public const string FIELD_CHAR_LAST = "L";

    /// <summary>
    /// W 表示有效工作日(周一到周五),只能出现在DayofMonth域，系统将在离指定日期的最近的有效工作日触发事件。例如：在 DayofMonth使用5W，如果5日是星期六，则将在最近的工作日：星期五，即4日触发。如果5日是星期天，则在6日(周一)触发；如果5日在星期一到星期五中的一天，则就在5日触发。另外一点，W的最近寻找不会跨过月份 。
    /// </summary>
    public const string FIELD_CHAR_WORKDAY = "W";

    /// <summary>
    /// # 表示第几个星期几，只能出现在DayofWeek域。例如在DayofWeek域使用6#3，表示在每月的第三个星期五触发。
    /// </summary>
    public const string FIELD_CHAR_WEEKDAY = "#";
    
    /// <summary>
    /// [0,59]
    /// </summary>
    public static IEnumerable<int> SecondList { get; } = Enumerable.Range(0, 60).Select(i => (int) i).ToArray();
    /// <summary>
    /// [0,59]
    /// </summary>
    public static IEnumerable<int> MinuteList { get; } = Enumerable.Range(0, 60).Select(i => (int) i).ToArray();
    /// <summary>
    /// [0,23]
    /// </summary>
    public static IEnumerable<int> HourList { get; } = Enumerable.Range(0, 24).Select(i => (int) i).ToArray();
    /// <summary>
    /// [1,31]
    /// </summary>
    public static IEnumerable<int> DayOfMonthList { get; } = Enumerable.Range(1, 31).Select(i => (int) i).ToArray();
    /// <summary>
    /// [1,12]
    /// </summary>
    public static IEnumerable<int> MonthList { get; } = Enumerable.Range(1, 12).Select(i => (int) i).ToArray();
    /// <summary>
    /// [0,6]
    /// </summary>
    public static IEnumerable<int> DayOfWeekList_0 { get; } = Enumerable.Range(0, 7).Select(i => (int) i).ToArray();
    /// <summary>
    /// [1,7]
    /// </summary>
    public static IEnumerable<int> WeekOfMonthList_0 { get; } = Enumerable.Range(0, 5).Select(i => (int) i).ToArray();
    /// <summary>
    /// [1,5]
    /// </summary>
    public static IEnumerable<int> DayOfWeekList_1 { get; } = Enumerable.Range(1, 7).Select(i => (int) i).ToArray();
    /// <summary>
    /// [1,5]
    /// </summary>
    public static IEnumerable<int> WeekOfMonthList_1 { get; } = Enumerable.Range(1, 5).Select(i => (int) i).ToArray();
}