using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using SchoolProyectApp.Models;
using SchoolProyectApp.Services;

namespace SchoolProyectApp.ViewModels
{
    public class EvaluationsViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private int _userId;

        private int _roleId;

        private int _schoolId;


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


        private string _searchQuery;
        private User _selectedUser;
        private Course _selectedCourse;
        private string _evaluationTitle;

        public ObservableCollection<User> SearchResults { get; set; } = new();
        public ObservableCollection<Course> Courses { get; set; } = new();
        public ObservableCollection<Evaluation> Evaluations { get; set; } = new();

        private bool _isCourseVisible;
        public bool IsCourseVisible
        {
            get => _isCourseVisible;
            set
            {
                _isCourseVisible = value;
                OnPropertyChanged();
            }
        }

        private bool _isSearchPopupVisible;
        public bool IsSearchPopupVisible
        {
            get => _isSearchPopupVisible;
            set
            {
                _isSearchPopupVisible = value;
                OnPropertyChanged();
            }
        }

        private bool _isTitleVisible;
        public bool IsTitleVisible
        {
            get => _isTitleVisible;
            set
            {
                _isTitleVisible = value;
                OnPropertyChanged();
            }
        }

        private bool _isDescriptionVisible;
        public bool IsDescriptionVisible
        {
            get => _isDescriptionVisible;
            set
            {
                _isDescriptionVisible = value;
                OnPropertyChanged();
            }
        }

        private bool _isDateVisible;
        public bool IsDateVisible
        {
            get => _isDateVisible;
            set
            {
                _isDateVisible = value;
                OnPropertyChanged();
            }
        }


