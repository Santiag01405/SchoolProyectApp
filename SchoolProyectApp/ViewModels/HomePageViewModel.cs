using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using SchoolProyectApp.Models;

namespace SchoolProyectApp.ViewModels
{
    public class HomePageViewModel : BaseViewModel
    {
        public ObservableCollection<Post> Posts { get; set; }
        public ICommand HomeCommand { get; }
        public ICommand ExampleCommand { get; }
        public ICommand ProfileCommand { get; }
        public ICommand MenuCommand { get; }
        public ICommand Example2Command { get; }
        public ICommand Example3Command { get; }
        public ICommand Example4Command { get; }

        public HomePageViewModel()
        {
            Posts = new ObservableCollection<Post>
            {
                new Post { ProfileImage = "user1.png", Name = "Fabio Spizuoco", Subtitle = "The Hive · Hoy", Message = "Solo los miembros de The Hive pueden ver este WOD" },
                new Post { ProfileImage = "user2.png", Name = "Flory Fernández", Subtitle = "The Hive · Jueves", Message = "Solo los miembros de The Hive pueden ver este WOD" },
                new Post { ProfileImage = "user3.png", Name = "Pedro Barrueta", Subtitle = "The Hive · Jueves", Message = "Solo los miembros de The Hive pueden ver este WOD" }
            };

            HomeCommand = new Command(() => NavigateTo("HomePage"));
            ExampleCommand = new Command(() => NavigateTo("ExamplePage"));
            ProfileCommand = new Command(() => NavigateTo("ProfilePage"));
            MenuCommand = new Command(() => NavigateTo("MenuPage"));
            Example2Command = new Command(() => NavigateTo("Example2Page"));
            Example3Command = new Command(() => NavigateTo("Example3Page"));
            Example4Command = new Command(() => NavigateTo("Example4Page"));
        }

        private async void NavigateTo(string page)
        {
            await Shell.Current.GoToAsync(page);
        }
    }
}
