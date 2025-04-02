using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using SchoolProyectApp.Models;
using SchoolProyectApp.Services;

namespace SchoolProyectApp.ViewModels
{
    public class NotificationViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private int _roleId;
        private CancellationTokenSource _cancellationTokenSource = new();

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


        public ObservableCollection<Notification> Notifications { get; set; } = new ObservableCollection<Notification>();
        public ICommand RefreshCommand { get; }
        public ICommand HomeCommand { get; }
        public ICommand ProfileCommand { get; }
        public ICommand OpenMenuCommand { get; }
        public ICommand CourseCommand { get; }
        public ICommand FirstProfileCommand { get; }
        public ICommand SendNotificationCommand { get; }

        public NotificationViewModel()
        {
            _apiService = new ApiService();
            //RefreshCommand = new Command(async () => await LoadNotifications());

           // Task.Run(async () => await LoadNotifications()); // Carga inicial

            HomeCommand = new Command(async () => await Shell.Current.GoToAsync("///homepage"));
            ProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///profile"));
            OpenMenuCommand = new Command(async () => await Shell.Current.GoToAsync("///menu"));
            CourseCommand = new Command(async () => await Shell.Current.GoToAsync("///courses"));
            FirstProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///firtsprofile"));
            SendNotificationCommand = new Command(async () => await Shell.Current.GoToAsync("///sendNotification"));

            _apiService = new ApiService();
            //Task.Run(async () => await LoadUserData());
            //Task.Run(async () => await LoadAttendanceAsNotifications());

            Task.Run(async () =>
            {
                await LoadNotifications(_cancellationTokenSource.Token);
                await LoadAttendanceAsNotifications(_cancellationTokenSource.Token);
                await LoadUserData(_cancellationTokenSource.Token);
            });


        }

        public void CancelTasks()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = new();
        }

        private async Task LoadUserData(CancellationToken token)
        {
            try
            {
                if (token.IsCancellationRequested) return;

                var storedUserId = await SecureStorage.GetAsync("user_id");
                if (string.IsNullOrEmpty(storedUserId) || !int.TryParse(storedUserId, out int userId))
                {
                    RoleID = 0;
                    return;
                }

                var user = await _apiService.GetUserDetailsAsync(userId);
                if (token.IsCancellationRequested || user == null) return;

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    RoleID = user.RoleID;
                    OnPropertyChanged(nameof(RoleID));
                    OnPropertyChanged(nameof(IsProfessor));
                    OnPropertyChanged(nameof(IsStudent));
                    OnPropertyChanged(nameof(IsParent));
                });
            }
            catch (Exception ex)
            {
                if (!token.IsCancellationRequested)
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                        Application.Current.MainPage.DisplayAlert("Error", "No se pudo cargar el usuario: " + ex.Message, "OK"));
                }
            }
        }

        private async Task LoadNotifications(CancellationToken token)
        {
            if (token.IsCancellationRequested) return;

            var userId = await SecureStorage.GetAsync("user_id");
            if (string.IsNullOrEmpty(userId)) return;

            var notifications = await _apiService.GetUserNotifications(int.Parse(userId));
            if (token.IsCancellationRequested) return;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Notifications.Clear();
                foreach (var notification in notifications)
                {
                    Notifications.Add(notification);
                }
            });
        }

        private async Task LoadAttendanceAsNotifications(CancellationToken token)
        {
            if (token.IsCancellationRequested) return;

            var userId = await SecureStorage.GetAsync("user_id");
            if (string.IsNullOrEmpty(userId)) return;

            var attendanceRecords = await _apiService.GetAttendanceNotifications(int.Parse(userId));
            if (token.IsCancellationRequested || attendanceRecords == null || attendanceRecords.Count == 0) return;

            Console.WriteLine($"Se recibieron {attendanceRecords.Count} registros de asistencia");

            MainThread.BeginInvokeOnMainThread(() =>
            {
                foreach (var a in attendanceRecords)
                {
                    Notifications.Add(new Notification
                    {
                        Title = $"Asistencia de su representado",
                        Content = $"Su representado estuvo {a.Status} en el curso {a.CourseName}",
                        Date = a.Date
                    });
                }
            });
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
    public class NotificationViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        public ObservableCollection<Notification> Notifications { get; set; } = new ObservableCollection<Notification>();

        public ICommand RefreshCommand { get; }
        public ICommand HomeCommand { get; }
        public ICommand ProfileCommand { get; }
        public ICommand OpenMenuCommand { get; }
        public ICommand CourseCommand { get; }
        public ICommand FirstProfileCommand { get; }

        public NotificationViewModel()
        {
            _apiService = new ApiService();
            RefreshCommand = new Command(async () => await LoadNotifications());
            Task.Run(async () => await LoadNotifications());

            HomeCommand = new Command(async () => await Shell.Current.GoToAsync("///homepage"));
            ProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///profile"));
            OpenMenuCommand = new Command(async () => await Shell.Current.GoToAsync("///menu"));
            CourseCommand = new Command(async () => await Shell.Current.GoToAsync("///courses"));
            FirstProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///firtsprofile"));
        }*/

       /* public void OnDisappearing()
        {
            Notifications.Clear(); // Limpia la lista para evitar errores de acceso a memoria
        }*/
        /*private async Task LoadNotifications()
        {
            var userId = await SecureStorage.GetAsync("user_id");
            if (string.IsNullOrEmpty(userId)) return;

            var notifications = await _apiService.GetUserNotifications(int.Parse(userId));

            // Ejecutamos en el hilo principal para evitar problemas con la UI
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Notifications.Clear(); // Limpiamos antes de agregar
                foreach (var notification in notifications)
                {
                    Notifications.Add(notification);
                }
            });
        }*/

        /*private async Task LoadNotifications()
        {
            var userId = await SecureStorage.GetAsync("user_id");
            if (string.IsNullOrEmpty(userId))
            {
                return;
            }

            var notifications = await _apiService.GetUserNotifications(int.Parse(userId));

            Notifications.Clear();

            if (notifications != null && notifications.Count > 0)
            {
                foreach (var item in notifications)
                {
                    Notifications.Add(item);
                }
            }
        }*/

       

    
