using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using SchoolProyectApp.Models;
using SchoolProyectApp.Services;
using System.Linq;

namespace SchoolProyectApp.ViewModels
{
    public class EvaluationsListViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private int _userId;
        private int _roleId;


        public int RoleID
        {
            get => _roleId;
            set
            {
                if (_roleId != value)
                {
                    _roleId = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsProfessor));
                    OnPropertyChanged(nameof(IsStudent));
                    OnPropertyChanged(nameof(IsParent));
                    OnPropertyChanged(nameof(IsHiddenForProfessor));
                    OnPropertyChanged(nameof(IsHiddenForStudent));
                }
            }
        }

        // Propiedades booleanas para Binding
        public bool IsProfessor => RoleID == 2;
        public bool IsStudent => RoleID == 1;
        public bool IsParent => RoleID == 3;
        public bool IsHiddenForProfessor => !IsProfessor;
        public bool IsHiddenForStudent => !IsStudent;

        public ObservableCollection<Evaluation> Evaluations { get; set; } = new();
        public ObservableCollection<Course> Courses { get; set; } = new();

        public Evaluation NewEvaluation { get; set; } = new();
        public Course SelectedCourse { get; set; }

        public ICommand DeleteEvaluationCommand { get; }
        public ICommand HomeCommand { get; }
        public ICommand ProfileCommand { get; }
        public ICommand CourseCommand { get; }
        public ICommand OpenMenuCommand { get; }
        public ICommand FirstProfileCommand { get; }
        public ICommand EvaluationCommand { get; }

        private string _searchQuery;
        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                _searchQuery = value;
                OnPropertyChanged();
            }
        }

        /*public EvaluationsListViewModel()
        {
            _apiService = new ApiService();

            DeleteEvaluationCommand = new Command<Evaluation>(async (evaluation) => await DeleteEvaluation(evaluation));

            HomeCommand = new Command(async () => await Shell.Current.GoToAsync("///homepage"));
            ProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///profile"));
            CourseCommand = new Command(async () => await Shell.Current.GoToAsync("///courses"));
            OpenMenuCommand = new Command(async () => await Shell.Current.GoToAsync("///menu"));
            FirstProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///firtsprofile"));
            EvaluationCommand = new Command(async () => await Shell.Current.GoToAsync("///evaluation"));

            Task.Run(async () =>
            {
                await LoadUserData();
                await LoadEvaluations();
                await LoadCourses();
            });
        }*/
        public EvaluationsListViewModel()
        {
            _apiService = new ApiService();

            DeleteEvaluationCommand = new Command<Evaluation>(async (evaluation) => await DeleteEvaluation(evaluation));

            HomeCommand = new Command(async () => await Shell.Current.GoToAsync("///homepage"));
            ProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///profile"));
            CourseCommand = new Command(async () => await Shell.Current.GoToAsync("///courses"));
            OpenMenuCommand = new Command(async () => await Shell.Current.GoToAsync("///menu"));
            FirstProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///firtsprofile"));
            EvaluationCommand = new Command(async () => await Shell.Current.GoToAsync("///evaluation"));

            Task.Run(async () =>
            {
                await LoadUserData();

                // 🔹 Esperar a que school_id esté disponible
                int retries = 0;
                int schoolId = 0;
                while (schoolId == 0 && retries < 5)
                {
                    var schoolStr = await SecureStorage.GetAsync("school_id");
                    int.TryParse(schoolStr, out schoolId);
                    if (schoolId == 0)
                        await Task.Delay(300);
                    retries++;
                }

                await LoadEvaluations();  // ✅ ahora con IDs válidos
                await LoadCourses();
            });
        }


        /*public async Task LoadEvaluations()
        {
            _userId = int.Parse(await SecureStorage.GetAsync("user_id") ?? "0");
            int schoolId = int.Parse(await SecureStorage.GetAsync("school_id") ?? "0");
            if (_userId == 0 || schoolId == 0) return;

            var evaluations = await _apiService.GetEvaluationsAsync(_userId, schoolId);

            var filtered = evaluations
                .Where(e => e.Date.Date >= DateTime.Today)
                .OrderBy(e => e.Date)
                .ToList();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Evaluations.Clear();
                foreach (var eval in filtered)
                {
                    Evaluations.Add(eval);
                }
            });

            var coursesDict = Courses.ToDictionary(c => c.CourseID);
            foreach (var eval in evaluations)
            {
                if (coursesDict.TryGetValue(eval.CourseID, out var course))
                    eval.Course = course;
            }
        }*/
        public async Task LoadEvaluations()
        {
            var userStr = await SecureStorage.GetAsync("user_id");
            var schoolStr = await SecureStorage.GetAsync("school_id");

            Console.WriteLine($"🔎 LoadEvaluations() -> user_id: {userStr}, school_id: {schoolStr}");

            _userId = int.Parse(userStr ?? "0");
            int schoolId = int.Parse(schoolStr ?? "0");

            if (_userId == 0 || schoolId == 0)
            {
                Console.WriteLine("⚠ No se cargaron evaluaciones porque userID o schoolID son 0.");
                return;
            }

            Console.WriteLine($"🌍 GET: api/evaluations?userID={_userId}&schoolId={schoolId}");
            var evaluations = await _apiService.GetEvaluationsAsync(_userId, schoolId);

        }
        private async Task LoadUserData()
        {
            try
            {
                var storedUserId = await SecureStorage.GetAsync("user_id");

                if (!string.IsNullOrEmpty(storedUserId) && int.TryParse(storedUserId, out int userId))
                {
                    var user = await _apiService.GetUserDetailsAsync(userId);

                    if (user != null)
                    {
                        RoleID = user.RoleID;
                        OnPropertyChanged(nameof(RoleID));
                        OnPropertyChanged(nameof(IsProfessor));
                        OnPropertyChanged(nameof(IsStudent));
                        OnPropertyChanged(nameof(IsParent));
                    }
                }
                else
                {
                    RoleID = 0;
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No se pudo cargar el usuario: " + ex.Message, "OK");
            }
        }

        private async Task LoadCourses()
        {
            int schoolId = int.Parse(await SecureStorage.GetAsync("school_id") ?? "0");

            var courses = await _apiService.GetCoursesAsync(schoolId);

            if (courses == null || courses.Count == 0)
            {
                Console.WriteLine("⚠ No se encontraron cursos en la API.");
                return;
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Courses.Clear();
                foreach (var course in courses)
                {
                    Courses.Add(course);
                }
            });
        }

        private async Task DeleteEvaluation(Evaluation evaluation)
        {
            bool success = await _apiService.DeleteEvaluationAsync(evaluation.EvaluationID, _userId);
            if (success)
            {
                Console.WriteLine("✔ Evaluación eliminada.");
                await LoadEvaluations();
            }
        }
    }
}


