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
    public class LoginViewModel : BaseViewModel
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
        public ICommand ResetFieldsCommand { get; }

        public LoginViewModel()
        {
            _apiService = new ApiService();
            LoginCommand = new Command(async () => await LoginAsync());
            NavigateToRegisterCommand = new Command(async () => await Shell.Current.GoToAsync("//register"));
            ResetFieldsCommand = new Command(() => ResetFields());
        }

        public void ResetFields()
        {
            Email = string.Empty;
            Password = string.Empty;
            Message = string.Empty;
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

                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(authResponse.Token);
                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value;

                if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int userId))
                {
                    await SecureStorage.SetAsync("user_id", userId.ToString());
                    Console.WriteLine($"✔ UserID guardado: {userId}");

                    var userDetails = await _apiService.GetUserDetailsAsync(userId);
                    if (userDetails != null)
                    {
                        Console.WriteLine($"✔ Usuario cargado: {userDetails.UserName}");
                        await SecureStorage.SetAsync("user_role", userDetails.RoleID.ToString());

                        // ✅ Guardar IDs para usarlos en las demás vistas
                        if (userDetails.SchoolID > 0)
                            await SecureStorage.SetAsync("school_id", userDetails.SchoolID.ToString());

                        if (userDetails.ClassroomID.HasValue)
                            await SecureStorage.SetAsync("classroom_id", userDetails.ClassroomID.Value.ToString());

                        if (userDetails.School != null)
                            await SecureStorage.SetAsync("school_name", userDetails.School.Name);
                    }

                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await Shell.Current.GoToAsync("//homepage");
                    });

                    Message = "Login exitoso";
                }
            }
            else
            {
                Message = "Error en el login";
            }

            IsBusy = false;
        }
    }
}


/*using System.ComponentModel;
using System.Windows.Input;
using SchoolProyectApp.Models;
using SchoolProyectApp.Services;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Controls;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace SchoolProyectApp.ViewModels
{
    public class LoginViewModel : BaseViewModel
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
        public ICommand ResetFieldsCommand { get; }


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
            ResetFieldsCommand = new Command(() => ResetFields());
        }

        /*private async Task LoginAsync()
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

                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(authResponse.Token);
                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value;

                if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int userId))
                {
                    await SecureStorage.SetAsync("user_id", userId.ToString());
                    Console.WriteLine($"✔ UserID guardado: {userId}");

                    var userDetails = await _apiService.GetUserDetailsAsync(userId);
                    if (userDetails != null)
                    {
                        Console.WriteLine($"✔ Usuario cargado: {userDetails.UserName}");
                        await SecureStorage.SetAsync("user_role", userDetails.RoleID.ToString());
                    }

                    // 🔹 Redirigir correctamente después del login
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await Shell.Current.GoToAsync("//homepage");
                    });

                    Message = "Login exitoso";
                }
            }
            else
            {
                Message = "Error en el login";
            }

            IsBusy = false;
        }
public void ResetFields()
        {
            Email = string.Empty;
            Password = string.Empty;
            Message = string.Empty;
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

                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(authResponse.Token);
                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value;

                if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int userId))
                {
                    await SecureStorage.SetAsync("user_id", userId.ToString());
                    Console.WriteLine($"✔ UserID guardado: {userId}");

                    var userDetails = await _apiService.GetUserDetailsAsync(userId);
                    if (userDetails != null)
                    {
                        Console.WriteLine($"✔ Usuario cargado: {userDetails.UserName}");
                        await SecureStorage.SetAsync("user_role", userDetails.RoleID.ToString());

                        // ✅ Guardar nombre del colegio
                        if (userDetails.School != null)
                        {
                            await SecureStorage.SetAsync("school_name", userDetails.School.Name);
                        }
                    }

                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await Shell.Current.GoToAsync("//homepage");
                    });

                    Message = "Login exitoso";
                }
            }
            else
            {
                Message = "Error en el login";
            }

            IsBusy = false;
        }



    }


}*/




