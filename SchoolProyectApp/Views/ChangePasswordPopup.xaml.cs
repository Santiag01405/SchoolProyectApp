using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using SchoolProyectApp.Services;
using SchoolProyectApp.Models;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace SchoolProyectApp.Views
{
    public partial class ChangePasswordPopup : Popup
    {
        private readonly ApiService _apiService = new ApiService();

        public ChangePasswordPopup()
        {
            InitializeComponent();
        }

        private void OnCancelClicked(object sender, EventArgs e) => Close();

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            var currentPassword = CurrentPasswordEntry.Text?.Trim();
            var newPassword = NewPasswordEntry.Text?.Trim();

            if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword))
            {
                MessageLabel.Text = "Ambos campos son obligatorios.";
                return;
            }

            var storedEmail = await SecureStorage.GetAsync("user_email");
            if (string.IsNullOrEmpty(storedEmail))
            {
                MessageLabel.Text = "No se encontró el email del usuario.";
                return;
            }

            // 1. Verificar contraseña actual con el login
            var loginUser = new User { Email = storedEmail, Password = currentPassword };
            var authResponse = await _apiService.LoginAsync(loginUser);

            if (authResponse == null || string.IsNullOrEmpty(authResponse.Token))
            {
                MessageLabel.Text = "Contraseña actual incorrecta.";
                return;
            }

            // 2. Actualizar contraseña
            var userId = await SecureStorage.GetAsync("user_id");
            var roleId = await SecureStorage.GetAsync("user_role");
            var userName = await SecureStorage.GetAsync("user_name");

            if (!int.TryParse(userId, out int uid) || !int.TryParse(roleId, out int rid))
            {
                MessageLabel.Text = "Error al validar ID de usuario.";
                return;
            }

            var updateUser = new User
            {
                UserID = uid,
                RoleID = rid,
                Password = newPassword,
                UserName = userName ?? "",
                Email = storedEmail
            };

            var result = await _apiService.UpdateUserAsync(updateUser);
            if (result)
            {
                await SecureStorage.SetAsync("user_password", newPassword);
                await Toast.Make("Contraseña actualizada correctamente.", ToastDuration.Short).Show();
                Close();
            }
            else
            {
                MessageLabel.Text = "Error al actualizar la contraseña.";
            }
        }
    }
}


/*using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using SchoolProyectApp.Services;
using SchoolProyectApp.Models;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;


namespace SchoolProyectApp.Views
{
    public partial class ChangePasswordPopup : Popup
    {
        private readonly ApiService _apiService = new ApiService();

        public ChangePasswordPopup()
        {
            InitializeComponent();
        }

        private void OnCancelClicked(object sender, EventArgs e) => Close();

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            var newPassword = PasswordEntry.Text;

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                MessageLabel.Text = "La contraseña no puede estar vacía.";
                return;
            }

            var userIdStr = await SecureStorage.GetAsync("user_id");
            var roleStr = await SecureStorage.GetAsync("user_role");

            if (!int.TryParse(userIdStr, out int userId) || !int.TryParse(roleStr, out int roleId))
            {
                MessageLabel.Text = "Error al recuperar ID de usuario.";
                return;
            }

            var storedName = await SecureStorage.GetAsync("user_name");
            var storedEmail = await SecureStorage.GetAsync("user_email");

            var user = new User
            {
                UserID = userId,
                RoleID = roleId,
                Password = newPassword,
                UserName = storedName ?? "",
                Email = storedEmail ?? ""
            };


            var result = await _apiService.UpdateUserAsync(user);

            if (result)
            {
                await SecureStorage.SetAsync("user_password", newPassword);
                await Toast.Make("Contraseña actualizada correctamente.", ToastDuration.Short).Show();
                Close(); // cerrar popup
            }
            else
            {
                MessageLabel.Text = "Error al actualizar la contraseña.";
                await Toast.Make("❌ Ocurrió un error al guardar.", ToastDuration.Short).Show();
            }

        }
    }
}*/
