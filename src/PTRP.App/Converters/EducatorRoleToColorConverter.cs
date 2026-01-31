using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PTRP.App.Converters;

/// <summary>
/// Converte il ruolo di un educatore in un colore per il badge.
/// - Coordinatore → Blu (#007BFF)
/// - Educatore → Verde (#28A745)
/// - Tirocinante → Arancione (#FD7E14)
/// - Inattivo → Grigio (#6C757D)
/// </summary>
public class EducatorRoleToColorConverter : IValueConverter
{
    private static readonly SolidColorBrush CoordinatorBrush = new(Color.FromRgb(0, 123, 255)); // Blu
    private static readonly SolidColorBrush EducatorBrush = new(Color.FromRgb(40, 167, 69));     // Verde
    private static readonly SolidColorBrush InternBrush = new(Color.FromRgb(253, 126, 20));     // Arancione
    private static readonly SolidColorBrush InactiveBrush = new(Color.FromRgb(108, 117, 125));  // Grigio
    private static readonly SolidColorBrush DefaultBrush = new(Color.FromRgb(108, 117, 125));   // Grigio default

    static EducatorRoleToColorConverter()
    {
        // Freeze brushes for performance
        CoordinatorBrush.Freeze();
        EducatorBrush.Freeze();
        InternBrush.Freeze();
        InactiveBrush.Freeze();
        DefaultBrush.Freeze();
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string role)
            return DefaultBrush;

        return role switch
        {
            "Coordinatore" => CoordinatorBrush,
            "Educatore" => EducatorBrush,
            "Tirocinante" => InternBrush,
            "Inattivo" => InactiveBrush,
            _ => DefaultBrush
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException("EducatorRoleToColorConverter does not support ConvertBack.");
    }
}
