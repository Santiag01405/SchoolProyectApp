using Microsoft.Maui.Controls;
using SchoolProyectApp.ViewModels;
using System;

namespace SchoolProyectApp.Views
{
    public partial class GradesPage : ContentPage
    {
        private readonly GradesViewModel _viewModel;
        private bool _isMenuVisible = false;

        public GradesPage()
        {
            InitializeComponent();
            _viewModel = BindingContext as GradesViewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (_viewModel != null && (_viewModel.GradesByCourse == null || _viewModel.GradesByCourse.Count == 0))
            {
                await _viewModel.LoadDataAsync(); // ⬅️ Corrección aquí
            }
        }

        private async void AnimateButton(object sender, EventArgs e)
        {
            if (sender is ImageButton button)
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
    }
}