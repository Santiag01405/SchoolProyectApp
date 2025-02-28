using System.Windows.Input;
using Microsoft.Maui.Controls;

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

        //public ICommand ProfileCommand { get; }
        public ICommand OpenMenuCommand { get; }

        public MenuViewModel()
        {

            //Barra de navegacion
            HomeCommand = new Command(async () => await Shell.Current.GoToAsync("///homepage"));
           // ProfileCommand = new Command(async () => await NavigateTo("profile"));
            OpenMenuCommand = new Command(async () => await Shell.Current.GoToAsync("///menu"));
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
        }
    }
}
