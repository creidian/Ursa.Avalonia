namespace Ursa.Converters;

public class SharpStringFormatter : IStringFormatter
{
    public static readonly SharpStringFormatter Instance = new SharpStringFormatter();
    
    public string Format(string? source, IReadOnlyList<IParameterReplaceRuler> rulers, IReadOnlyDictionary<string, object?> args)
    {
        if (source is null)
        {
            return string.Empty;
        }

        if (args is null || args.Count == 0)
        {
            return source;
        }

        return InnerFormat(source, rulers, args.Keys.ToArray(), args.Values.ToArray());
    }

    public string Format(string? source, IReadOnlyList<IParameterReplaceRuler> rulers, params (string Key, object? Value)[] args)
    {
        if (source is null)
        {
            return string.Empty;
        }

        if (args is null || args.Length == 0)
        {
            return source;
        }

        string[] keys = new string[args.Length];
        object?[] values = new object?[args.Length];
        for (var i = 0; i < args.Length; i++)
        {
            keys[i] = args[i].Key;
            values[i] = args[i].Value;
        }

        return InnerFormat(source, rulers, keys, values);
    }

    public string Format(string? source, IReadOnlyList<IParameterReplaceRuler> rulers, string[] keys, object?[] values)
    {
        if (source is null)
        {
            return string.Empty;
        }
        
        return InnerFormat(source, rulers, keys, values);
    }
    
    private string InnerFormat(string source, IReadOnlyList<IParameterReplaceRuler> rulers, string[] keys, object?[] values)
    {
        if (keys is null || keys.Length == 0)
        {
            return source;
        }

        if (rulers is not null && rulers.Count != 0)
        {
            for (var i = 0; i < keys.Length; i++)
            {
                string key = keys[i];
                object? value = values[i];
                bool isFound = false;
                foreach (IParameterReplaceRuler ruler in rulers)
                {
                    if (string.Equals(ruler.ParameterName, key, StringComparison.CurrentCultureIgnoreCase))
                    {
                        IParameterReplacer replacer = ruler.Replacer ?? SharpParameterReplacer.Instance;
                        source = replacer.Replace(source, ruler, value);
                        isFound = true;
                        break;
                    }
                } // 遍历参数替换规则

                if (!isFound)
                {
                    source = SharpParameterReplacer.Instance.Replace(source, null, key, null, value);
                }
            }
        } // 自定义参数替换规则
        else
        {
            bool isReplaced = false;
            for (var i = 0; i < keys.Length; i++)
            {
                string key = keys[i];
                object? value = values[i];
                source = SharpParameterReplacer.Instance.Replace(source, null, key, null, value, false, true, out int count);
                if (count > 0)
                {
                    isReplaced = true;
                }
            }

            if (!isReplaced)
            {
                source = string.Format(source, values.ToArray());
            }
        } // 系统默认参数替换规则

        return source;
    }
}