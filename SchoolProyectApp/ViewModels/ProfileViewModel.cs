﻿using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Controls;
using SchoolProyectApp.Models;
using SchoolProyectApp.Services;
using System.Threading.Tasks;

namespace SchoolProyectApp.ViewModels
{

    public class ProfileViewModel : BaseViewModel
    {

        public ICommand HomeCommand { get; }
        public ICommand ProfileCommand { get; }
        public ICommand OpenMenuCommand { get; }

        private readonly ApiService _apiService;

        private string _userName = string.Empty;
        private string _email = string.Empty;
        private string _password = string.Empty;
        private bool _isBusy;
        private string _message = string.Empty;
        private int _roleID;

        public string UserName
        {
            get => _userName;
            set { _userName = value; OnPropertyChanged(nameof(UserName)); }
        }

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

        public int RoleID
        {
            get => _roleID;
            set { _roleID = value; OnPropertyChanged(nameof(RoleID)); }
        }

        public ICommand UpdateUserCommand { get; }
        public ICommand LoadUserCommand { get; }

        public ProfileViewModel()
        {
            _apiService = new ApiService();

            UpdateUserCommand = new Command(async () => await UpdateUserAsync());
            LoadUserCommand = new Command(async () => await LoadUserDataAsync());

            _ = LoadUserDataAsync();

            HomeCommand = new Command(async () => await Shell.Current.GoToAsync("///homepage"));
            ProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///profile"));
            OpenMenuCommand = new Command(async () => await Shell.Current.GoToAsync("///menu"));
        }


        private async Task LoadUserDataAsync()
        {
            IsBusy = true;
            Console.WriteLine(" Cargando datos del usuario...");

            //  Recuperar el UserID de SecureStorage
            var userIdString = await SecureStorage.GetAsync("user_id");
            Console.WriteLine($"🔹 user_id obtenido de SecureStorage: {userIdString}");

            if (string.IsNullOrEmpty(userIdString) || userIdString == "0")
            {
                Console.WriteLine(" No se encontró el ID de usuario en SecureStorage o es inválido.");
                Message = " Error cargando datos del usuario.";
                IsBusy = false;
                return;
            }

            Console.WriteLine($"✔ UserID recuperado de SecureStorage en ProfileViewModel: {userIdString}");

            //  Intenta convertirlo a número
            if (!int.TryParse(userIdString, out int userId))
            {
                Console.WriteLine(" Error: No se pudo convertir el UserID a número.");
                Message = " Error cargando datos del usuario.";
                IsBusy = false;
                return;
            }

            Console.WriteLine($" Solicitando datos del usuario con ID: {userId}");

            //  Llamar a la API para obtener los datos del usuario
            var user = await _apiService.GetUserDetailsAsync(userId);

            if (user != null)
            {
                UserName = user.UserName;
                Email = user.Email;
                RoleID = user.RoleID;
                Message = "✅ Datos cargados correctamente.";
            }
            else
            {
                Message = "❌ Error cargando datos del usuario.";
            }

            IsBusy = false;
        }


        /*private async Task LoadUserDataAsync()
        {
            IsBusy = true;
            Console.WriteLine("Cargando datos del usuario...");

            // Obtener el UserID almacenado
            var userIdString = await SecureStorage.GetAsync("user_id");

            if (string.IsNullOrEmpty(userIdString))
            {
                Console.WriteLine("No se encontró el ID de usuario en SecureStorage.");
                Message = "Error cargando datos del usuario.";
                IsBusy = false;
                return;
            }

            Console.WriteLine($"UserID recuperado de SecureStorage: {userIdString}");

            // Llamar a la API para obtener los datos del usuario
            var user = await _apiService.GetUserDetailsAsync(userIdString);

            if (user != null)
            {
                UserName = user.UserName;
                Email = user.Email;
                RoleID = user.RoleID;

                Message = "Datos cargados correctamente.";
            }
            else
            {
                Message = "Error cargando datos del usuario.";
            }

            IsBusy = false;
        }*/

        //Update
        private async Task UpdateUserAsync()
        {
            if (string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(Email))
            {
                Message = "Nombre y email son obligatorios.";
                return;
            }

            IsBusy = true;
            Console.WriteLine("Actualizando datos del usuario...");

            var userID = await SecureStorage.GetAsync("user_id");
            var roleID = await SecureStorage.GetAsync("user_role");

            if (string.IsNullOrEmpty(userID) || string.IsNullOrEmpty(roleID))
            {
                Console.WriteLine("No se encontró el ID de usuario o el rol en SecureStorage.");
                Message = "No se pudo obtener el ID de usuario o el rol.";
                IsBusy = false;
                return;
            }

            Console.WriteLine($"Enviando datos: UserID = {userID}, RoleID = {roleID}");

            var user = new User
            {
                UserID = int.Parse(userID),
                UserName = UserName,
                Email = Email,
                Password = Password, // Se enviará como PasswordHash
                RoleID = int.Parse(roleID) // Se envía el RoleID
            };

            bool success = await _apiService.UpdateUserAsync(user);

            if (success)
            {
                Message = "Perfil actualizado correctamente.";
                await SecureStorage.SetAsync("user_email", Email);
                await SecureStorage.SetAsync("user_name", UserName);
            }
            else
            {
                Message = "Error al actualizar el perfil.";
            }

            IsBusy = false;
        }
    }


}