        /*public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                _searchQuery = value;
                OnPropertyChanged();
            }
        }*/

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                _searchQuery = value;
                OnPropertyChanged();
                IsCourseVisible = !string.IsNullOrWhiteSpace(_searchQuery);
            }
        }

        public User SelectedUser
        {
            get => _selectedUser;
            set
            {
                _selectedUser = value;
                OnPropertyChanged();
            }
        }

        /* public Course SelectedCourse
         {
             get => _selectedCourse;
             set
             {
                 _selectedCourse = value;
                 OnPropertyChanged();
             }
         }*/

        public Course SelectedCourse
        {
            get => _selectedCourse;
            set
            {
                _selectedCourse = value;
                OnPropertyChanged();
                IsTitleVisible = _selectedCourse != null;
            }
        }


        /*public string EvaluationTitle
        {
            get => _evaluationTitle;
            set
            {
                _evaluationTitle = value;
                OnPropertyChanged();
            }
        }*/

        public string EvaluationTitle
        {
            get => _evaluationTitle;
            set
            {
                _evaluationTitle = value;
                OnPropertyChanged();
                IsDescriptionVisible = !string.IsNullOrWhiteSpace(_evaluationTitle);
            }
        }

        private string _description;
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged();
                IsDateVisible = !string.IsNullOrWhiteSpace(_description);
            }
        }

        private bool _isCreateButtonVisible;
        public bool IsCreateButtonVisible
        {
            get => _isCreateButtonVisible;
            set
            {
                _isCreateButtonVisible = value;
                OnPropertyChanged();
            }
        }

        public DateTime SelectedDate
        {
            get => NewEvaluation.Date;
            set
            {
                if (NewEvaluation.Date != value)
                {
                    NewEvaluation.Date = value;
                    OnPropertyChanged();
                    IsCreateButtonVisible = NewEvaluation.Date >= DateTime.Today;
                }
            }
        }


        public Evaluation NewEvaluation { get; set; } = new()
        {
            Date = DateTime.Now
        };



        public ICommand SearchUsersCommand { get; }
        public ICommand CreateEvaluationCommand { get; }
        public ICommand LoadCoursesCommand { get; }
        public ICommand HomeCommand { get; }
        public ICommand ProfileCommand { get; }
        public ICommand OpenMenuCommand { get; }
        public ICommand FirstProfileCommand { get; }

        public ICommand ResetCommand { get; }

        public ICommand CloseSearchPopupCommand { get; }

        public EvaluationsViewModel()
        {
            _apiService = new ApiService();
            SearchUsersCommand = new Command(async () => await SearchUsers());
            CreateEvaluationCommand = new Command(async () => await CreateEvaluation());
            LoadCoursesCommand = new Command(async () => await LoadCourses());

            HomeCommand = new Command(async () => await Shell.Current.GoToAsync("///homepage"));
            ProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///profile"));
            OpenMenuCommand = new Command(async () => await Shell.Current.GoToAsync("///menu"));
            FirstProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///firtsprofile"));

            ResetCommand = new Command(ResetPage);

            CloseSearchPopupCommand = new Command(() => IsSearchPopupVisible = false);

            Task.Run(async () =>
            {
                await LoadEvaluations();
                await LoadCourses();
            });

            _apiService = new ApiService();
            Task.Run(async () => await LoadUserData());
        }

        /*private async Task LoadUserData()
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
        }*/
        private async Task LoadUserData()
        {
            try
            {
                var storedUserId = await SecureStorage.GetAsync("user_id");
                var storedSchoolId = await SecureStorage.GetAsync("school_id");

                if (!string.IsNullOrEmpty(storedUserId) && int.TryParse(storedUserId, out int userId))
                {
                    _userId = userId;
                }

                if (!string.IsNullOrEmpty(storedSchoolId) && int.TryParse(storedSchoolId, out int schoolId))
                {
                    _schoolId = schoolId;
                }

                if (_userId > 0)
                {
                    var user = await _apiService.GetUserDetailsAsync(_userId);
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

        /*public async Task LoadEvaluations()
        {
            _userId = int.Parse(await SecureStorage.GetAsync("user_id") ?? "0");
            if (_userId == 0) return;

            var evaluations = await _apiService.GetEvaluationsAsync(_userId);
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Evaluations.Clear();
                foreach (var eval in evaluations)
                {
                    Evaluations.Add(eval);
                }
            });
        }*/

        public async Task LoadEvaluations()
        {
            _userId = int.Parse(await SecureStorage.GetAsync("user_id") ?? "0");
            var schoolIdStr = await SecureStorage.GetAsync("school_id");

            if (_userId == 0 || string.IsNullOrEmpty(schoolIdStr) || !int.TryParse(schoolIdStr, out int schoolId))
            {
                Console.WriteLine("⚠ No se encontró school_id, no se cargaron evaluaciones.");
                return;
            }

            Console.WriteLine($"🔍 Cargando evaluaciones para UserID={_userId}, SchoolID={schoolId}");

            var evaluations = await _apiService.GetEvaluationsAsync(_userId, schoolId);

            if (evaluations == null)
            {
                Console.WriteLine("⚠ La API devolvió null.");
                return;
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
                    Console.WriteLine($"📌 Evaluación: {eval.Title} ({eval.Date})");
                    Evaluations.Add(eval);
                }
            });
        }



        private void ResetPage()
        {
            SearchQuery = string.Empty;
            SelectedUser = null;
            SelectedCourse = null;
            EvaluationTitle = string.Empty;
            Description = string.Empty;
            NewEvaluation = new Evaluation { Date = DateTime.Today };
            SearchResults.Clear();

            IsCourseVisible = false;
            IsTitleVisible = false;
            IsDescriptionVisible = false;
            IsDateVisible = false;

            OnPropertyChanged(nameof(SearchQuery));
            OnPropertyChanged(nameof(SelectedUser));
            OnPropertyChanged(nameof(SelectedCourse));
            OnPropertyChanged(nameof(EvaluationTitle));
            OnPropertyChanged(nameof(Description));
            OnPropertyChanged(nameof(NewEvaluation));
            OnPropertyChanged(nameof(SearchResults));

            OnPropertyChanged(nameof(IsCourseVisible));
            OnPropertyChanged(nameof(IsTitleVisible));
            OnPropertyChanged(nameof(IsDescriptionVisible));
            OnPropertyChanged(nameof(IsDateVisible));
        }

        private async Task LoadCourses()
        {
            int schoolId = int.Parse(await SecureStorage.GetAsync("school_id") ?? "0");
            if (schoolId == 0) return;

            var courses = await _apiService.GetCoursesAsync(schoolId);
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Courses.Clear();
                foreach (var course in courses)
                {
                    Courses.Add(course);
                }
            });
        }


        private async Task SearchUsers()
        {
            if (string.IsNullOrEmpty(SearchQuery)) return;

            var users = await _apiService.SearchUsersAsync(SearchQuery);
            SearchResults.Clear();
            foreach (var user in users)
            {
                SearchResults.Add(user);
            }

            //IsSearchPopupVisible = true;
        }

        /*private async Task CreateEvaluation()
        {
            Console.WriteLine("📌 Intentando crear evaluación...");

            if (NewEvaluation.Date < DateTime.Now)
            {
                Console.WriteLine("❌ No puedes asignar evaluaciones con fecha pasada.");
                await Shell.Current.DisplayAlert("Fecha inválida", "No puedes asignar evaluaciones con fecha pasada.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(EvaluationTitle))
            {
                Console.WriteLine("❌ El título de la evaluación es obligatorio.");
                await Shell.Current.DisplayAlert("Falta título", "El título de la evaluación es obligatorio.", "OK");
                return;
            }

            // 🔹 Asigna valores seleccionados
            NewEvaluation.Title = EvaluationTitle;
            NewEvaluation.UserID = SelectedUser?.UserID ?? 0;
            NewEvaluation.CourseID = SelectedCourse?.CourseID ?? 0;

            bool success = await _apiService.CreateEvaluationAsync(NewEvaluation);
            if (success)
            {
                Console.WriteLine("✔ Evaluación creada correctamente.");
                await Shell.Current.DisplayAlert("Éxito", "Evaluación creada correctamente.", "OK");
                await LoadEvaluations();
                ResetPage();
            }
            else
            {
                Console.WriteLine("❌ No se pudo asignar una evaluación.");
                await Shell.Current.DisplayAlert("Error", "No se pudo crear la evaluación. Intenta de nuevo.", "OK");
            }
        }*/

        private async Task CreateEvaluation()
        {
            Console.WriteLine("📌 Intentando crear evaluación...");

            if (NewEvaluation.Date < DateTime.Now)
            {
                await Shell.Current.DisplayAlert("Fecha inválida", "No puedes asignar evaluaciones con fecha pasada.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(EvaluationTitle))
            {
                await Shell.Current.DisplayAlert("Falta título", "El título de la evaluación es obligatorio.", "OK");
                return;
            }

            // ✅ Asignar valores
            NewEvaluation.Title = EvaluationTitle;
            NewEvaluation.UserID = SelectedUser?.UserID ?? 0;
            NewEvaluation.CourseID = SelectedCourse?.CourseID ?? 0;
            NewEvaluation.SchoolID = _schoolId; // <-- Se agrega aquí

            bool success = await _apiService.CreateEvaluationAsync(NewEvaluation);
            if (success)
            {
                await Shell.Current.DisplayAlert("Éxito", "Evaluación creada correctamente.", "OK");
                await LoadEvaluations();
                ResetPage();
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "No se pudo crear la evaluación. Intenta de nuevo.", "OK");
            }
        }

    }
}







