﻿using Microsoft.Maui.Controls;
using SchoolProyectApp.ViewModels;

namespace SchoolProyectApp.Views
{
    public partial class HomePage : ContentPage
    {
        public HomePage()
        {
            InitializeComponent();
            BindingContext = new HomePageViewModel();
        }

        private async void AnimateButton(object sender, EventArgs e)
        {
            if (sender is ImageButton button)
            {
                await button.ScaleTo(0.8, 100, Easing.CubicIn);
                await button.ScaleTo(1, 100, Easing.CubicOut);
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
