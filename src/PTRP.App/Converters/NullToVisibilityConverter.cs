using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PTRP.App.Converters
{
    /// <summary>
    /// Converts null/empty values to Visibility enum.
    /// Used to hide UI elements when data is not available.
    /// </summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts null/empty value to Visibility.
        /// </summary>
        /// <param name="value">Value to check (object, string, etc.)</param>
        /// <param name="targetType">Target type (Visibility)</param>
        /// <param name="parameter">Optional parameter ("Invert" to reverse logic)</param>
        /// <param name="culture">Culture info</param>
        /// <returns>Visibility.Visible if value is not null/empty, Visibility.Collapsed otherwise</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isNull = value == null;
            
            // Check for empty strings
            if (!isNull && value is string str)
            {
                isNull = string.IsNullOrWhiteSpace(str);
            }

            // Check for parameter to invert logic
            bool invert = parameter is string param && param.Equals("Invert", StringComparison.OrdinalIgnoreCase);

            if (invert)
            {
                return isNull ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                return isNull ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        /// <summary>
        /// Not implemented (one-way binding only).
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("NullToVisibilityConverter is one-way only.");
        }
    }
}
