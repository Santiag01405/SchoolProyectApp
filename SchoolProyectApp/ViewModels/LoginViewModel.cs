using System.ComponentModel;
using System.Windows.Input;
using SchoolProyectApp.Models;
using SchoolProyectApp.Services;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Controls;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Microsoft.Maui.Networking;
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

        private Color _messageColor = Colors.Red;
        public Color MessageColor
        {
            get => _messageColor;
            set
            {
                _messageColor = value;
                OnPropertyChanged(nameof(MessageColor));
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
                MessageColor = Colors.Red;
                return;
            }

            // 1) Chequeo rápido de conectividad local antes de mostrar spinner
            var access = Connectivity.Current.NetworkAccess;
            if (access != NetworkAccess.Internet)
            {
                Message = "❌ No hay conexión a Internet. Verifica tu red.";
                MessageColor = Colors.Red;
                Console.WriteLine($"[LoginAsync] No hay Internet (Connectivity): {access}");
                return;
            }

            IsBusy = true;
            try
            {
                var user = new User { Email = Email, Password = Password };
                var authResponse = await _apiService.LoginAsync(user);

                // diagnóstico en consola
                Console.WriteLine($"[LoginAsync] authResponse == null? {authResponse == null}");

                if (authResponse == null)
                {
                    // Internet hay, pero ApiService no devolvió objeto → posible fallo en la API
                    //Se actualizo el codigo para decir Credenciales Invalidas ya que no esta configurado
                    Message = "❌ Credenciales inválidas.";
                    MessageColor = Colors.Red;
                    Console.WriteLine("[LoginAsync] authResponse == null después de llamar a ApiService.");
                }
                else if (!string.IsNullOrEmpty(authResponse.Token))
                {
                    // ✅ Login correcto
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

                        Message = "✔ Login exitoso";
                        MessageColor = Colors.Green;
                    }
                    else
                    {
                        // Token presente pero no se pudo extraer userId
                        Message = "✔ Login (token recibido) — pero no se pudo leer UserID";
                        MessageColor = Colors.Orange;
                        Console.WriteLine("[LoginAsync] Token recibido pero no se pudo extraer UserID del JWT.");
                    }
                }
                else
                {
                    // ApiService devolvió objeto pero sin token → credenciales inválidas
                    Message = "❌ Credenciales inválidas";
                    MessageColor = Colors.Red;
                    Console.WriteLine("[LoginAsync] authResponse sin token. Asumiendo credenciales inválidas.");
                }
            }
            catch (HttpRequestException ex)
            {
                Message = $"Error de conexión: {ex.Message}";
                MessageColor = Colors.Red;
                Console.WriteLine($"[LoginAsync] HttpRequestException: {ex}");
            }
            catch (TaskCanceledException ex)
            {
                Message = $"Tiempo de espera agotado: {ex.Message}";
                MessageColor = Colors.Red;
                Console.WriteLine($"[LoginAsync] TaskCanceledException (timeout): {ex}");
            }
            catch (Exception ex)
            {
                Message = $"Error inesperado: {ex.Message}";
                MessageColor = Colors.Red;
                Console.WriteLine($"[LoginAsync] Exception inesperada: {ex}");
            }
            finally
            {
                IsBusy = false;
            }
        }



    }
}







