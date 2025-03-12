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
            Task.Run(async () => await LoadNotifications());

            HomeCommand = new Command(async () => await Shell.Current.GoToAsync("///homepage"));
            ProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///profile"));
            CourseCommand = new Command(async () => await Shell.Current.GoToAsync("///courses"));
            OpenMenuCommand = new Command(async () => await Shell.Current.GoToAsync("///menu"));
            LogoutCommand = new Command(async () => await Logout());
            RefreshCommand = new Command(async () => await LoadUserDataFromApi());
            FirstProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///firtsprofile"));

            // 🔹 Automatically update username when modified in ProfilePage
            MessagingCenter.Subscribe<ProfileViewModel, string>(this, "UserUpdated", async (sender, newUserName) =>
            {
                Console.WriteLine($"🔄 Received new username from Profile: {newUserName}");
                UserName = newUserName;
            });
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

        private async Task LoadNotifications()
        {
            var notificationsFromApi = await _apiService.GetNotificationsAsync();

            Console.WriteLine($"📥 Notificaciones recibidas de la API: {notificationsFromApi?.Count ?? 0}");

            Notifications.Clear();

            if (notificationsFromApi != null)
            {
                var validNotifications = notificationsFromApi
                    .Where(n => !string.IsNullOrWhiteSpace(n.Title)) // Filtra las vacías
                    .ToList();

                foreach (var notification in validNotifications)
                {
                    Notifications.Add(notification);
                }

                Console.WriteLine($"📋 Notificaciones válidas después del filtrado: {Notifications.Count}");
            }

            // Actualiza HasNotifications correctamente
            HasNotifications = Notifications.Count > 0;

            Console.WriteLine($"📢 HasNotifications actualizado a: {HasNotifications}");
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

        /* private async Task LoadNotifications()
         {
             var notificationsFromApi = await _apiService.GetNotificationsAsync();

             Notifications.Clear();

             if (notificationsFromApi != null && notificationsFromApi.Any())
             {
                 foreach (var notification in notificationsFromApi)
                 {
                     Notifications.Add(notification);
                 }
             }

             OnPropertyChanged(nameof(HasNotifications));
         }*/


/*public class HomePageViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private string _userName;
        private int _userId;

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

        public string WelcomeMessage => $"Bienvenido, {UserName}";
        public ObservableCollection<Notification> Notifications { get; set; } = new();
        public ICommand HomeCommand { get; }
        public ICommand ProfileCommand { get; }
        public ICommand OpenMenuCommand { get; }
        public ICommand CourseCommand { get; }

        public bool HasNotifications => Notifications.Count > 0;

        public HomePageViewModel()
        {
            _apiService = new ApiService();
            Task.Run(async () => await LoadUserData());
            Task.Run(async () => await LoadNotifications());

            HomeCommand = new Command(async () => await Shell.Current.GoToAsync("///homepage"));
            ProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///profile"));
            OpenMenuCommand = new Command(async () => await Shell.Current.GoToAsync("///menu"));
            CourseCommand = new Command(async () => await Shell.Current.GoToAsync("///courses"));
        }

        private async Task LoadUserData()
        {
            var storedUserId = await SecureStorage.GetAsync("user_id");
            var storedUserName = await SecureStorage.GetAsync("user_name");

            if (!string.IsNullOrEmpty(storedUserId) && int.TryParse(storedUserId, out _userId))
            {
                Console.WriteLine($"✅ Usuario autenticado: {_userId}");
            }
            else
            {
                Console.WriteLine("❌ No se encontró el ID del usuario.");
            }

            UserName = !string.IsNullOrEmpty(storedUserName) ? storedUserName : "Usuario";
        }

        private async Task LoadNotifications()
        {
            var notificationsFromApi = await _apiService.GetNotificationsAsync();

            Notifications.Clear();

            if (notificationsFromApi != null && notificationsFromApi.Any())
            {
                foreach (var notification in notificationsFromApi)
                {
                    Notifications.Add(notification);
                }
            }

            OnPropertyChanged(nameof(HasNotifications));
        }
    }
}*/




