/*using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using SchoolProyectApp.Models;
using SchoolProyectApp.Services;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SchoolProyectApp.ViewModels
{
    public class SchedulePageViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        public ObservableCollection<Course> Courses { get; set; }
        public ICommand HomeCommand { get; }
        public ICommand WodCommand { get; }
        public ICommand ProfileCommand { get; }
        public ICommand OpenMenuCommand { get; }
        public ICommand AddStudentCommand { get; }
        public ICommand RemoveStudentCommand { get; }

        private bool _canEditCourses;
        public bool CanEditCourses
        {
            get => _canEditCourses;
            set => SetProperty(ref _canEditCourses, value);
        }

        public SchedulePageViewModel()
        {
            _apiService = new ApiService();
            Courses = new ObservableCollection<Course>();
            HomeCommand = new Command(() => NavigateTo("Home"));
            WodCommand = new Command(() => NavigateTo("Wod"));
            ProfileCommand = new Command(() => NavigateTo("Profile"));
            OpenMenuCommand = new Command(() => NavigateTo("Menu"));
            AddStudentCommand = new Command<Course>(async (course) => await AddStudentToCourse(course));
            RemoveStudentCommand = new Command<Course>(async (course) => await RemoveStudentFromCourse(course));
            LoadCourses();
        }

        private async void LoadCourses()
        {
            var userRole = await SecureStorage.GetAsync("user_role");
            CanEditCourses = userRole == "2"; // Solo los profesores pueden editar

            var userId = await SecureStorage.GetAsync("user_id");
            if (string.IsNullOrEmpty(userId)) return;

            var courses = await _apiService.GetCoursesForUserAsync(int.Parse(userId));
            Courses.Clear();
            foreach (var course in courses)
            {
                Courses.Add(course);
            }
        }

        private async Task AddStudentToCourse(Course course)
        {
            var success = await _apiService.AddStudentToCourseAsync(course.CourseId);
            if (success)
            {
                LoadCourses(); // Recargar la lista después de agregar
            }
        }

        private async Task RemoveStudentFromCourse(Course course)
        {
            var success = await _apiService.RemoveStudentFromCourseAsync(course.CourseId);
            if (success)
            {
                LoadCourses(); // Recargar la lista después de eliminar
            }
        }

        private void NavigateTo(string page)
        {
            // Implementar navegación a otras páginas
        }
    }
}*/