/*using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using SchoolProyectApp.Models;
using SchoolProyectApp.Services;
using SchoolProyectApp.ViewModels;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;


namespace SchoolProyectApp.ViewModels
{
    public class EvaluationsListViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private int _userId;

        private int _roleId;

        public int RoleID
        {
            get => _roleId;
            set
            {
                if (_roleId != value)
                {
                    _roleId = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsProfessor));
                    OnPropertyChanged(nameof(IsStudent));
                    OnPropertyChanged(nameof(IsParent));
                    OnPropertyChanged(nameof(IsHiddenForProfessor));
                    OnPropertyChanged(nameof(IsHiddenForStudent));
                }
            }
        }


        // Propiedades booleanas para Binding en XAML
        public bool IsProfessor => RoleID == 2;
        public bool IsStudent => RoleID == 1;
        public bool IsParent => RoleID == 3;

        public bool IsHiddenForProfessor => !IsProfessor;
        public bool IsHiddenForStudent => !IsStudent;


        public ObservableCollection<Evaluation> Evaluations { get; set; } = new();
        public ObservableCollection<Course> Courses { get; set; } = new();

        public Evaluation NewEvaluation { get; set; } = new();
        private int _selectedCourseID;

        private Course _selectedCourse;
        public Course SelectedCourse
        {
            get => _selectedCourse;
            set
            {
                _selectedCourse = value;
                OnPropertyChanged();
            }
        }


        public ICommand SearchUsersCommand { get; }
        public ICommand CreateEvaluationCommand { get; }
        public ICommand DeleteEvaluationCommand { get; }
        public ICommand HomeCommand { get; }
        public ICommand ProfileCommand { get; }
        public ICommand CourseCommand { get; }
        public ICommand OpenMenuCommand { get; }
        public ICommand FirstProfileCommand { get; }
        public ICommand EvaluationCommand { get; }


        private string _searchQuery;
        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                _searchQuery = value;
                OnPropertyChanged();
            }
        }

        public EvaluationsListViewModel()
        {
            _apiService = new ApiService();
            DeleteEvaluationCommand = new Command<Evaluation>(async (evaluation) => await DeleteEvaluation(evaluation));

            Task.Run(async () => await LoadEvaluations());
            Task.Run(async () => await LoadCourses());

            HomeCommand = new Command(async () => await Shell.Current.GoToAsync("///homepage"));
            ProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///profile"));
            CourseCommand = new Command(async () => await Shell.Current.GoToAsync("///courses"));
            OpenMenuCommand = new Command(async () => await Shell.Current.GoToAsync("///menu"));
            FirstProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///firtsprofile"));
            CreateEvaluationCommand = new Command(async () => await Shell.Current.GoToAsync("///createEvaluation"));
            EvaluationCommand = new Command(async () => await Shell.Current.GoToAsync("///evaluation"));

            _apiService = new ApiService();
            Task.Run(async () => await LoadUserData());

        }

        public async Task LoadEvaluations()
        {
            _userId = int.Parse(await SecureStorage.GetAsync("user_id") ?? "0");
            if (_userId == 0) return;

            var evaluations = await _apiService.GetEvaluationsAsync(_userId);

            // Filtrar solo las evaluaciones con fecha mayor o igual a hoy
            var filtered = evaluations
                .Where(e => e.Date.Date >= DateTime.Today)
                .OrderBy(e => e.Date) // opcional: ordenamos por fecha ascendente
                .ToList();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Evaluations.Clear();
                foreach (var eval in filtered)
                {
                    Evaluations.Add(eval);
                }
            });
            /* foreach (var eval in evaluations)
             {
                 Console.WriteLine($"✅ Eval: {eval.Title}, Curso: {eval.Course?.Name}");
             }
var coursesDict = Courses.ToDictionary(c => c.CourseID);
            foreach (var eval in evaluations)
            {
                if (coursesDict.TryGetValue(eval.CourseID, out var course))
                    eval.Course = course;
            }

        }

        private async Task LoadUserData()
        {
            try
            {
                var storedUserId = await SecureStorage.GetAsync("user_id");

                if (!string.IsNullOrEmpty(storedUserId) && int.TryParse(storedUserId, out int userId))
                {
                    var user = await _apiService.GetUserDetailsAsync(userId);

                    if (user != null)
                    {

                        RoleID = user.RoleID; // 🔹 Aquí nos aseguramos de que se asigne correctamente


                        // 🔹 Forzar actualización en UI
                        OnPropertyChanged(nameof(RoleID));
                        OnPropertyChanged(nameof(IsProfessor));
                        OnPropertyChanged(nameof(IsStudent));
                        OnPropertyChanged(nameof(IsParent));
                    }
                }
                else
                {
                    RoleID = 0; // Asignar un valor por defecto si no se encuentra el usuario
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No se pudo cargar el usuario: " + ex.Message, "OK");
            }
        }
        private async Task LoadCourses()
        {
            Console.WriteLine("🔄 Cargando cursos desde la API...");

            var courses = await _apiService.GetCoursesAsync();

            if (courses == null || courses.Count == 0)
            {
                Console.WriteLine("⚠ No se encontraron cursos en la API.");
                return;
            }

            Console.WriteLine($"✅ Cursos recibidos: {courses.Count}");

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Courses.Clear();
                foreach (var course in courses)
                {
                    Console.WriteLine($"📚 Curso: {course.Name} (ID: {course.CourseID})");
                    Courses.Add(course);
                }
            });
        }

        
        private async Task DeleteEvaluation(Evaluation evaluation)
        {
            bool success = await _apiService.DeleteEvaluationAsync(evaluation.EvaluationID, _userId);
            if (success)
            {
                Console.WriteLine("✔ Evaluación eliminada.");
                await LoadEvaluations();
            }
        }
    }
}*/

