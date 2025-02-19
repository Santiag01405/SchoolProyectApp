using Microsoft.Maui.Controls;
using SchoolProyectApp.Views;

namespace SchoolProyectApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
        }
    }
}