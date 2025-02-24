using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using SchoolProyectApp.Models;
using SchoolProyectApp.Services;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Controls;

namespace SchoolProyectApp.ViewModels
{
    public class RegisterViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _message = string.Empty;
        private bool _isBusy;
        private string _selectedRole = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(nameof(Email)); }
        }

        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(nameof(Password)); }
        }

        public string Message
        {
            get => _message;
            set { _message = value; OnPropertyChanged(nameof(Message)); }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(nameof(IsBusy)); }
        }

        public ObservableCollection<string> Roles { get; } = new ObservableCollection<string>
        {
            "Student", "Parent", "Teacher"
        };

        public string SelectedRole
        {
            get => _selectedRole;
            set { _selectedRole = value; OnPropertyChanged(nameof(SelectedRole)); }
        }

        public ICommand RegisterCommand { get; }
        public ICommand GoToLoginCommand { get; }

        public RegisterViewModel()
        {
            _apiService = new ApiService();
            RegisterCommand = new Command(async () => await RegisterAsync());
            GoToLoginCommand = new Command(async () => await Shell.Current.GoToAsync("//login"));
        }

        private async Task RegisterAsync()
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(SelectedRole))
            {
                Message = "Todos los campos son obligatorios";
                return;
            }

            IsBusy = true;

            int roleID = SelectedRole switch
            {
                "Student" => 1,
                "Parent" => 2,
                "Teacher" => 3,
                _ => 0
            };

            if (roleID == 0)
            {
                Message = "Rol no válido";
                IsBusy = false;
                return;
            }

            var user = new User { Email = Email, Password = Password, RoleID = roleID };
            var authResponse = await _apiService.RegisterAsync(user);

            if (authResponse != null && !string.IsNullOrEmpty(authResponse.Token))
            {
                await SecureStorage.SetAsync("auth_token", authResponse.Token);
                Message = "Registro exitoso. Redirigiendo...";
                await Shell.Current.GoToAsync("//login");
            }
            else
            {
                Message = "Error en el registro";
            }

            IsBusy = false;
        }

        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}