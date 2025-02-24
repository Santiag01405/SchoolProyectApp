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
        public ICommand ProfileCommand { get; }
        public ICommand OpenMenuCommand { get; }

        public HomePageViewModel()
        {
            Posts = new ObservableCollection<Post>
            {
                new Post { ProfileImage = "user1.png", Name = "FExample", Subtitle = "Example" },
                new Post { ProfileImage = "user2.png", Name = "Example", Subtitle = "Example", Message = "Example" },
                new Post { ProfileImage = "user3.png", Name = "Example", Subtitle = "Example", Message = "Example" }
            };

            HomeCommand = new Command(async () => await NavigateTo("homepage"));
            ProfileCommand = new Command(async () => await NavigateTo("profile"));
            OpenMenuCommand = new Command(async () => await NavigateTo("menu"));
        }

        private async Task NavigateTo(string page)
        {
            await Shell.Current.GoToAsync(page);
        }
    }
}
