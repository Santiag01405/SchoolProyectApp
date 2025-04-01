using Microsoft.Maui.Controls;
using SchoolProyectApp.ViewModels;

namespace SchoolProyectApp.Views
{
    public partial class NotificationPage : ContentPage
    {
        private readonly NotificationViewModel _viewModel;

        public NotificationPage()
        {
            InitializeComponent();
            _viewModel = new NotificationViewModel();
            BindingContext = _viewModel;
        }
        /*protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _viewModel.OnDisappearing(); // Llamamos a la limpieza de datos
        }*/
        private async void AnimateButton(object sender, EventArgs e)
        {
            if (sender is ImageButton button)
            {
                await button.ScaleTo(0.8, 100, Easing.CubicIn);
                await button.ScaleTo(1, 100, Easing.CubicOut);
            }
        }
    }
}

