using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PTRP.App.Converters
{
    /// <summary>
    /// Converts project state string to color brush for badge display.
    /// Used in PatientListView DataGrid to show colored status badges.
    /// </summary>
    public class ProjectStateToColorConverter : IValueConverter
    {
        /// <summary>
        /// Converts project state to color brush.
        /// </summary>
        /// <param name="value">Project state string (Active, Suspended, Completed, Deceased, None)</param>
        /// <param name="targetType">Target type (Brush)</param>
        /// <param name="parameter">Optional parameter</param>
        /// <param name="culture">Culture info</param>
        /// <returns>SolidColorBrush for the badge background</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string state)
                return new SolidColorBrush(Colors.Gray);

            return state switch
            {
                "Active" => new SolidColorBrush(Color.FromRgb(76, 175, 80)),      // Material Green 500
                "Suspended" => new SolidColorBrush(Color.FromRgb(255, 193, 7)),   // Material Amber 500
                "Completed" => new SolidColorBrush(Color.FromRgb(158, 158, 158)), // Material Grey 500
                "Deceased" => new SolidColorBrush(Color.FromRgb(244, 67, 54)),    // Material Red 500
                "None" => new SolidColorBrush(Color.FromRgb(189, 189, 189)),      // Material Grey 400
                _ => new SolidColorBrush(Colors.Gray)
            };
        }

        /// <summary>
        /// Not implemented (one-way binding only).
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("ProjectStateToColorConverter is one-way only.");
        }
    }
}
