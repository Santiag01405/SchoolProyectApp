using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace SchoolProyectApp.Converter
{
    public class TabBackgroundMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2 || values[0] is not string tabDay || values[1] is not string selectedDay)
                return Colors.Gray;

            return tabDay == selectedDay ? Color.FromArgb("#0C4251") : Color.FromArgb("#BBBBBB");
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
