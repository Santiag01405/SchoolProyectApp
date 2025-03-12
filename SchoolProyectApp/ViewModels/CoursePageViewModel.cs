using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using SchoolProyectApp.Models;
using SchoolProyectApp.Services;
using SchoolProyectApp.Views;

namespace SchoolProyectApp.ViewModels
{
    public class CoursesPageViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private int _userId;
        private int _userRole;
        private string _searchText = string.Empty;
        
        public ObservableCollection<Course> Courses { get; set; } = new();
        public ObservableCollection<Course> FilteredCourses { get; set; } = new();
        
        public ICommand LoadCoursesCommand { get; }
        public ICommand EditCourseCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand HomeCommand { get; }
        public ICommand ProfileCommand { get; }
        public ICommand OpenMenuCommand { get; }
        public ICommand CourseCommand { get; }
        public ICommand FirstProfileCommand { get; }


        public bool IsCourseListEmpty => !FilteredCourses.Any();

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilterCourses();
                OnPropertyChanged(nameof(IsCourseListEmpty));
            }
        }

        public CoursesPageViewModel()
        {
            _apiService = new ApiService();
            LoadCoursesCommand = new Command(async () => await LoadCoursesAsync());
            EditCourseCommand = new Command<Course>(async (course) => await OpenEditPopup(course));
            SearchCommand = new Command(FilterCourses);
            HomeCommand = new Command(async () => await Shell.Current.GoToAsync("///homepage"));
            ProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///profile"));
            OpenMenuCommand = new Command(async () => await Shell.Current.GoToAsync("///menu"));
            CourseCommand = new Command(async () => await Shell.Current.GoToAsync("///courses"));
            FirstProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///firtsprofile"));


            Task.Run(async () => await InitializeUser());
        }

        //Abrir editar curso
        private async Task OpenEditPopup(Course course)
        {
            if (_userRole != 2) return; // Solo los profesores pueden editar
            await Shell.Current.Navigation.PushModalAsync(new EditCoursePopup(course));
        }
        private async Task InitializeUser()
        {
            var userIdString = await SecureStorage.GetAsync("user_id");
            var roleString = await SecureStorage.GetAsync("user_role");

            Console.WriteLine($"🔹 Obteniendo datos de SecureStorage: user_id={userIdString}, role_id={roleString}");

            if (int.TryParse(userIdString, out _userId) && int.TryParse(roleString, out _userRole))
            {
                await LoadCoursesAsync();
            }
            else
            {
                Console.WriteLine("Error: No se pudo obtener el ID o rol del usuario.");
            }
        }

        private async Task LoadCoursesAsync()
        {
            if (_userId == 0) return;

            Console.WriteLine($"📡 Cargando cursos para el usuario {_userId}...");

            var enrollments = await _apiService.GetEnrollmentsAsync(_userId);

            if (enrollments == null || !enrollments.Any())
            {
                Console.WriteLine("⚠ No se encontraron inscripciones para este usuario.");
                return;
            }

            Courses.Clear();
            FilteredCourses.Clear();

            foreach (var enrollment in enrollments)
            {
                var course = await _apiService.GetCourseByIdAsync(enrollment.CourseID);
                if (course != null)
                {
                    Console.WriteLine($"✅ Curso cargado: {course.Name}");
                    Courses.Add(course);
                }
                else
                {
                    Console.WriteLine($"❌ No se encontró información para el curso con ID {enrollment.CourseID}");
                }
            }

            FilterCourses();
            OnPropertyChanged(nameof(IsCourseListEmpty));
        }


        private void FilterCourses()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FilteredCourses.Clear();
                foreach (var course in Courses)
                {
                    FilteredCourses.Add(course);
                }
            }
            else
            {
                var lowerSearchText = SearchText.ToLower();
                FilteredCourses.Clear();
                
                foreach (var course in Courses.Where(c => c.Name.ToLower().Contains(lowerSearchText) || c.TeacherID.ToString().Contains(lowerSearchText)))
                {
                    FilteredCourses.Add(course);
                }
            }
            
            OnPropertyChanged(nameof(IsCourseListEmpty));
        }

        private async Task EditCourseAsync(Course course)
        {
            if (_userRole != 2)
            {
                Console.WriteLine("⛔ No tienes permiso para editar cursos.");
                return;
            }

            Console.WriteLine($"📝 Editando curso: {course.Name}");
        }
    }
}
