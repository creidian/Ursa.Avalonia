using System;
using System.Linq;
using TimeCrontab;
using Ursa.Controls;

namespace Ursa.CronPickerDemo;

public class CronExpressionParser : ICronExpressionParser
{
    public static readonly CronExpressionParser Instance = new();
    public DateTime GetNextTime(string expression, bool isSecondContained, bool isYearContained, DateTime startDate)
    {
        TimeCrontab.CronStringFormat cronFormat = TimeCrontab.CronStringFormat.Default;
        string[] args = expression.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (args.Length < 5)
        {
            throw new ArgumentException("corn表达式格式错误，缺少必要字段");
        }
        
        if (isSecondContained && isYearContained)
        {
            cronFormat = TimeCrontab.CronStringFormat.WithSecondsAndYears;
            if (args.Length < 6)
            {
                throw new ArgumentException("corn表达式格式错误，缺少年份字段");
            }

            if (args.Length < 7)
            {
                cronFormat = TimeCrontab.CronStringFormat.WithSeconds;
            }
        }
        else if (isSecondContained)
        {
            cronFormat = TimeCrontab.CronStringFormat.WithSeconds;
            if (args.Length < 6)
            {
                throw new ArgumentException("corn表达式格式错误，缺少秒字段");
            }
        }
        else if (isYearContained)
        {
            cronFormat = TimeCrontab.CronStringFormat.WithYears;
            if (args.Length < 5)
            {
                throw new ArgumentException("corn表达式格式错误，缺少年份字段");
            }
            
            if (args.Length < 6)
            {
                cronFormat = TimeCrontab.CronStringFormat.Default;
            }
        }

        /*string[] args = expression.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (args.Length == 7)
        {
            cronFormat = TimeCrontab.CronStringFormat.WithSecondsAndYears;
            if (args.Last() is "Y" or "y")
            {
                cronFormat = CronStringFormat.WithYears;
                expression = expression.Substring(0, expression.Length - 1).Trim();
            }
        }
        else if (args.Length == 6)
        {
            cronFormat = TimeCrontab.CronStringFormat.WithSeconds;
        }*/

        Crontab crontab;
        try
        {
            crontab = Crontab.Parse(expression, cronFormat);
        }
        catch (Exception e)
        {
            throw new ArgumentException($"corn表达式解析失败-{e.Message}", e);
        }

        return crontab.GetNextOccurrence(startDate);
    }
}