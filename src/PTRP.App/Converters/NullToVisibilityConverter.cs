using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PTRP.App.Converters;

/// <summary>
/// Converts null to Visibility.Collapsed and non-null to Visibility.Visible.
/// Used throughout the app for conditional visibility based on object presence.
/// </summary>
public class NullToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// When true, inverts the logic: null = Visible, non-null = Collapsed
    /// </summary>
    public bool Invert { get; set; } = false;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isNull = value == null;

        if (Invert)
        {
            return isNull ? Visibility.Visible : Visibility.Collapsed;
        }
        else
        {
            return isNull ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException("NullToVisibilityConverter does not support ConvertBack");
    }
}
