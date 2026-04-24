using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using MediBookDesktop.Models;

namespace MediBookDesktop.Helpers;

public class AppointmentStatusBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch
        {
            AppointmentStatus.Booked => new SolidColorBrush(Color.FromRgb(20, 108, 148)),
            AppointmentStatus.Completed => new SolidColorBrush(Color.FromRgb(15, 118, 110)),
            AppointmentStatus.Cancelled => new SolidColorBrush(Color.FromRgb(180, 35, 24)),
            AppointmentStatus.NoShow => new SolidColorBrush(Color.FromRgb(161, 98, 7)),
            _ => new SolidColorBrush(Color.FromRgb(102, 112, 133))
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
