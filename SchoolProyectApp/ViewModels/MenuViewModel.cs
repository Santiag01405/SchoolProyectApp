using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace SchoolProyectApp.ViewModels
{
    public class MenuViewModel : BaseViewModel
    {
        public ICommand HomeCommand { get; }
        public ICommand OpenScheduleCommand { get; }
        public ICommand OpenEditScheduleCommand { get; } 
        public ICommand OpenMenuCommand { get; }
        public ICommand CourseCommand { get; }
        public ICommand ProfileCommand { get; }
        public ICommand LoginCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand FirstProfileCommand { get; }
        public MenuViewModel()
        {
            // Barra de navegación
            HomeCommand = new Command(async () => await Shell.Current.GoToAsync("///homepage"));
            ProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///profile"));
            OpenMenuCommand = new Command(async () => await Shell.Current.GoToAsync("///menu"));
            CourseCommand = new Command(async () => await Shell.Current.GoToAsync("///courses"));
            FirstProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///firtsprofile"));

            //Cerrar sesion
            LogoutCommand = new Command(async () => await Logout());


            // Todos pueden ver el horario
            OpenScheduleCommand = new Command(async () => await Shell.Current.GoToAsync("///schedule"));

            // Solo los Teachers (rol 2) pueden editar el horario
            OpenEditScheduleCommand = new Command(async () =>
            {
                if (GetUserRole() == 2) // 🔹 Validar rol en ejecución, no en constructor
                {
                    await Shell.Current.GoToAsync("///editschedule");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Acceso Denegado", "Solo los maestros pueden editar el horario.", "OK");
                }
            });

        }

        private int GetUserRole()
        {
            var roleString = SecureStorage.GetAsync("user_role").Result;
            return int.TryParse(roleString, out int role) ? role : 0;
        }

        //Logout
        private async Task Logout()
        {
            if (Application.Current.MainPage is AppShell shell)
            {
                await shell.Logout();
            }
        }
    }
}
