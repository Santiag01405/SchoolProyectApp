using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace SchoolProyectApp.Converter
{
    public class TabBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var selectedTab = value?.ToString();
            var targetTab = parameter?.ToString();

            return selectedTab == targetTab
                ? Color.FromArgb("#0C4251") // Color del botón activo
                : Color.FromArgb("#6bbdda"); // Color del botón inactivo
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