//-----------------------------------------------------------------------------------------------
/*using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using SchoolProyectApp.Models;
using SchoolProyectApp.Services;

namespace SchoolProyectApp.ViewModels
{
    public class EvaluationsViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private int _userId;

        // 🔍 Para la búsqueda de usuarios
        private string _searchQuery;
        private User _selectedUser;

        public ObservableCollection<User> SearchResults { get; set; } = new();
        public ObservableCollection<Evaluation> Evaluations { get; set; } = new();

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                _searchQuery = value;
                OnPropertyChanged();
            }
        }

        public User SelectedUser
        {
            get => _selectedUser;
            set
            {
                _selectedUser = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanCreateEvaluation)); // ✅ Habilita la creación de evaluaciones
            }
        }

        public Evaluation NewEvaluation { get; set; } = new();

        public bool CanCreateEvaluation => SelectedUser != null;

        public ICommand SearchUsersCommand { get; }
        public ICommand CreateEvaluationCommand { get; }

        public EvaluationsViewModel()
        {
            _apiService = new ApiService();
            SearchUsersCommand = new Command(async () => await SearchUsers());
            CreateEvaluationCommand = new Command(async () => await CreateEvaluation(), () => CanCreateEvaluation);

            Task.Run(async () => await LoadEvaluations());
        }

        public async Task LoadEvaluations()
        {
            _userId = int.Parse(await SecureStorage.GetAsync("user_id") ?? "0");

            if (_userId == 0) return;

            var evaluations = await _apiService.GetEvaluationsAsync(_userId);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Evaluations.Clear();
                foreach (var eval in evaluations)
                {
                    Evaluations.Add(eval);
                }
            });
        }

        private async Task SearchUsers()
        {
            if (string.IsNullOrEmpty(SearchQuery)) return;

            var users = await _apiService.SearchUsersAsync(SearchQuery);
            SearchResults.Clear();
            foreach (var user in users)
            {
                SearchResults.Add(user);
            }
        }

        private async Task CreateEvaluation()
        {
            if (SelectedUser == null)
            {
                Console.WriteLine("❌ Debes seleccionar un usuario.");
                return;
            }

            // 🔹 Asigna el ID del usuario seleccionado
            NewEvaluation.UserID = SelectedUser.UserID;

            bool success = await _apiService.CreateEvaluationAsync(NewEvaluation);
            if (success)
            {
                Console.WriteLine("✔ Evaluación creada correctamente.");
                await LoadEvaluations();
            }
            else
            {
                Console.WriteLine("XXX No se pudo asignar una evaluación.");
            }
        }
    }
}*/




