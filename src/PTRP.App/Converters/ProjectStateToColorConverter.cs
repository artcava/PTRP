using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PTRP.App.Converters
{
    /// <summary>
    /// Converts a project state string to a colored brush for badge display.
    /// Used in PatientListView to visually distinguish patient states.
    /// </summary>
    [ValueConversion(typeof(string), typeof(Brush))]
    public class ProjectStateToColorConverter : IValueConverter
    {
        /// <summary>
        /// Converts project state to corresponding color brush.
        /// </summary>
        /// <param name="value">The project state string (Active, Suspended, Completed, Deceased)</param>
        /// <param name="targetType">Not used</param>
        /// <param name="parameter">Not used</param>
        /// <param name="culture">Not used</param>
        /// <returns>SolidColorBrush matching the state</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string state)
            {
                return state switch
                {
                    "Active" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#28A745")!), // Green
                    "Suspended" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC107")!), // Yellow
                    "Completed" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6C757D")!), // Gray
                    "Deceased" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#000000")!), // Black
                    _ => Brushes.Gray
                };
            }
            
            return Brushes.Gray;
        }

        /// <summary>
        /// ConvertBack not implemented (one-way binding).
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("ProjectStateToColorConverter is one-way only.");
        }
    }
}
