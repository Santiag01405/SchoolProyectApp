    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Microsoft.Maui.Controls;
    using SchoolProyectApp.Models;
    using SchoolProyectApp.Services;

namespace SchoolProyectApp.ViewModels
{
    public class HomePageViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private string _userName;
        private int _userId;
        public ObservableCollection<Evaluation> UpcomingEvaluations { get; set; } = new();

        public ObservableCollection<Course> TodaysClasses { get; set; } = new();

        private Course _nextClass;
        public Course NextClass
        {
            get => _nextClass;
            set
            {
                _nextClass = value;
                OnPropertyChanged();
            }
        }


        public string Today => $"📅 Hoy es {DateTime.Now:dddd dd 'de' MMMM}";

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

        public string WelcomeMessage => $"¡Bienvenido, {UserName}!";

        public ObservableCollection<Notification> Notifications { get; set; } = new();
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

        public HomePageViewModel()
        {
            _apiService = new ApiService();

            // 🔹 Load user from API instead of SecureStorage
            Task.Run(async () => await LoadUserDataFromApi());
            //Task.Run(async () => await LoadNotifications());
            //Task.Run(async () => await LoadAttendanceAsNotifications());
            Task.Run(async () => await LoadCombinedNotifications());
            Task.Run(async () => await LoadHomepageData());


            HomeCommand = new Command(async () => await Shell.Current.GoToAsync("///homepage"));
            ProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///profile"));
            CourseCommand = new Command(async () => await Shell.Current.GoToAsync("///courses"));
            OpenMenuCommand = new Command(async () => await Shell.Current.GoToAsync("///menu"));
            LogoutCommand = new Command(async () => await Logout());
            RefreshCommand = new Command(async () => await LoadUserDataFromApi());
            FirstProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///firtsprofile"));
            CreateEvaluationCommand = new Command(async () => await Shell.Current.GoToAsync("///createEvaluation"));
            EvaluationCommand = new Command(async () => await Shell.Current.GoToAsync("///evaluation"));

            //Notificaciones
            NotificationCommand = new Command(async () => await Shell.Current.GoToAsync("///notification"));

            // 🔹 Automatically update username when modified in ProfilePage
            MessagingCenter.Subscribe<ProfileViewModel, string>(this, "UserUpdated", async (sender, newUserName) =>
            {
                Console.WriteLine($"🔄 Received new username from Profile: {newUserName}");
                UserName = newUserName;
            });

            _apiService = new ApiService();
            Task.Run(async () => await LoadUserData());

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

        public async Task LoadUserDataFromApi()
        {
            try
            {
                var storedUserId = await SecureStorage.GetAsync("user_id");

                if (string.IsNullOrEmpty(storedUserId) || !int.TryParse(storedUserId, out _userId))
                {
                    Console.WriteLine("❌ Ninguna ID encontrada");
                    UserName = "Usuario";
                    return;
                }

                Console.WriteLine($"🌍 Fetching user data from API for ID: {_userId}");
                var user = await _apiService.GetUserDetailsAsync(_userId);

                if (user != null)
                {
                    Console.WriteLine($"✔ Fetched username from API: {user.UserName}");
                    UserName = user.UserName;
                }
                else
                {
                    Console.WriteLine("❌ Failed to fetch user from API.");
                    UserName = "Usuario";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error fetching user: {ex.Message}");
                UserName = "Usuario";
            }
        }

        /* private async Task LoadNotifications()
         {
             try
             {
                 var userId = await SecureStorage.GetAsync("user_id");
                 if (string.IsNullOrEmpty(userId)) return;

                 var notificationsFromApi = await _apiService.GetUserNotifications(int.Parse(userId));

                 Console.WriteLine($"📥 Notificaciones recibidas: {notificationsFromApi?.Count ?? 0}");

                 Notifications.Clear();

                 if (notificationsFromApi != null && notificationsFromApi.Any())
                 {
                     var recentNotifications = notificationsFromApi
                         .OrderByDescending(n => n.Date) // Ordenar por fecha descendente
                         .Take(3) // Solo las 3 más recientes
                         .ToList();

                     foreach (var notification in recentNotifications)
                     {
                         Console.WriteLine($"📢 Agregando notificación: {notification.Title} - {notification.Date}");
                         Notifications.Add(notification);
                     }

                     HasNotifications = Notifications.Count > 0;
                     Console.WriteLine($"🔄 HasNotifications actualizado a: {HasNotifications}");
                 }
                 else
                 {
                     Console.WriteLine("❌ No hay notificaciones en la API.");
                 }
             }
             catch (Exception ex)
             {
                 Console.WriteLine($"❌ Error en LoadNotifications: {ex.Message}");
             }
         }

         //Notidicaciones de asistencias a padres
         private async Task LoadAttendanceAsNotifications()
         {

             var userId = await SecureStorage.GetAsync("user_id");
             if (string.IsNullOrEmpty(userId)) return;

             var attendanceRecords = await _apiService.GetAttendanceNotifications(int.Parse(userId));

             if (attendanceRecords == null || attendanceRecords.Count == 0) return;

             Console.WriteLine($"Se recibieron {attendanceRecords.Count} registros de asistencia");

             var rawId = await SecureStorage.GetAsync("user_id");
             Console.WriteLine($"ID de usuario actual: {rawId}");


             foreach (var a in attendanceRecords)
             {
                 Notifications.Add(new Notification
                 {
                     Title = $"Asistencia de su representado",
                     Content = $"Su representado estuvo {a.Status} en el curso {a.CourseName}",
                     Date = a.Date
                 });
             }
         }*/

        private async Task LoadCombinedNotifications()
        {
            try
            {
                var userIdStr = await SecureStorage.GetAsync("user_id");
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId)) return;

                var regularNotifications = await _apiService.GetUserNotifications(userId) ?? new List<Notification>();

                var attendanceRecords = await _apiService.GetAttendanceNotifications(userId) ?? new List<AttendanceNotification>();

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

                Notifications.Clear();
                foreach (var notification in allNotifications)
                {
                    Notifications.Add(notification);
                }

                HasNotifications = Notifications.Count > 0;
                Console.WriteLine($"✅ Cargadas {Notifications.Count} notificaciones recientes.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en LoadCombinedNotifications: {ex.Message}");
            }
        }

        public async Task LoadHomepageData()
        {
            var userIdStr = await SecureStorage.GetAsync("user_id");
            if (!int.TryParse(userIdStr, out var userId)) return;

            // 🔹 Evaluaciones
            var allEvaluations = await _apiService.GetEvaluationsAsync(userId);
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

            // 🔹 Clases de hoy
            var allSchedule = await _apiService.GetUserWeeklySchedule(userId);

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

                Console.WriteLine($"📚 Clases de hoy ({today}): {TodaysClasses.Count}");
            });
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

    



