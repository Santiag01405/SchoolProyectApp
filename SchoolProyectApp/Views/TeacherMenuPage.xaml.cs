using SchoolProyectApp.ViewModels;
using Microsoft.Maui.Controls;

namespace SchoolProyectApp.Views
{
    public partial class TeacherMenuPage : ContentPage
    {
        public TeacherMenuPage()
        {
            InitializeComponent();
            BindingContext = new TeacherMenuViewModel();
        }
    }
}