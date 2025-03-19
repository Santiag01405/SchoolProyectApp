using Microsoft.Maui.Controls;
using SchoolProyectApp.ViewModels;

namespace SchoolProyectApp.Views
{
    public partial class ProfilePage : ContentPage
    {
        public ProfileViewModel _viewModel;
        public ProfilePage()
        {
            InitializeComponent();
            _viewModel = new ProfileViewModel();
            BindingContext = _viewModel;
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.ResetFields();
        }
        private async void AnimateButton(object sender, EventArgs e)
        {
            if (sender is ImageButton button)
            {
                await button.ScaleTo(0.8, 100, Easing.CubicIn);
                await button.ScaleTo(1, 100, Easing.CubicOut);
            }
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
    }
}