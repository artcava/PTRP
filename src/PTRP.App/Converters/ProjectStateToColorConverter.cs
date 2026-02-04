using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using PTRP.Models.Enums;

namespace PTRP.App.Converters;

/// <summary>
/// Converts TherapyProjectState enum to a Color brush for UI display.
/// Used in ProjectListView for status badges.
/// </summary>
public class ProjectStateToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not TherapyProjectState status)
            return Brushes.Gray;

        return status switch
        {
            TherapyProjectState.Active => new SolidColorBrush(Color.FromRgb(40, 167, 69)),      // Green #28A745
            TherapyProjectState.Suspended => new SolidColorBrush(Color.FromRgb(255, 193, 7)),   // Yellow #FFC107
            TherapyProjectState.Completed => new SolidColorBrush(Color.FromRgb(0, 123, 255)),   // Blue #007BFF
            TherapyProjectState.Deceased => new SolidColorBrush(Color.FromRgb(108, 117, 125)),  // Gray #6C757D
            _ => Brushes.Gray
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException("ProjectStateToColorConverter does not support ConvertBack");
    }
}
