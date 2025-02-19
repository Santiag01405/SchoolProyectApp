using SchoolProyectApp.ViewModels;
using Microsoft.Maui.Controls;

namespace SchoolProyectApp.Views
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent(); 
            BindingContext = new LoginViewModel();
        }
    }
}
