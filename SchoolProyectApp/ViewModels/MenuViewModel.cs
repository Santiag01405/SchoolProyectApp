using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace SchoolProyectApp.ViewModels
{
    public class MenuViewModel : BaseViewModel
    {
        public ICommand MessagesCommand { get; }
        public ICommand RankingCommand { get; }
        public ICommand TimerCommand { get; }
        public ICommand CompetitionsCommand { get; }
        public ICommand CalendarCommand { get; }
        public ICommand MaxRecordsCommand { get; }
        public ICommand BenchmarksCommand { get; }
        public ICommand PhysicalEvolutionCommand { get; }
        public ICommand AnalyticsCommand { get; }
        public ICommand ExerciseLibraryCommand { get; }
        public ICommand VideosCommand { get; }
        public ICommand HomeCommand { get; }
        public ICommand OpenScheduleCommand { get; }
        public ICommand OpenEditScheduleCommand { get; } 
        public ICommand OpenMenuCommand { get; }

        public MenuViewModel()
        {
            // Barra de navegación
            HomeCommand = new Command(async () => await Shell.Current.GoToAsync("///homepage"));
            OpenMenuCommand = new Command(async () => await Shell.Current.GoToAsync("///menu"));

            // Comandos de las opciones del menú
            MessagesCommand = new Command(async () => await Shell.Current.GoToAsync("messages"));
            RankingCommand = new Command(async () => await Shell.Current.GoToAsync("ranking"));
            TimerCommand = new Command(async () => await Shell.Current.GoToAsync("timer"));
            CompetitionsCommand = new Command(async () => await Shell.Current.GoToAsync("competitions"));
            CalendarCommand = new Command(async () => await Shell.Current.GoToAsync("calendar"));
            MaxRecordsCommand = new Command(async () => await Shell.Current.GoToAsync("maxrecords"));
            BenchmarksCommand = new Command(async () => await Shell.Current.GoToAsync("benchmarks"));
            PhysicalEvolutionCommand = new Command(async () => await Shell.Current.GoToAsync("physicalevolution"));
            AnalyticsCommand = new Command(async () => await Shell.Current.GoToAsync("analytics"));
            ExerciseLibraryCommand = new Command(async () => await Shell.Current.GoToAsync("exerciselibrary"));
            VideosCommand = new Command(async () => await Shell.Current.GoToAsync("videos"));

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
    }
}
