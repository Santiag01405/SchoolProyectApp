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

        public bool CanSendNotification => SelectedUser != null && !string.IsNullOrEmpty(Title) && !string.IsNullOrEmpty(Content);

        public ICommand SearchCommand { get; }
        public ICommand SendCommand { get; }
        public ICommand HomeCommand { get; }
        public ICommand NotificationCommand { get; }
        public ICommand ResetCommand { get; }

        public SendNotificationViewModel()
        {
            _apiService = new ApiService();
            SearchCommand = new Command(async () => await SearchUsers());
            SendCommand = new Command(async () => await SendNotification(), () => CanSendNotification);
            HomeCommand = new Command(async () => await Shell.Current.GoToAsync("///homepage"));
            NotificationCommand = new Command(async () => await Shell.Current.GoToAsync("///notification"));

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

        private async Task SearchUsers()
        {
            if (string.IsNullOrEmpty(SearchQuery)) return;

            var users = await _apiService.SearchUsersAsync(SearchQuery);
            SearchResults.Clear();
            foreach (var user in users)
            {
                SearchResults.Add(user);
                OnPropertyChanged(nameof(HasSearchResults));
            }
        }

        private async Task SendNotification()
        {
            if (!CanSendNotification) return;

            var notification = new Notification
            {
                Title = Title,
                Content = Content,
                Date = DateTime.Now,
                UserID = SelectedUser.UserID
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

