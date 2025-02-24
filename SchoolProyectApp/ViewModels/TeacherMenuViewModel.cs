using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using SchoolProyectApp.Models;
using SchoolProyectApp.Services;

namespace SchoolProyectApp.ViewModels
{
    public class TeacherMenuViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        public ObservableCollection<Notification> Notifications { get; set; } = new();
        public ObservableCollection<Course> Courses { get; set; } = new();
        public ObservableCollection<User> Students { get; set; } = new();

        public ICommand LoadNotificationsCommand { get; }
        public ICommand LoadCoursesCommand { get; }
        public ICommand LoadStudentsCommand { get; }
        public ICommand AssignGradesCommand { get; }

        public TeacherMenuViewModel()
        {
            _apiService = new ApiService();
            LoadNotificationsCommand = new Command(async () => await LoadNotifications());
            LoadCoursesCommand = new Command(async () => await LoadCourses());
            LoadStudentsCommand = new Command(async () => await LoadStudents());
            AssignGradesCommand = new Command(AssignGrades);

            _ = LoadNotifications();
            _ = LoadCourses();
            _ = LoadStudents();
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

        private async Task LoadStudents()
        {
            var students = await _apiService.GetUsersAsync(); // Asegúrate de que este método solo devuelva estudiantes
            Students.Clear();
            foreach (var student in students)
            {
                Students.Add(student);
            }
        }

        private void AssignGrades()
        {
            // Aquí puedes agregar la lógica para asignar calificaciones a los estudiantes
        }
    }
}
