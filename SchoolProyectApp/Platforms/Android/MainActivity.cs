using Android.App;
using Android.Content.PM;
using Android.OS;

namespace SchoolProyectApp
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Cambia el color de la barra de navegación (abajo)
            Window.SetNavigationBarColor(Android.Graphics.Color.ParseColor("#0C4251"));

            // Opcional: también cambiar barra de estado (arriba)
            Window.SetStatusBarColor(Android.Graphics.Color.ParseColor("#0C4251"));
        }

    }
}
