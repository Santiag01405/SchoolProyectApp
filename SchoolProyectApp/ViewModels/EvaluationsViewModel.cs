using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using SchoolProyectApp.Models;
using SchoolProyectApp.Services;
using CommunityToolkit.Mvvm.Input;

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
                }
            }
        }

        public bool IsProfessor => RoleID == 2;
        public bool IsStudent => RoleID == 1;
        public bool IsParent => RoleID == 3;

        private Course _selectedCourse;
        private string _evaluationTitle;
        private string _description;

        public ObservableCollection<Course> Courses { get; set; } = new();
        public ObservableCollection<Evaluation> Evaluations { get; set; } = new();

        public Course SelectedCourse
        {
            get => _selectedCourse;
            set
            {
                SetProperty(ref _selectedCourse, value);
                IsTitleVisible = _selectedCourse != null;
            }
        }

        public string EvaluationTitle
        {
            get => _evaluationTitle;
            set
            {
                SetProperty(ref _evaluationTitle, value);
                IsDescriptionVisible = !string.IsNullOrWhiteSpace(_evaluationTitle);
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                SetProperty(ref _description, value);
                IsDateVisible = !string.IsNullOrWhiteSpace(_description);
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

        public Evaluation NewEvaluation { get; set; } = new() { Date = DateTime.Now };

        private bool _isTitleVisible;
        public bool IsTitleVisible
        {
            get => _isTitleVisible;
            set => SetProperty(ref _isTitleVisible, value);
        }

        private bool _isDescriptionVisible;
        public bool IsDescriptionVisible
        {
            get => _isDescriptionVisible;
            set => SetProperty(ref _isDescriptionVisible, value);
        }

        private bool _isDateVisible;
        public bool IsDateVisible
        {
            get => _isDateVisible;
            set => SetProperty(ref _isDateVisible, value);
        }

        private bool _isCreateButtonVisible;
        public bool IsCreateButtonVisible
        {
            get => _isCreateButtonVisible;
            set => SetProperty(ref _isCreateButtonVisible, value);
        }

        public ICommand CreateEvaluationCommand { get; }
        public ICommand LoadCoursesCommand { get; }
        public ICommand HomeCommand { get; }
        public ICommand ProfileCommand { get; }
        public ICommand OpenMenuCommand { get; }
        public ICommand FirstProfileCommand { get; }
        public ICommand ResetCommand { get; }

        public EvaluationsViewModel()
        {
            _apiService = new ApiService();
            CreateEvaluationCommand = new AsyncRelayCommand(CreateEvaluation);
            LoadCoursesCommand = new AsyncRelayCommand(LoadCourses);
            HomeCommand = new AsyncRelayCommand(() => Shell.Current.GoToAsync("///homepage"));
            ProfileCommand = new AsyncRelayCommand(() => Shell.Current.GoToAsync("///profile"));
            OpenMenuCommand = new AsyncRelayCommand(() => Shell.Current.GoToAsync("///menu"));
            FirstProfileCommand = new AsyncRelayCommand(() => Shell.Current.GoToAsync("///firtsprofile"));
            ResetCommand = new RelayCommand(ResetPage);

            Task.Run(async () =>
            {
                await LoadUserData();
                if (IsProfessor)
                {
                    await LoadCourses();
                }
                else
                {
                    await LoadEvaluations();
                }
            });
        }

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

        public async Task LoadEvaluations()
        {
            _userId = int.Parse(await SecureStorage.GetAsync("user_id") ?? "0");
            _schoolId = int.Parse(await SecureStorage.GetAsync("school_id") ?? "0");

            if (_userId == 0 || _schoolId == 0) return;

            var evaluations = await _apiService.GetEvaluationsAsync(_userId, _schoolId);
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Evaluations.Clear();
                foreach (var eval in evaluations)
                {
                    Evaluations.Add(eval);
                }
            });
        }

        private void ResetPage()
        {
            SelectedCourse = null;
            EvaluationTitle = string.Empty;
            Description = string.Empty;
            NewEvaluation = new Evaluation { Date = DateTime.Today };

            IsTitleVisible = false;
            IsDescriptionVisible = false;
            IsDateVisible = false;
            IsCreateButtonVisible = false;

            OnPropertyChanged(nameof(SelectedCourse));
            OnPropertyChanged(nameof(EvaluationTitle));
            OnPropertyChanged(nameof(Description));
            OnPropertyChanged(nameof(NewEvaluation));
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

        private async Task CreateEvaluation()
        {
            Console.WriteLine("📌 Intentando crear evaluación...");

            if (SelectedCourse == null)
            {
                await Shell.Current.DisplayAlert("Falta curso", "Debes seleccionar un curso.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(EvaluationTitle))
            {
                await Shell.Current.DisplayAlert("Falta título", "El título de la evaluación es obligatorio.", "OK");
                return;
            }

            if (NewEvaluation.Date < DateTime.Today)
            {
                await Shell.Current.DisplayAlert("Fecha inválida", "No puedes asignar evaluaciones con fecha pasada.", "OK");
                return;
            }

            NewEvaluation.Title = EvaluationTitle;
            NewEvaluation.Description = Description;
            NewEvaluation.UserID = _userId; // Asignamos el ID del profesor
            NewEvaluation.CourseID = SelectedCourse.CourseID;
            NewEvaluation.SchoolID = _schoolId;

            bool success = await _apiService.CreateEvaluationAsync(NewEvaluation);
            if (success)
            {
                await Shell.Current.DisplayAlert("Éxito", "Evaluación creada correctamente.", "OK");
                // Cargar de nuevo las evaluaciones para el profesor (si es el caso)
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

        public async Task LoadEvaluations()
        {
            _userId = int.Parse(await SecureStorage.GetAsync("user_id") ?? "0");
            _schoolId = int.Parse(await SecureStorage.GetAsync("school_id") ?? "0");

            if (_userId == 0 || _schoolId == 0) return;

            var evaluations = await _apiService.GetEvaluationsAsync(_userId, _schoolId);
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Evaluations.Clear();
                foreach (var eval in evaluations)
                {
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

            var schoolIdStr = await SecureStorage.GetAsync("school_id");
            if (!int.TryParse(schoolIdStr, out int schoolId) || schoolId == 0)
            {
                Console.WriteLine("⚠ No se pudo obtener schoolId.");
                return;
            }

            Console.WriteLine($"🔎 Buscando usuarios con query='{SearchQuery}' y schoolId={schoolId}");

            var users = await _apiService.SearchUsersAsync(SearchQuery, schoolId);
            SearchResults.Clear();

            if (users != null && users.Count > 0)
            {
                foreach (var user in users)
                {
                    SearchResults.Add(user);
                }
            }
            else
            {
                Console.WriteLine("⚠ No se encontraron usuarios en la API.");
            }
        }

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
}*/







