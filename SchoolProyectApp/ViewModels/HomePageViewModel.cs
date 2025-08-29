using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using SchoolProyectApp.Models;
using SchoolProyectApp.Services;
using Microsoft.Maui.Graphics;

namespace SchoolProyectApp.ViewModels
{
    public class HomePageViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private string _userName;
        private int _userId;
        private int _schoolId;
        private int _roleId;

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        private string _currentLapsoName;
        public string CurrentLapsoName
        {
            get => _currentLapsoName;
            set => SetProperty(ref _currentLapsoName, value);
        }
        public ObservableCollection<Evaluation> UpcomingEvaluations { get; set; } = new();
        public ObservableCollection<Course> TodaysClasses { get; set; } = new();
        public ObservableCollection<Notification> Notifications { get; set; } = new();
        public ObservableCollection<Child> Hijos { get; set; } = new();

        public ICommand HomeCommand { get; }
        public ICommand ProfileCommand { get; }
        public ICommand CourseCommand { get; }
        public ICommand OpenMenuCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand FirstProfileCommand { get; }
        public ICommand NotificationCommand { get; }
        public ICommand CreateEvaluationCommand { get; }
        public ICommand EvaluationCommand { get; }
        public ICommand SelectChildCommand { get; }

        public string UserName
        {
            get => _userName;
            set
            {
                if (_userName != value)
                {
                    _userName = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(WelcomeMessage));
                }
            }
        }

        private string _studentOverallAverage;
        public string StudentOverallAverage
        {
            get => _studentOverallAverage;
            set => SetProperty(ref _studentOverallAverage, value);
        }
        public string WelcomeMessage => $"¡Bienvenido, {UserName}!";

        private string _schoolName;
        public string SchoolName
        {
            get => _schoolName;
            set
            {
                if (_schoolName != value)
                {
                    _schoolName = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(WelcomeSchoolMessage));
                }
            }
        }
        public string WelcomeSchoolMessage => $"{SchoolName} - ¡Bienvenido, {UserName}!";

        public string Today => $"📅 Hoy es {DateTime.Now:dddd dd 'de' MMMM}";

        public int RoleID
        {
            get => _roleId;
            set
            {
                if (_roleId != value)
                {
                    _roleId = value;
                    OnPropertyChanged();
                    // Notificar cambios para las propiedades de visibilidad
                    OnPropertyChanged(nameof(IsProfessor));
                    OnPropertyChanged(nameof(IsStudent));
                    OnPropertyChanged(nameof(IsParent));
                    OnPropertyChanged(nameof(IsNurseAndProfessor));
                }
            }
        }

        public bool IsProfessor => RoleID == 2;
        public bool IsStudent => RoleID == 1;
        public bool IsParent => RoleID == 3;
        public bool IsNurseAndProfessor => !IsStudent & !IsParent;

        private bool _hasNotifications;
        public bool HasNotifications
        {
            get => _hasNotifications;
            set
            {
                if (_hasNotifications != value)
                {
                    _hasNotifications = value;
                    OnPropertyChanged();
                }
            }
        }


        // Propiedades de colores
        private Color _primaryColor = Color.FromArgb("#0C4251");
        public Color PrimaryColor
        {
            get => _primaryColor;
            set => SetProperty(ref _primaryColor, value);
        }

        private Color _secondaryColor = Color.FromArgb("#6bbdda");
        public Color SecondaryColor
        {
            get => _secondaryColor;
            set => SetProperty(ref _secondaryColor, value);
        }

        private Color _textColor = Colors.Black;
        public Color TextColor
        {
            get => _textColor;
            set => SetProperty(ref _textColor, value);
        }

        private Color _pageBackgroundColor = Colors.White;
        public Color PageBackgroundColor
        {
            get => _pageBackgroundColor;
            set => SetProperty(ref _pageBackgroundColor, value);
        }


        public HomePageViewModel()
        {
            _apiService = new ApiService();

            // ❌ El constructor no debe bloquearse con await.
            // Creamos un método de inicialización que se puede llamar desde la vista.
            // Para la carga inicial, usamos Task.Run para no bloquear el constructor.
            Task.Run(async () => await InitializeAsync());

            HomeCommand = new Command(async () => await Shell.Current.GoToAsync("///homepage"));
            ProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///profile"));
            CourseCommand = new Command(async () => await Shell.Current.GoToAsync("///courses"));
            OpenMenuCommand = new Command(async () => await Shell.Current.GoToAsync("///menu"));
            LogoutCommand = new Command(async () => await Logout());
            RefreshCommand = new Command(async () => await RefreshData());
            FirstProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///firtsprofile"));
            CreateEvaluationCommand = new Command(async () => await Shell.Current.GoToAsync("///createEvaluation"));
            EvaluationCommand = new Command(async () => await Shell.Current.GoToAsync("///evaluation"));
            NotificationCommand = new Command(async () => await Shell.Current.GoToAsync("///notification"));
            SelectChildCommand = new Command<Child>(async (child) => await OnSelectChild(child));

            MessagingCenter.Subscribe<ProfileViewModel, string>(this, "UserUpdated", (sender, newUserName) =>
            {
                UserName = newUserName;
            });
        }

        /// <summary>
        /// Método principal para inicializar y cargar todos los datos de la página.
        /// </summary>
        public async Task InitializeAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                await LoadUserDataFromApi();
                await LoadCombinedNotifications();
                await LoadHomepageData();
                await LoadUserSchoolName();
                if (IsStudent) // Opcional, pero recomendable para no cargar datos innecesarios
                {
                    await LoadStudentOverallAverageAsync();
                }
                if (_schoolId != 0)
                {
                    var currentLapso = await _apiService.GetCurrentLapsoAsync(_schoolId);
                    CurrentLapsoName = currentLapso != null ? currentLapso.Nombre : "Lapso no encontrado";
                }
                else
                {
                    CurrentLapsoName = "Lapso no asignado";
                }
            }
            finally
            {
                IsBusy = false;
            }

        }
        private async Task RefreshData()
        {
            await InitializeAsync();
        }

        public async Task LoadUserSchoolName()
        {
            var name = await SecureStorage.GetAsync("school_name");
            SchoolName = !string.IsNullOrEmpty(name) ? name : "Colegio no asignado";
        }

        public async Task LoadUserDataFromApi()
        {
            try
            {
                var storedUserId = await SecureStorage.GetAsync("user_id");
                if (string.IsNullOrEmpty(storedUserId) || !int.TryParse(storedUserId, out _userId))
                {
                    UserName = "Usuario";
                    return;
                }

                var user = await _apiService.GetUserDetailsAsync(_userId);
                if (user != null)
                {
                    UserName = user.UserName;
                    _schoolId = user.SchoolID;
                    SchoolName = user.School?.Name ?? "Mi Colegio";
                    RoleID = user.RoleID;

                    // 🎨 aplicar colores dinámicos
                    // 🎨 aplicar colores dinámicos
                    if (_schoolId == 5)
                    {
                        PrimaryColor = Color.FromArgb("#0d4483");       // PrimaryColor
                        SecondaryColor = Color.FromArgb("#0098da");     // SecondaryColor
                        TextColor = Color.FromArgb("#FFFFFF");          // PrimaryTextColor
                        PageBackgroundColor = Color.FromArgb("#FFFFFF");// PageBackgroundColor
                    }
                    else
                    {
                        PrimaryColor = Color.FromArgb("#0C4251");
                        SecondaryColor = Color.FromArgb("#6bbdda");
                        TextColor = Colors.Black;
                        PageBackgroundColor = Colors.White;
                    }


                    if (IsParent)
                    {
                        await LoadHijosAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error fetching user: {ex.Message}");
                UserName = "Usuario";
            }

        }

        private async Task LoadCombinedNotifications()
        {
            if (_schoolId == 0) return; // Asegurar que tenemos el SchoolID antes de continuar

            try
            {
                var userIdStr = await SecureStorage.GetAsync("user_id");
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId)) return;

                var regularNotifications = await _apiService.GetUserNotifications(userId, _schoolId) ?? new List<Notification>();
                var attendanceRecords = await _apiService.GetAttendanceNotifications(userId, _schoolId) ?? new List<AttendanceNotification>();

                var attendanceNotifications = attendanceRecords.Select(a => new Notification
                {
                    Title = "Asistencia de su representado",
                    Content = $"Su representado estuvo {a.Status} en el curso {a.CourseName}",
                    Date = a.Date
                }).ToList();

                var allNotifications = regularNotifications
                                        .Concat(attendanceNotifications)
                                        .OrderByDescending(n => n.Date)
                                        .Take(3)
                                        .ToList();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Notifications.Clear();
                    foreach (var notification in allNotifications)
                        Notifications.Add(notification);
                    HasNotifications = Notifications.Count > 0;
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en LoadCombinedNotifications: {ex.Message}");
            }
        }

        public async Task LoadHomepageData()
        {
            if (_schoolId == 0) return; // Asegurar que tenemos el SchoolID antes de continuar

            var userIdStr = await SecureStorage.GetAsync("user_id");
            if (!int.TryParse(userIdStr, out var userId)) return;

            var allEvaluations = await _apiService.GetEvaluationsAsync(userId, _schoolId);
            var nextEvaluations = allEvaluations?
                .Where(e => e.Date > DateTime.Now)
                .OrderBy(e => e.Date)
                .Take(1)
                .ToList();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                UpcomingEvaluations.Clear();
                if (nextEvaluations != null)
                {
                    foreach (var eval in nextEvaluations)
                        UpcomingEvaluations.Add(eval);
                }
            });

            var allSchedule = await _apiService.GetUserWeeklySchedule(userId, _schoolId);
            var today = (int)DateTime.Now.DayOfWeek;

            var todayCourses = allSchedule?
                .Where(c => c.DayOfWeek == today)
                .OrderBy(c => c.CourseName)
                .Select(c => new Course
                {
                    CourseID = c.CourseID,
                    Name = c.CourseName
                })
                .ToList();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                TodaysClasses.Clear();
                if (todayCourses != null)
                {
                    foreach (var course in todayCourses)
                        TodaysClasses.Add(course);
                }
            });
        }

        public async Task LoadStudentOverallAverageAsync()
        {
            var userId = await SecureStorage.GetAsync("user_id");
            var schoolId = await SecureStorage.GetAsync("school_id");
            if (!int.TryParse(userId, out int uId) || !int.TryParse(schoolId, out int schId)) return;

            var averageResult = await _apiService.GetStudentOverallAverageAsync(uId, schId);
            if (averageResult != null)
            {
                StudentOverallAverage = averageResult.OverallAverage.ToString("F2", CultureInfo.InvariantCulture);
            }
            else
            {
                StudentOverallAverage = "N/A";
            }
        }

        private async Task LoadHijosAsync()
        {
            try
            {
                var hijosList = await _apiService.GetHijosAsync(_userId, _schoolId);
                if (hijosList != null)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        Hijos.Clear();
                        foreach (var hijo in hijosList)
                        {
                            Hijos.Add(hijo);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al cargar los hijos: {ex.Message}");
            }
        }

        private async Task OnSelectChild(Child child)
        {
            if (child == null)
                return;

            try
            {
                // Navegamos al dashboard del hijo, pasando su UserID en la URL.
                // Esta es la forma recomendada y más robusta.
                await Shell.Current.GoToAsync($"childDashboard?studentId={child.UserID}&childName={Uri.EscapeDataString(child.StudentName)}");


            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al navegar al dashboard del hijo: {ex.Message}");
            }
        }

        private async Task Logout()
        {
            if (Application.Current.MainPage is AppShell shell)
            {
                await shell.Logout();
            }
        }
    }
}