//-------------------------------------------------------------------------------------------------
/*using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using SchoolProyectApp.Models;
using SchoolProyectApp.Services;


    namespace SchoolProyectApp.ViewModels
{
    public class EvaluationsViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private int _userId;

        public ObservableCollection<Evaluation> Evaluations { get; set; } = new();
        public ObservableCollection<Course> Courses { get; set; } = new(); // 🆕 Lista de cursos

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

        public EvaluationsViewModel()
        {
            _apiService = new ApiService();
            SearchUsersCommand = new Command(async () => await SearchUsers());
            CreateEvaluationCommand = new Command(async () => await CreateEvaluation());
            DeleteEvaluationCommand = new Command<Evaluation>(async (evaluation) => await DeleteEvaluation(evaluation));

            Task.Run(async () => await LoadEvaluations());
            Task.Run(async () => await LoadCourses()); 
        }

        public async Task LoadEvaluations()
        {
            _userId = int.Parse(await SecureStorage.GetAsync("user_id") ?? "0");
            if (_userId == 0) return;

            var evaluations = await _apiService.GetEvaluationsAsync(_userId);
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Evaluations.Clear();
                foreach (var eval in evaluations)
                {
                    Evaluations.Add(eval);
                }
            });
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

        private async Task SearchUsers()
        {
            if (string.IsNullOrEmpty(SearchQuery)) return;

            var users = await _apiService.SearchUsersAsync(SearchQuery);
            SearchResults.Clear();
            foreach (var user in users)
            {
                SearchResults.Add(user);
            }
        }

        /*private async Task SearchUsers()
        {
            var users = await _apiService.SearchUsersAsync(SearchQuery);
            Console.WriteLine($"🔍 Se encontraron {users.Count} usuarios.");
        }//

        private async Task CreateEvaluation()
        {
            if (SelectedCourse == null)
            {
                Console.WriteLine("❌ Debes seleccionar un curso.");
                return;
            }

            if (NewEvaluation.Date < DateTime.Now)
            {
                Console.WriteLine("⚠ No se pueden crear evaluaciones en el pasado.");
                return;
            }

            // 🔹 Asignar CourseID correctamente antes de enviarlo
            NewEvaluation.CourseID = SelectedCourse.CourseID;

            bool success = await _apiService.CreateEvaluationAsync(NewEvaluation);
            if (success)
            {
                Console.WriteLine("✔ Evaluación creada correctamente.");
                await LoadEvaluations();
            }
            else
            {
                Console.WriteLine("XXX No se pudo asignar una evaluación.");
            }
        }


        /*private async Task CreateEvaluation()
        {
            if (NewEvaluation.Date < DateTime.Now)
            {
                Console.WriteLine("⚠ No se pueden crear evaluaciones en el pasado.");
                return;
            }

            if (SelectedCourse == 0)
            {
                Console.WriteLine("❌ Debes seleccionar un curso.");
                return;
            }

            var storedUserId = await SecureStorage.GetAsync("user_id");
            if (!string.IsNullOrEmpty(storedUserId) && int.TryParse(storedUserId, out int userId))
            {
                NewEvaluation.UserID = userId;
                NewEvaluation.CourseID = SelectedCourse; // 🆕 Asignar el curso seleccionado
            }
            else
            {
                Console.WriteLine("❌ No se pudo obtener el UserID del almacenamiento.");
                return;
            }

            Console.WriteLine($"📌 Enviando evaluación con UserID: {NewEvaluation.UserID}, CourseID: {NewEvaluation.CourseID}");

            bool success = await _apiService.CreateEvaluationAsync(NewEvaluation);
            if (success)
            {
                Console.WriteLine("✔ Evaluación creada correctamente.");
                await LoadEvaluations();
            }
            else
            {
                Console.WriteLine("XXX No se pudo asignar una evaluación.");
            }
        }//

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

//----------------------------------------------------------------------------------------------
/*using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using SchoolProyectApp.Models;
using SchoolProyectApp.Services;

namespace SchoolProyectApp.ViewModels
{
    public class EvaluationsViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private int _userId;

        public ObservableCollection<Evaluation> Evaluations { get; set; } = new();

        public Evaluation NewEvaluation { get; set; } = new();

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

        public ICommand SearchUsersCommand { get; }
        public ICommand CreateEvaluationCommand { get; }
        public ICommand DeleteEvaluationCommand { get; }

        public EvaluationsViewModel()
        {
            _apiService = new ApiService();
            SearchUsersCommand = new Command(async () => await SearchUsers());
            CreateEvaluationCommand = new Command(async () => await CreateEvaluation());
            DeleteEvaluationCommand = new Command<Evaluation>(async (evaluation) => await DeleteEvaluation(evaluation));

            NewEvaluation = new Evaluation
            {
                Date = DateTime.Now
            };

            Task.Run(async () => await LoadEvaluations());
        }


        public async Task LoadEvaluations()
        {
            _userId = int.Parse(await SecureStorage.GetAsync("user_id") ?? "0");

            if (_userId == 0) return;

            var evaluations = await _apiService.GetEvaluationsAsync(_userId);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Evaluations.Clear();
                foreach (var eval in evaluations)
                {
                    Evaluations.Add(eval);
                }
            });
        }

        private async Task SearchUsers()
        {
            var users = await _apiService.SearchUsersAsync(SearchQuery);
            Console.WriteLine($"🔍 Se encontraron {users.Count} usuarios.");
        }

        private async Task CreateEvaluation()
        {
            if (NewEvaluation.Date < DateTime.Now)
            {
                Console.WriteLine("⚠ No se pueden crear evaluaciones en el pasado.");
                return;
            }

            // 🔹 Asegurar que el UserID sea correcto
            var storedUserId = await SecureStorage.GetAsync("user_id");
            if (!string.IsNullOrEmpty(storedUserId) && int.TryParse(storedUserId, out int userId))
            {
                NewEvaluation.UserID = userId;
            }
            else
            {
                Console.WriteLine("❌ No se pudo obtener el UserID del almacenamiento.");
                return;
            }

            Console.WriteLine($"📌 Enviando evaluación con UserID: {NewEvaluation.UserID}");

            bool success = await _apiService.CreateEvaluationAsync(NewEvaluation);
            if (success)
            {
                Console.WriteLine("✔ Evaluación creada correctamente.");
                await LoadEvaluations();
            }
            else
            {
                Console.WriteLine("XXX No se pudo asignar una evaluación.");
            }
        }



        /*private async Task CreateEvaluation()
        {
            if (NewEvaluation.Date < DateTime.Now)
            {
                Console.WriteLine("⚠ No se pueden crear evaluaciones en el pasado.");
                return;
            }

            bool success = await _apiService.CreateEvaluationAsync(NewEvaluation);
            if (success)
            {
                Console.WriteLine("✔ Evaluación creada correctamente.");
                await LoadEvaluations();
            }
            else { Console.WriteLine("XXX No se pudo asignar una evaluaciom"); }
        }//

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