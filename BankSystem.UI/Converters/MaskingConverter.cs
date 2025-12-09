using System;
using System.Globalization;
using Avalonia.Data.Converters;
using BankSystem.Services;

namespace BankSystem.UI.Converters;

public class MaskingConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null) return string.Empty;
        var param = parameter as string;
        var str = value.ToString() ?? string.Empty;
        return param switch
        {
            "account" => Masking.MaskAccount(str),
            "id" => Masking.MaskId(str),
            _ => str
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value;
}
