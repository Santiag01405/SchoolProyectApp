﻿using Microsoft.Maui.Controls;
using SchoolProyectApp.ViewModels;

namespace SchoolProyectApp.Views
{
    public partial class EvaluationsListPage : ContentPage
    {
        private readonly EvaluationsListViewModel _viewModel;

        public EvaluationsListPage()
        {
            InitializeComponent();
            _viewModel = new EvaluationsListViewModel();
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadEvaluations();
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

