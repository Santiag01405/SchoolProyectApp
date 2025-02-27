using System.ComponentModel;
using System.Windows.Input;
using SchoolProyectApp.Models;
using SchoolProyectApp.Services;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Controls;

namespace SchoolProyectApp.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _message = string.Empty;
        private bool _isBusy;

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

        public ICommand LoginCommand { get; }
        public ICommand NavigateToRegisterCommand { get; }

        public LoginViewModel()
        {
            _apiService = new ApiService();
            LoginCommand = new Command(async () => await LoginAsync());
            NavigateToRegisterCommand = new Command(async () => await Shell.Current.GoToAsync("//register")); // 🔹 Se corrigió la ruta
        }


        private async Task LoginAsync()
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                Message = "Email y contraseña requeridos";
                return;
            }

            IsBusy = true;
            var user = new User { Email = Email, Password = Password };
            var authResponse = await _apiService.LoginAsync(user);

            if (authResponse != null && !string.IsNullOrEmpty(authResponse.Token))
            {
                await SecureStorage.SetAsync("auth_token", authResponse.Token);
                Message = "Login exitoso";

                // 🔹 Solución: Primero ir a login y luego a HomePage
                await Shell.Current.GoToAsync("//login"); // 🔹 Limpia la pila
                await Shell.Current.GoToAsync("HomePage"); // 🔹 Luego navega a HomePage
            }
            else
            {
                Message = "Error en el login";
            }

            IsBusy = false;
        }

        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}




/*using System.ComponentModel;
using System.Windows.Input;
using SchoolProyectApp.Models;
using SchoolProyectApp.Services;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Controls;

namespace SchoolProyectApp.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _message = string.Empty;
        private bool _isBusy;

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

        public ICommand LoginCommand { get; }

        public LoginViewModel()
        {
            _apiService = new ApiService();
            LoginCommand = new Command(async () => await LoginAsync());
        }

        private async Task LoginAsync()
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                Message = "Email y contraseña requeridos";
                return;
            }

            IsBusy = true;
            var user = new User { Email = Email, Password = Password };
            var authResponse = await _apiService.LoginAsync(user);

            if (authResponse != null && !string.IsNullOrEmpty(authResponse.Token))
            {
                await SecureStorage.SetAsync("auth_token", authResponse.Token);
                Message = "Login exitoso";

                // 🔹 Redirigir a HomePage después del login
                await Shell.Current.GoToAsync("//homepage");
            }
            else
            {
                Message = "Error en el login";
            }

            IsBusy = false;
        }

        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}*/




