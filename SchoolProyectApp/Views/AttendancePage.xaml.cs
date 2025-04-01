using Microsoft.Maui.Controls;
using SchoolProyectApp.ViewModels;

namespace SchoolProyectApp.Views
{
    public partial class AttendancePage : ContentPage
    {
        public AttendancePage()
        {
            InitializeComponent();
            BindingContext = new AttendanceViewModel();
        }
    }
}

