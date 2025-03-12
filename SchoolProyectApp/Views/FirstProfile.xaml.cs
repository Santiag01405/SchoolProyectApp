using Microsoft.Maui.Controls;
using SchoolProyectApp.ViewModels;

namespace SchoolProyectApp.Views
{
    public partial class FirstProfile : ContentPage
    {
        public FirstProfile()
        {
            InitializeComponent();
            BindingContext = new FirstProfileViewModel();
        }
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

