using Microsoft.Maui.Controls;
using SchoolProyectApp.ViewModels;

namespace SchoolProyectApp.Views
{
    public partial class SendNotificationPage : ContentPage
    {
        private readonly SendNotificationViewModel _viewModel;

        public SendNotificationPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new SendNotificationViewModel();
        }

        // 🔹 Ejecutar la búsqueda cuando el usuario presione "Enter" en el teclado
        private void SearchEntry_Completed(object sender, System.EventArgs e)
        {
            if (_viewModel.SearchCommand.CanExecute(null))
            {
                _viewModel.SearchCommand.Execute(null);
            }
        }

        // 🔹 Limpiar selección y campos de entrada al salir de la pantalla
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _viewModel.SelectedUser = null;
            _viewModel.Title = string.Empty;
            _viewModel.Content = string.Empty;
        }
        private async void AnimateButton(object sender, EventArgs e)
        {
            if (sender is ImageButton button)
            {
                await button.ScaleTo(0.8, 100, Easing.CubicIn);
                await button.ScaleTo(1, 100, Easing.CubicOut);
            }
        }

        bool _isMenuVisible = false;

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
    }
}
