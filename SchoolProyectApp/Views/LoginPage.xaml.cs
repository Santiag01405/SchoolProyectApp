using SchoolProyectApp.ViewModels;
using Microsoft.Maui.Controls;

namespace SchoolProyectApp.Views
{
    public partial class LoginPage : ContentPage
    {
        public LoginViewModel _viewModel;
        public LoginPage()
        {
            InitializeComponent(); 
            _viewModel = new LoginViewModel();
            BindingContext = _viewModel;
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.ResetFields();
        }

        private void TogglePasswordLabel_Tapped(object sender, EventArgs e)
        {
            PasswordEntry.IsPassword = !PasswordEntry.IsPassword;


            // Verifica si el Label está en el contexto
            if (TogglePasswordLabel != null)
            {
                // Cambiar el icono entre ojo abierto y cerrado
                TogglePasswordLabel.Text = PasswordEntry.IsPassword ? "\uf06e" : "\uf070"; // fa-eye / fa-eye-slash

            }
        }



        /* private void TogglePasswordButton_Clicked(object sender, EventArgs e)
         {
             PasswordEntry.IsPassword = !PasswordEntry.IsPassword;

             // Cambiar el icono según el estado
             TogglePasswordButton.Text = PasswordEntry.IsPassword ? "👁️" : "🙈";
         }*/

    }
}
