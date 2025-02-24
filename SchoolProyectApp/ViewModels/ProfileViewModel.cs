using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using SchoolProyectApp.Models;

namespace SchoolProyectApp.ViewModels
{
    public class ProfileViewModel : BaseViewModel
    {
        public ObservableCollection<Post> Posts { get; set; }
        public ICommand EditStatusCommand { get; }
        public ICommand ViewPRsCommand { get; }
        public ICommand ViewBenchmarksCommand { get; }

        public ProfileViewModel()
        {
            // Datos de ejemplo
            Posts = new ObservableCollection<Post>
            {
                new Post { Title = "8 Rounds for Time", Description = "2 Snatch 80% RM\n4 Muscle-Ups\n8 Box Jump (30”/24”)\nTotal: 18:30" }
            };

            EditStatusCommand = new Command(() => NavigateTo("EditStatusPage"));
            ViewPRsCommand = new Command(() => NavigateTo("PRsPage"));
            ViewBenchmarksCommand = new Command(() => NavigateTo("BenchmarksPage"));
        }

        private async void NavigateTo(string page)
        {
            await Shell.Current.GoToAsync(page);
        }
    }
}