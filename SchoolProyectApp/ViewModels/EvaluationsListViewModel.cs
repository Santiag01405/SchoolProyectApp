using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using SchoolProyectApp.Models;
using SchoolProyectApp.Services;
using System.Linq;
using System.Collections.Generic;

namespace SchoolProyectApp.ViewModels
{
    [QueryProperty(nameof(SelectedChild), "SelectedChild")]
    [QueryProperty(nameof(IsParentView), "IsParentView")]
    public class EvaluationsListViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private int _userId;
        private int _roleId;
        private string _pageTitle = "Mis Evaluaciones";

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        private Child _selectedChild;
        public Child SelectedChild
        {
            get => _selectedChild;
            set
            {
                SetProperty(ref _selectedChild, value);
                _ = InitializeAsync();
            }
        }

        public string PageTitle
        {
            get => _pageTitle;
            set => SetProperty(ref _pageTitle, value);
        }

        private string _selectedFilter = "Venideras"; // Filtro predeterminado
        public string SelectedFilter
        {
            get => _selectedFilter;
            set
            {
                SetProperty(ref _selectedFilter, value);
                _ = LoadEvaluations();
            }
        }

        public int RoleID
        {
            get => _roleId;
            set
            {
                if (SetProperty(ref _roleId, value))
                {
                    OnPropertyChanged(nameof(IsProfessor));
                    OnPropertyChanged(nameof(IsStudent));
                    OnPropertyChanged(nameof(IsParent));
                    OnPropertyChanged(nameof(IsHiddenForProfessor));
                    OnPropertyChanged(nameof(IsHiddenForStudent));
                }
            }
        }

        public bool IsProfessor => RoleID == 2;
        public bool IsStudent => RoleID == 1;
        public bool IsParent => RoleID == 3;
        public bool IsHiddenForProfessor => !IsProfessor;
        public bool IsHiddenForStudent => !IsStudent;

        public ObservableCollection<Evaluation> Evaluations { get; set; } = new();
        public ObservableCollection<Course> Courses { get; set; } = new();

        public ICommand DeleteEvaluationCommand { get; }
        public ICommand HomeCommand { get; }
        public ICommand ProfileCommand { get; }
        public ICommand CourseCommand { get; }
        public ICommand OpenMenuCommand { get; }
        public ICommand FirstProfileCommand { get; }
        public ICommand EvaluationCommand { get; }
        public ICommand GoBackCommand { get; }

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
            GoBackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));

            // La inicialización ahora se hace en InitializeAsync()
        }

        public async Task GoBackAsync()
        {
            await Shell.Current.GoToAsync("..");
        }

        public async Task InitializeAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                await LoadUserData();
                await LoadCourses();
                await LoadEvaluations();
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task LoadEvaluations()
        {
            var schoolIdStr = await SecureStorage.GetAsync("school_id");
            if (!int.TryParse(schoolIdStr, out int schoolId))
            {
                return;
            }

            int targetUserId;
            if (SelectedChild != null)
            {
                targetUserId = SelectedChild.UserID;
                PageTitle = $"Evaluaciones de {SelectedChild.StudentName}";
            }
            else
            {
                var userIdStr = await SecureStorage.GetAsync("user_id");
                if (!int.TryParse(userIdStr, out targetUserId))
                {
                    return;
                }
                PageTitle = "Mis Evaluaciones";
            }

            _userId = targetUserId;

            var evaluations = await _apiService.GetEvaluationsAsync(_userId, schoolId);
            if (evaluations == null) return;

            var coursesDict = Courses.ToDictionary(c => c.CourseID, c => c);
            foreach (var eval in evaluations)
            {
                if (coursesDict.TryGetValue(eval.CourseID, out var course))
                    eval.Course = course;
                else
                    eval.Course = new Course { Name = "(Curso no asignado)" };
            }

            IEnumerable<Evaluation> filteredEvaluations;
            if (SelectedFilter == "Venideras")
            {
                filteredEvaluations = evaluations
                    .Where(e => e.Date.Date >= System.DateTime.Today)
                    .OrderBy(e => e.Date);
            }
            else if (SelectedFilter == "Pasadas")
            {
                filteredEvaluations = evaluations
                    .Where(e => e.Date.Date < System.DateTime.Today)
                    .OrderByDescending(e => e.Date);
            }
            else // "Todas"
            {
                filteredEvaluations = evaluations.OrderByDescending(e => e.Date);
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Evaluations.Clear();
                foreach (var eval in filteredEvaluations)
                {
                    Evaluations.Add(eval);
                }
            });
        }

        public async Task LoadUserData()
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
                    }
                }
                else
                {
                    RoleID = 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar datos de usuario: {ex.Message}");
            }
        }

        public async Task LoadCourses()
        {
            int schoolId = int.Parse(await SecureStorage.GetAsync("school_id") ?? "0");
            var courses = await _apiService.GetCoursesAsync(schoolId);

            if (courses == null || courses.Count == 0) return;

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
                await LoadEvaluations();
            }
        }

        private bool _isParentView;
        public bool IsParentView
        {
            get => _isParentView;
            set
            {
                SetProperty(ref _isParentView, value);
                OnPropertyChanged(nameof(ShowParentHeader));
            }
        }

        public bool ShowParentHeader => IsParentView;

    }
}
/*using System.Collections.ObjectModel;
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

            Task.Run(async () => await LoadEvaluations());
        }
          

public async Task InitializeAsync()
        {
            // Carga los datos de forma secuencial y en el orden correcto
            await LoadUserData();
            await LoadCourses();
            await LoadEvaluations();
        }

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

            if (evaluations == null)
            {
                Console.WriteLine("⚠ Evaluaciones es null.");
                return;
            }

            Console.WriteLine($"📌 Evaluaciones recibidas: {evaluations.Count}");

            // ✅ Asegurar que los cursos estén cargados
            if (Courses.Count == 0)
            {
                var courses = await _apiService.GetCoursesAsync(schoolId);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Courses.Clear();
                    foreach (var c in courses)
                        Courses.Add(c);
                });
            }

            // ✅ Vincular cada evaluación con su curso
            var coursesDict = Courses.ToDictionary(c => c.CourseID, c => c);
            foreach (var eval in evaluations)
            {
                if (coursesDict.TryGetValue(eval.CourseID, out var course))
                    eval.Course = course;
                else
                    eval.Course = new Course { Name = "(Curso no asignado)" };
            }

            var filtered = evaluations
                .Where(e => e.Date.Date >= DateTime.Today)
                .OrderBy(e => e.Date)
                .ToList();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Evaluations.Clear();
                foreach (var eval in filtered)
                {
                    Console.WriteLine($"📌 Mostrando evaluación: {eval.Title} - {eval.Course?.Name}");
                    Evaluations.Add(eval);
                }
            });
        }


        public async Task LoadUserData()
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

        public async Task LoadCourses()
        {
            int schoolId = int.Parse(await SecureStorage.GetAsync("school_id") ?? "0");

            var courses = await _apiService.GetCoursesAsync(schoolId);

            if (courses == null || courses.Count == 0)
            {
                Console.WriteLine("[DOTNET] ⚠ No se encontraron cursos en la API.");
                return;
            }

            Console.WriteLine($"[DOTNET] 📌 Cursos recibidos: {courses.Count}");

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
                Console.WriteLine("[DOTNET] ✔ Evaluación eliminada.");
                await LoadEvaluations();
            }
        }
    }
}*/


