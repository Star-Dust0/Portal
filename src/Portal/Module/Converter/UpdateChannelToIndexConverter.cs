using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Portal.Module.Converter;

public class UpdateChannelToIndexConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            "dev" => 0,
            "nightly" => 1,
            "commit" => 2,
            _ => 0
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}