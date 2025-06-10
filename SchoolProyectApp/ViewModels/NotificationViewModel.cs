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

        public ObservableCollection<Notification> RegularNotifications { get; set; } = new();
        public ObservableCollection<AttendanceNotification> AttendanceNotifications { get; set; } = new();
        public ObservableCollection<object> ActiveNotifications { get; set; } = new();

        private string _selectedTab = "Normales";
        public string SelectedTab
        {
            get => _selectedTab;
            set
            {
                if (_selectedTab != value)
                {
                    _selectedTab = value;
                    OnPropertyChanged();
                    UpdateActiveNotifications();
                }
            }
        }

        public ICommand SwitchTabCommand => new Command<string>((tab) => SelectedTab = tab);

        public ICommand RefreshCommand { get; }
        public ICommand HomeCommand { get; }
        public ICommand ProfileCommand { get; }
        public ICommand OpenMenuCommand { get; }
        public ICommand CourseCommand { get; }
        public ICommand FirstProfileCommand { get; }
        public ICommand SendNotificationCommand { get; }
        public ICommand MarkAsReadCommand { get; }
        public ICommand DeleteNotificationCommand { get; }
        public ICommand DeleteAttendanceCommand { get; }


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

        public bool IsProfessor => RoleID == 2;
        public bool IsStudent => RoleID == 1;
        public bool IsParent => RoleID == 3;
        public bool IsHiddenForProfessor => !IsProfessor;
        public bool IsHiddenForStudent => !IsStudent;
        public bool HasNotifications => ActiveNotifications.Count > 0;


        public NotificationViewModel()
        {
            _apiService = new ApiService();

            HomeCommand = new Command(async () => await Shell.Current.GoToAsync("///homepage"));
            ProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///profile"));
            OpenMenuCommand = new Command(async () => await Shell.Current.GoToAsync("///menu"));
            CourseCommand = new Command(async () => await Shell.Current.GoToAsync("///courses"));
            FirstProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///firtsprofile"));
            SendNotificationCommand = new Command(async () => await Shell.Current.GoToAsync("///sendNotification"));

            MarkAsReadCommand = new Command<Notification>(async (notification) => await MarkAsReadAsync(notification));


            //Eliminar notificacion
            DeleteNotificationCommand = new Command<Notification>(async (notification) =>
            {
                if (notification == null) return;

                bool confirm = await Application.Current.MainPage.DisplayAlert(
                    "Eliminar", "¿Deseas eliminar esta notificación?", "Sí", "No");

                if (!confirm) return;

                if (notification.NotifyID > 0)
                {
                    var success = await _apiService.DeleteNotificationAsync(notification.NotifyID);
                    if (!success)
                    {
                        await Application.Current.MainPage.DisplayAlert("Error", "No se pudo eliminar la notificación del servidor.", "OK");
                        return;
                    }
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    RegularNotifications.Remove(notification);
                    ActiveNotifications.Remove(notification);
                    UpdateActiveNotifications();
                });
            });

            //Eliminar asistencia
            DeleteAttendanceCommand = new Command<AttendanceNotification>(async (attendance) =>
            {
                if (attendance == null) return;

                bool confirm = await Application.Current.MainPage.DisplayAlert(
                    "Eliminar", "¿Deseas eliminar esta notificación de asistencia?", "Sí", "No");

                if (!confirm) return;

                var success = await _apiService.DeleteAttendanceAsync(attendance.AttendanceID);

                if (success)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        AttendanceNotifications.Remove(attendance);
                        UpdateActiveNotifications();
                    });
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "No se pudo eliminar la asistencia.", "OK");
                }
            });



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
                RegularNotifications.Clear();
                foreach (var notification in notifications)
                    RegularNotifications.Add(notification);

                UpdateActiveNotifications();
            });
        }

       private async Task LoadAttendanceAsNotifications(CancellationToken token)
        {
            if (token.IsCancellationRequested) return;

            var userId = await SecureStorage.GetAsync("user_id");
            if (string.IsNullOrEmpty(userId)) return;

            var attendanceRecords = await _apiService.GetAttendanceNotifications(int.Parse(userId));
            if (token.IsCancellationRequested || attendanceRecords == null || attendanceRecords.Count == 0) return;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                AttendanceNotifications.Clear();
                foreach (var a in attendanceRecords)
                {
                    AttendanceNotifications.Add(a); 
                }
            });

        }

        private void UpdateActiveNotifications()
        {
            ActiveNotifications.Clear();

            if (SelectedTab == "Normales")
            {
                foreach (var n in RegularNotifications)
                    ActiveNotifications.Add(n);
            }
            else
            {
                foreach (var a in AttendanceNotifications)
                    ActiveNotifications.Add(a);
            }
            OnPropertyChanged(nameof(HasNotifications));
        }


        private async Task MarkAsReadAsync(Notification notification)
        {
            if (notification == null || notification.IsRead) return;

            var success = await _apiService.MarkNotificationAsReadAsync(notification.NotifyID);
            if (success)
            {
                notification.IsRead = true;
                OnPropertyChanged(nameof(ActiveNotifications));
            }
        }
    }
}




       
