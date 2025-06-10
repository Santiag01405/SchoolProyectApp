using Microsoft.Maui.Controls;
using SchoolProyectApp.Models;

namespace SchoolProyectApp.Selectors
{
    public class NotificationTemplateSelector : DataTemplateSelector
    {
        public DataTemplate RegularTemplate { get; set; }
        public DataTemplate AttendanceTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (item is AttendanceNotification)
                return AttendanceTemplate;

            return RegularTemplate;
        }
    }
}
