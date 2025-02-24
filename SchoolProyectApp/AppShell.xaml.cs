using SchoolProyectApp.Views;
using Microsoft.Maui.Controls;

namespace SchoolProyectApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Registrar rutas de navegación
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
            Routing.RegisterRoute(nameof(StudentMenuPage), typeof(StudentMenuPage));
            Routing.RegisterRoute(nameof(TeacherMenuPage), typeof(TeacherMenuPage));
            Routing.RegisterRoute(nameof(ParentMenuPage), typeof(ParentMenuPage));
        }

        // Método para redirigir al usuario según su rol después del login
        public static async Task NavigateToRoleMenu(string userRole)
        {
            switch (userRole)
            {
                case "Student":
                    await Shell.Current.GoToAsync("//studentmenu");
                    break;
                case "Teacher":
                    await Shell.Current.GoToAsync("//teachermenu");
                    break;
                case "Parent":
                    await Shell.Current.GoToAsync("//parentmenu");
                    break;
                default:
                    await Shell.Current.GoToAsync("//login");
                    break;
            }
        }
    }
}

/*namespace SchoolProyectApp
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
}*/