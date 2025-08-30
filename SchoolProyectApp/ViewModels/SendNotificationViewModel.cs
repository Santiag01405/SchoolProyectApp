using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using SchoolProyectApp.Models;
using SchoolProyectApp.Services;

namespace SchoolProyectApp.ViewModels
{
    public class SendNotificationViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private string _searchQuery;
        private User _selectedUser;
        private string _title;
        private string _content;
        private bool _isProfessor;
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
        public bool HasSearchResults => SearchResults?.Any() == true;


        public ObservableCollection<User> SearchResults { get; set; } = new ObservableCollection<User>();

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
                OnPropertyChanged(nameof(CanSendNotification));
            }
        }

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSendNotification));
            }
        }

        public string Content
        {
            get => _content;
            set
            {
                _content = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSendNotification));
            }
        }

        // Propiedades de colores
        private Color _primaryColor;
        public Color PrimaryColor
        {
            get => _primaryColor;
            set => SetProperty(ref _primaryColor, value);
        }

        private Color _accentColor;
        public Color AccentColor
        {
            get => _accentColor;
            set => SetProperty(ref _accentColor, value);
        }

        private Color _resetButtonBackgroundColor;
        public Color ResetButtonBackgroundColor
        {
            get => _resetButtonBackgroundColor;
            set => SetProperty(ref _resetButtonBackgroundColor, value);
        }

        private Color _resetButtonTextColor;
        public Color ResetButtonTextColor
        {
            get => _resetButtonTextColor;
            set => SetProperty(ref _resetButtonTextColor, value);
        }

        private Color _pageBackgroundColor;
        public Color PageBackgroundColor
        {
            get => _pageBackgroundColor;
            set => SetProperty(ref _pageBackgroundColor, value);
        }
        public bool CanSendNotification => SelectedUser != null && !string.IsNullOrEmpty(Title) && !string.IsNullOrEmpty(Content);

        public ICommand SearchCommand { get; }
        public ICommand SendCommand { get; }
        public ICommand HomeCommand { get; }
        public ICommand NotificationCommand { get; }
        public ICommand ResetCommand { get; }
        public ICommand FirstProfileCommand { get; }
        public ICommand OpenMenuCommand { get; }

        public SendNotificationViewModel()
        {
            _apiService = new ApiService();
            SearchCommand = new Command(async () => await SearchUsers());
            SendCommand = new Command(async () => await SendNotification(), () => CanSendNotification);
            NotificationCommand = new Command(async () => await Shell.Current.GoToAsync("///notification"));
            HomeCommand = new Command(async () => await Shell.Current.GoToAsync("///homepage"));
            OpenMenuCommand = new Command(async () => await Shell.Current.GoToAsync("///menu"));
            FirstProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///firtsprofile"));

            ResetCommand = new Command(ResetPage);

            Task.Run(async () => await CheckUserRole());

            _apiService = new ApiService();
            Task.Run(async () => await LoadUserData());

        }
        private void ResetPage()
        {
            SearchQuery = string.Empty;
            SelectedUser = null;
            Title = string.Empty;
            Content = string.Empty;
            SearchResults.Clear();

            OnPropertyChanged(nameof(SearchQuery));
            OnPropertyChanged(nameof(SelectedUser));
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(Content));
            OnPropertyChanged(nameof(HasSearchResults));
            OnPropertyChanged(nameof(CanSendNotification)); // si dependes de este
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
                        var schoolId = user.SchoolID;
                        // 🎨 aplicar colores dinámicos
                        if (schoolId == 5)
                        {
                            PrimaryColor = Color.FromArgb("#0d4483");
                            AccentColor = Color.FromArgb("#0098da");
                            ResetButtonBackgroundColor = Colors.LightGray;
                            ResetButtonTextColor = Colors.Black;
                            PageBackgroundColor = Colors.White;
                        }
                        else
                        {
                            PrimaryColor = Color.FromArgb("#0C4251");
                            AccentColor = Color.FromArgb("#f1c864");
                            ResetButtonBackgroundColor = Colors.LightGray;
                            ResetButtonTextColor = Colors.Black;
                            PageBackgroundColor = Colors.White;
                        }

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
        private async Task CheckUserRole()
        {
            var userId = await SecureStorage.GetAsync("user_id");
            if (string.IsNullOrEmpty(userId)) return;

            var user = await _apiService.GetUserDetailsAsync(int.Parse(userId));
            _isProfessor = user.RoleID == 2;

            if (!_isProfessor)
            {
                await Application.Current.MainPage.DisplayAlert("Acceso Denegado", "Solo los profesores pueden enviar notificaciones.", "OK");
                await Shell.Current.GoToAsync("///notification");
            }
        }

        /*private async Task SearchUsers()
        {
            if (string.IsNullOrEmpty(SearchQuery)) return;

            var users = await _apiService.SearchUsersAsync(SearchQuery);
            SearchResults.Clear();
            foreach (var user in users)
            {
                SearchResults.Add(user);
                OnPropertyChanged(nameof(HasSearchResults));
            }
        }*/
        private async Task SearchUsers()
        {
            if (string.IsNullOrEmpty(SearchQuery))
                return;

            var schoolIdStr = await SecureStorage.GetAsync("school_id");
            if (!int.TryParse(schoolIdStr, out int schoolId) || schoolId == 0)
            {
                Console.WriteLine("⚠ No se pudo obtener schoolId para búsqueda.");
                return;
            }

            Console.WriteLine($"🔎 Buscando usuarios con query='{SearchQuery}' y schoolId={schoolId}");

            IEnumerable<User> users = new List<User>();

            // ✅ Nueva lógica de búsqueda: Por cédula si es numérico, por nombre si es texto.
            if (long.TryParse(SearchQuery, out long cedula))
            {
                Console.WriteLine("Buscando por cédula...");
                var userFound = await _apiService.GetUserByCedulaAsync(SearchQuery, schoolId);
                if (userFound != null)
                {
                    users = new List<User> { userFound };
                }
            }
            else
            {
                Console.WriteLine("Buscando por nombre de usuario...");
                users = await _apiService.SearchUsersAsync(SearchQuery, schoolId);
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                SearchResults.Clear();

                if (users != null && users.Any())
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

                OnPropertyChanged(nameof(HasSearchResults));
            });
        }
        private async Task SendNotification()
        {
            if (!CanSendNotification) return;

            // Obtener el school_id desde SecureStorage
            var schoolIdStr = await SecureStorage.GetAsync("school_id");
            if (!int.TryParse(schoolIdStr, out int schoolId) || schoolId == 0)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el SchoolID. Intenta de nuevo.", "OK");
                return;
            }

            var notification = new Notification
            {
                Title = Title,
                Content = Content,
                Date = DateTime.Now,
                UserID = SelectedUser.UserID,
                SchoolID = schoolId   // ✅ Se envía el SchoolID requerido
            };

            var success = await _apiService.SendNotificationAsync(notification);
            if (success)
            {
                await Application.Current.MainPage.DisplayAlert("Éxito", "Notificación enviada correctamente.", "OK");
                await Shell.Current.GoToAsync("///notification");
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Hubo un problema al enviar la notificación.", "OK");
            }
        }

    }
}

