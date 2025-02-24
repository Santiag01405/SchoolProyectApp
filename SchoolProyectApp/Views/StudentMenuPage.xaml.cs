using SchoolProyectApp.ViewModels;
using Microsoft.Maui.Controls;

namespace SchoolProyectApp.Views
{
    public partial class StudentMenuPage : ContentPage
    {
        public StudentMenuPage()
        {
            InitializeComponent();
            BindingContext = new StudentMenuViewModel();
        }
    }
}
