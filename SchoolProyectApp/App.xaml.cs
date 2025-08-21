namespace SchoolProyectApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Establecer el tema de la aplicación
            Current.UserAppTheme = AppTheme.Light;

            MainPage = new AppShell();
        }
    }
}
