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
                new Post { ProfileImage = "usuarios.png", Name = "Ejemplo de titulo", Subtitle = "Ejemplo de subtitulo", Message = "Ejemplo de mensaje" },
                new Post { ProfileImage = "usuarios.png", Name = "Ejemplo de titulo", Subtitle = "Ejemplo de subtitulo", Message = "Ejemplo de mensaje" },
                new Post { ProfileImage = "usuarios.png", Name = "Ejemplo de titulo", Subtitle = "Ejemplo de subtitulo", Message = "Ejemplo de mensaje" },
                new Post { ProfileImage = "usuarios.png", Name = "Ejemplo de titulo", Subtitle = "Ejemplo de subtitulo", Message = "Ejemplo de mensaje" },
                new Post { ProfileImage = "usuarios.png", Name = "Ejemplo de titulo", Subtitle = "Ejemplo de subtitulo", Message = "Ejemplo de mensaje" }
            };

            HomeCommand = new Command(async () => await Shell.Current.GoToAsync("///homepage"));
            ProfileCommand = new Command(async () => await NavigateTo("profile"));
            OpenMenuCommand = new Command(async () => await Shell.Current.GoToAsync("///menu"));
        }

        private async Task NavigateTo(string page)
        {
            await Shell.Current.GoToAsync(page);
        }



    }
}
