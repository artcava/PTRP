using System;
using System.Globalization;
using System.Windows.Data;

namespace PTRP.App.Converters
{
    /// <summary>
    /// Converts a boolean value to its inverse.
    /// Used in XAML bindings to invert boolean properties.
    /// Example: IsEnabled="{Binding IsLoading, Converter={StaticResource InverseBooleanConverter}}"
    /// </summary>
    [ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBooleanConverter : IValueConverter
    {
        /// <summary>
        /// Converts a boolean value to its inverse.
        /// </summary>
        /// <param name="value">The boolean value to invert</param>
        /// <param name="targetType">Not used</param>
        /// <param name="parameter">Not used</param>
        /// <param name="culture">Not used</param>
        /// <returns>The inverted boolean value</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            
            return false;
        }

        /// <summary>
        /// Converts the inverted boolean back to original (ConvertBack).
        /// </summary>
        /// <param name="value">The inverted boolean value</param>
        /// <param name="targetType">Not used</param>
        /// <param name="parameter">Not used</param>
        /// <param name="culture">Not used</param>
        /// <returns>The original boolean value</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            
            return false;
        }
    }
}
