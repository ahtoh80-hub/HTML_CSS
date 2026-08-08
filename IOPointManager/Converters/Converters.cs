using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using IOPointManager.Interfaces;
using IOPointManager.Models;

namespace IOPointManager.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public Color TrueColor { get; set; } = Colors.Green;
        public Color FalseColor { get; set; } = Colors.Red;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && boolValue)
                return new SolidColorBrush(TrueColor);
            return new SolidColorBrush(FalseColor);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class LogColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is EventType eventType)
            {
                return eventType switch
                {
                    EventType.Info => new SolidColorBrush(Colors.White),
                    EventType.Warning => new SolidColorBrush(Colors.Gold),
                    EventType.Error => new SolidColorBrush(Colors.Red),
                    _ => new SolidColorBrush(Colors.White)
                };
            }
            return new SolidColorBrush(Colors.White);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class QualityToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int score)
            {
                if (score >= 80) return new SolidColorBrush(Colors.Green);
                if (score >= 50) return new SolidColorBrush(Colors.Orange);
                return new SolidColorBrush(Colors.Red);
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IOPointStatus status)
            {
                return status switch
                {
                    IOPointStatus.Active => new SolidColorBrush(Colors.Green),
                    IOPointStatus.Inactive => new SolidColorBrush(Colors.Gray),
                    IOPointStatus.Commissioning => new SolidColorBrush(Colors.Blue),
                    IOPointStatus.Maintenance => new SolidColorBrush(Colors.Orange),
                    IOPointStatus.Fault => new SolidColorBrush(Colors.Red),
                    IOPointStatus.Decommissioned => new SolidColorBrush(Colors.DarkGray),
                    _ => new SolidColorBrush(Colors.Gray)
                };
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}