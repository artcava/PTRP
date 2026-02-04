using System;
using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PTRP.App.Converters
{
    /// <summary>
    /// Converts a collection's count to Visibility enum.
    /// Used to show/hide UI sections when a collection is empty or populated.
    /// </summary>
    public class CollectionCountToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts collection count to Visibility.
        /// Returns Visible if collection has items, Collapsed if empty or null.
        /// </summary>
        /// <param name="value">Collection to check (IEnumerable)</param>
        /// <param name="targetType">Target type (Visibility)</param>
        /// <param name="parameter">Optional parameter ("Invert" to reverse logic)</param>
        /// <param name="culture">Culture info</param>
        /// <returns>Visibility.Visible if collection has items, Visibility.Collapsed if empty</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool hasItems = false;

            if (value is ICollection collection)
            {
                hasItems = collection.Count > 0;
            }
            else if (value is IEnumerable enumerable)
            {
                // Check if enumerable has at least one item
                var enumerator = enumerable.GetEnumerator();
                hasItems = enumerator.MoveNext();
            }

            // Check for parameter to invert logic
            bool invert = parameter is string param && param.Equals("Invert", StringComparison.OrdinalIgnoreCase);

            if (invert)
            {
                return hasItems ? Visibility.Collapsed : Visibility.Visible;
            }
            else
            {
                return hasItems ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Not implemented (one-way binding only).
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("CollectionCountToVisibilityConverter is one-way only.");
        }
    }
}
