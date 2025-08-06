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
            Routing.RegisterRoute(nameof(HomePage), typeof(HomePage));
            Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
            Routing.RegisterRoute(nameof(ChildDashboardPage), typeof(ChildDashboardPage));
        }
        public async Task Logout()
        {
            /*// Eliminar credenciales guardadas
            await SecureStorage.SetAsync("user_id", "");
            await SecureStorage.SetAsync("user_role", "");
            await SecureStorage.SetAsync("user_name", "");
            await SecureStorage.SetAsync("password", "");

            // Redirigir al Login y limpiar la pila de navegación
            Application.Current.MainPage = new NavigationPage(new Views.LoginPage());*/

            try
            {
                Console.WriteLine("🔹 Cerrando sesión...");

                // ❗ Elimina SOLO lo necesario, no todo el SecureStorage
                await SecureStorage.SetAsync("auth_token", "");
                await SecureStorage.SetAsync("user_id", "");
                await SecureStorage.SetAsync("user_role", "");

                Console.WriteLine("✔ Datos eliminados de SecureStorage.");

                // 🔹 Reiniciar correctamente la MainPage para que Shell siga funcionando
                Device.BeginInvokeOnMainThread(() =>
                {
                    Application.Current.MainPage = new AppShell();
                });

                Console.WriteLine("✔ Redirigido a LoginPage.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al cerrar sesión: {ex.Message}");
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