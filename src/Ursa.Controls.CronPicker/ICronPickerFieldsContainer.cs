namespace Ursa.Controls;

public interface ICronPickerFieldsContainer
{
    /*/// <summary>
    /// 规则值变更事件
    /// </summary>
    event EventHandler<UsualValueChangedEventArgs<(ICronPickerRulersContainer Field, string Value)>> RulerValueChanged;*/
    IEnumerable<ICronPickerRulersContainer> GetCronPickerFields();
    void Select(int index);
    /*void RemoveRulerValueChangedHandler(EventHandler<UsualValueChangedEventArgs<(ICronPickerRulersContainer Field, string Value)>> handler);
    void AddRulerValueChangedHandler(EventHandler<UsualValueChangedEventArgs<(ICronPickerRulersContainer Field, string Value)>> handler);*/
}

public interface ICronPickerRulersContainer
{
    /// <summary>
    /// 规则值变更事件
    /// </summary>
    event EventHandler<UsualValueChangedEventArgs<string>> ValueChanged;
    string Value { get; }
    CronFieldTypes GetFieldType();
    void ParsetoValue(string cronFieldString);
}

public interface ICronPickerRulerSelector
{
    void Select(bool isSelected);
}
public interface ICronPickerRulerItem
{
    /// <summary>
    /// 规则值变更事件
    /// </summary>
    event EventHandler<UsualValueChangedEventArgs<string>> ValueChanged;
    int Priority { get; }
    CronParseResult ParseTo(string text);
    string Value { get; }
    void VerifyCurrentCronValue();
}

public interface ICronPickerRulerItemParent
{
    void ValueChanged(string value);
}

public class CronParseResult
{
    public bool IsSuccessed => Code == ParseResultTypeCodes.Success;
    
    public ParseResultTypeCodes Code { get; }
    
    public string Message { get; }

    private CronParseResult(ParseResultTypeCodes code, string message)
    {
        Code = code;
        Message = message;
    }
    
    public static CronParseResult Success(string message = "Success") => new(ParseResultTypeCodes.Success, message);
    public static CronParseResult UnSupported(string message = "Unknown ruler") => new(ParseResultTypeCodes.UnSupported, message);
    public static CronParseResult FormatError(string message) => new(ParseResultTypeCodes.FormatError, message);
}

public enum ParseResultTypeCodes : byte
{
    /// <summary>
    /// 不受支持的规则
    /// </summary>
    UnSupported = 0,
    Success = 1,
    FormatError = 2,
}