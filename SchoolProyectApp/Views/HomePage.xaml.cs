using Microsoft.Maui.Controls;
using SchoolProyectApp.ViewModels;

namespace SchoolProyectApp.Views
{
    public partial class HomePage : ContentPage
    {
        private bool _isMenuVisible = false;

        public HomePage()
        {
            InitializeComponent();
            BindingContext = new HomePageViewModel();
        }

        private async void AnimateButton(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                await button.ScaleTo(0.8, 100, Easing.CubicIn);
                await button.ScaleTo(1, 100, Easing.CubicOut);
            }
        }

        private async void MenuButton_Clicked(object sender, EventArgs e)
        {
            if (_isMenuVisible)
            {
                await SideMenu.TranslateTo(-260, 0, 250, Easing.CubicIn);
                SideMenu.IsVisible = false;
                _isMenuVisible = false;
            }
            else
            {
                SideMenu.IsVisible = true;
                await SideMenu.TranslateTo(0, 0, 250, Easing.CubicOut);
                _isMenuVisible = true;
            }
        }


        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (BindingContext is HomePageViewModel viewModel)
            {
                Console.WriteLine("🔄 HomePage está apareciendo, recargando datos del usuario...");
                viewModel.LoadUserDataFromApi().ConfigureAwait(false);


            }
        }
    }
}
