using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using SchoolProyectApp.Models;
using SchoolProyectApp.Services;

namespace SchoolProyectApp.ViewModels
{
    public class ParentMenuViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        public ObservableCollection<Notification> Notifications { get; set; } = new();
        public ObservableCollection<Course> Courses { get; set; } = new();
        public ObservableCollection<Grade> Grades { get; set; } = new();

        public ICommand LoadNotificationsCommand { get; }
        public ICommand LoadCoursesCommand { get; }
        public ICommand LoadGradesCommand { get; }

        public ParentMenuViewModel()
        {
            _apiService = new ApiService();
            LoadNotificationsCommand = new Command(async () => await LoadNotifications());
            LoadCoursesCommand = new Command(async () => await LoadCourses());
            LoadGradesCommand = new Command(async () => await LoadGrades());

            _ = LoadNotifications();
            _ = LoadCourses();
            _ = LoadGrades();
        }

        private async Task LoadNotifications()
        {
            var notifications = await _apiService.GetNotificationsAsync();
            Notifications.Clear();
            foreach (var notification in notifications)
            {
                Notifications.Add(notification);
            }
        }

        private async Task LoadCourses()
        {
            var courses = await _apiService.GetCoursesAsync();
            Courses.Clear();
            foreach (var course in courses)
            {
                Courses.Add(course);
            }
        }

        private async Task LoadGrades()
        {
            var grades = await _apiService.GetGradesAsync();
            Grades.Clear();
            foreach (var grade in grades)
            {
                Grades.Add(grade);
            }
        }
    }
}
