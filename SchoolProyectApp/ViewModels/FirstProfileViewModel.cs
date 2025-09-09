using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using SchoolProyectApp.Services;
using SchoolProyectApp.Models;
using Microsoft.Maui.Storage;

namespace SchoolProyectApp.ViewModels
{
    public class FirstProfileViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private string _userName;
        private string _email;
        private string _role;
        private int _roleId;
        private string _cedula; // 👈 NUEVA PROPIEDAD

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

        public string UserName
        {
            get => _userName;
            set
            {
                _userName = value;
                OnPropertyChanged();
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged();
            }
        }

        public string Cedula // 👈 NUEVA PROPIEDAD
        {
            get => _cedula;
            set
            {
                _cedula = value;
                OnPropertyChanged();
            }
        }

        public string Role
        {
            get => _role;
            set
            {
                _role = value;
                OnPropertyChanged();
            }
        }
        
        public ICommand NavigateToProfileCommand { get; }
        public ICommand HomeCommand { get; }
        public ICommand ProfileCommand { get; }
        public ICommand OpenMenuCommand { get; }
        public ICommand CourseCommand { get; }
        public ICommand FirstProfileCommand { get; }

        public FirstProfileViewModel()
        {
            _apiService = new ApiService();
            NavigateToProfileCommand = new Command(async () => await GoToProfilePage());
            Task.Run(async () => await LoadUserData());

            HomeCommand = new Command(async () => await Shell.Current.GoToAsync("///homepage"));
            OpenMenuCommand = new Command(async () => await Shell.Current.GoToAsync("///menu"));
            CourseCommand = new Command(async () => await Shell.Current.GoToAsync("///courses"));
            FirstProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///firtsprofile"));

            _apiService = new ApiService();
            Task.Run(async () => await LoadUserData());
        }

        private async Task LoadUserData()
        {
            var storedUserId = await SecureStorage.GetAsync("user_id");

            if (!string.IsNullOrEmpty(storedUserId) && int.TryParse(storedUserId, out int userId))
            {
                var user = await _apiService.GetUserDetailsAsync(userId);

                if (user != null)
                {
                    UserName = user.UserName;
                    Email = user.Email;
                    Role = user.RoleID == 1 ? "Estudiante" : user.RoleID == 2 ? "Profesor" : user.RoleID == 3 ? "Padre" : user.RoleID == 4 ? "Psicólogo" : "Desconocido";
                    Cedula = user.Cedula; // 👈 ASIGNACIÓN DE LA CÉDULA
                    RoleID = user.RoleID;
                    OnPropertyChanged(nameof(RoleID));
                    OnPropertyChanged(nameof(IsProfessor));
                    OnPropertyChanged(nameof(IsStudent));
                    OnPropertyChanged(nameof(IsParent));
                }
                else
                {
                    RoleID = 0;
                }
            }
        }

        private async Task GoToProfilePage()
        {
            await Shell.Current.GoToAsync("///profile");
        }
    }
}