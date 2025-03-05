using System.ComponentModel;
using System.Windows.Input;
using SchoolProyectApp.Models;
using SchoolProyectApp.Services;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Controls;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace SchoolProyectApp.ViewModels
{
    public class LoginViewModel : BaseViewModel // 🔹 Ahora hereda de BaseViewModel para usar OnPropertyChanged
    {
        private readonly ApiService _apiService;
        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _message = string.Empty;
        private bool _isBusy;
        private int _roleID;

        public new event PropertyChangedEventHandler? PropertyChanged;

        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged(nameof(Email));
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                OnPropertyChanged(nameof(Message));
            }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged(nameof(IsBusy));
            }
        }

        public int RoleID
        {
            get => _roleID;
            set
            {
                _roleID = value;
                OnPropertyChanged(nameof(RoleID));
            }
        }

        public ICommand LoginCommand { get; }
        public ICommand NavigateToRegisterCommand { get; }

        public LoginViewModel()
        {
            _apiService = new ApiService();
            LoginCommand = new Command(async () => await LoginAsync());
            NavigateToRegisterCommand = new Command(async () => await Shell.Current.GoToAsync("//register"));
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

                // Extraer `UserID` del JWT Token
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(authResponse.Token);

                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value;

                if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int userId))
                {
                    await SecureStorage.SetAsync("user_id", userId.ToString()); // Convertir int a string
                    Console.WriteLine($" UserID extraído y guardado: {userId}");
                }
                else
                {
                    Console.WriteLine(" Error: No se pudo extraer UserID del token.");
                }

                // Guardar UserID y RoleID en SecureStorage si existen en `authResponse`
                await SecureStorage.SetAsync("user_id", authResponse.UserID.ToString() ?? ""); // Manejo seguro de `null`
                await SecureStorage.SetAsync("user_role", authResponse.RoleID.ToString() ?? ""); // Manejo seguro de `null`

                Message = "Login exitoso";
                await Shell.Current.GoToAsync("//homepage"); // Redirigir al homepage
            }
            else
            {
                Message = "Error en el login";
            }

            IsBusy = false;
        }
    }
}

/* private async Task LoginAsync()
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
         // 🔹 Guardar datos en SecureStorage
         await SecureStorage.SetAsync("auth_token", authResponse.Token);
         await SecureStorage.SetAsync("user_id", authResponse.UserID.ToString());
         await SecureStorage.SetAsync("user_role", authResponse.RoleID.ToString());

         RoleID = authResponse.RoleID; // Actualizar RoleID en el ViewModel

         Message = "Login exitoso";

         // 🔹 Redirigir según el rol del usuario
         string route = RoleID switch
         {
             1 => "homepage",  // Student
             2 => "homepage",  // Teacher
             3 => "homepage",  // Parent
             _ => "homepage"   // Por defecto, redirigir al homepage
         };

         await Shell.Current.GoToAsync($"///{route}");
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









