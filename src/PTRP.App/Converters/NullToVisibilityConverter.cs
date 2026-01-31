using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PTRP.App.Converters
{
    /// <summary>
    /// Converts null/non-null values to Visibility.
    /// Used to show/hide UI elements based on whether a value is present.
    /// Example: Hide detail panel when no item is selected.
    /// </summary>
    [ValueConversion(typeof(object), typeof(Visibility))]
    public class NullToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts null to Collapsed, non-null to Visible.
        /// </summary>
        /// <param name="value">The value to check for null</param>
        /// <param name="targetType">Not used</param>
        /// <param name="parameter">Optional: "Invert" to reverse logic</param>
        /// <param name="culture">Not used</param>
        /// <returns>Visibility.Visible if not null, Visibility.Collapsed if null</returns>
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var isNull = value == null;
            var invert = parameter is string param && param == "Invert";
            
            if (invert)
            {
                return isNull ? Visibility.Visible : Visibility.Collapsed;
            }
            
            return isNull ? Visibility.Collapsed : Visibility.Visible;
        }

        /// <summary>
        /// ConvertBack not implemented (one-way binding).
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("NullToVisibilityConverter is one-way only.");
        }
    }
}
