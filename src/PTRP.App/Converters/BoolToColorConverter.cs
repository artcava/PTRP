using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PTRP.App.Converters;

/// <summary>
/// Converte un booleano in un colore basato su parametro "TrueColor|FalseColor"
/// Esempio: "#007ACC|Transparent" => True = #007ACC, False = Transparent
/// </summary>
public class BoolToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not bool boolValue)
            return Brushes.Transparent;

        var param = parameter?.ToString() ?? "Black|Gray";
        var colors = param.Split('|');

        if (colors.Length != 2)
            return Brushes.Transparent;

        var targetColor = boolValue ? colors[0] : colors[1];

        try
        {
            return (Brush)new BrushConverter().ConvertFromString(targetColor)!;
        }
        catch
        {
            return Brushes.Transparent;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
