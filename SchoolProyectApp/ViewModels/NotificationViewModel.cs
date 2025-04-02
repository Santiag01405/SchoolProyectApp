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
            RefreshCommand = new Command(async () => await LoadNotifications());

            Task.Run(async () => await LoadNotifications()); // Carga inicial

            HomeCommand = new Command(async () => await Shell.Current.GoToAsync("///homepage"));
            ProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///profile"));
            OpenMenuCommand = new Command(async () => await Shell.Current.GoToAsync("///menu"));
            CourseCommand = new Command(async () => await Shell.Current.GoToAsync("///courses"));
            FirstProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///firtsprofile"));
            SendNotificationCommand = new Command(async () => await Shell.Current.GoToAsync("///sendNotification"));

            _apiService = new ApiService();
            Task.Run(async () => await LoadUserData());
            Task.Run(async () => await LoadAttendanceAsNotifications());


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
        private async Task LoadNotifications()
        {
            var userId = await SecureStorage.GetAsync("user_id");
            if (string.IsNullOrEmpty(userId)) return;

            var notifications = await _apiService.GetUserNotifications(int.Parse(userId));

            Notifications.Clear();
            foreach (var notification in notifications)
            {
                Notifications.Add(notification);
            }
        }

        //Notidicaciones de asistencias a padres
        private async Task LoadAttendanceAsNotifications()
        {

            var userId = await SecureStorage.GetAsync("user_id");
            if (string.IsNullOrEmpty(userId)) return;

            var attendanceRecords = await _apiService.GetAttendanceNotifications(int.Parse(userId));

            if (attendanceRecords == null || attendanceRecords.Count == 0) return;

            Console.WriteLine($"🔍 Se recibieron {attendanceRecords.Count} registros de asistencia");

            var rawId = await SecureStorage.GetAsync("user_id");
            Console.WriteLine($"🧠 ID de usuario actual: {rawId}");


            foreach (var a in attendanceRecords)
            {
                Notifications.Add(new Notification
                {
                    Title = $"Asistencia de {a.StudentName}",
                    Content = $"Su hijo estuvo {a.Status} en el curso {a.CourseName}",
                    Date = a.Date
                });
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

       

    
