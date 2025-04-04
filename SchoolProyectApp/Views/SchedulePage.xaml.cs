﻿using Microsoft.Maui.Controls;
using SchoolProyectApp.ViewModels;
using SchoolProyectApp.Services;

namespace SchoolProyectApp.Views
{
    public partial class SchedulePage : ContentPage
    {
        private ScheduleViewModel _viewModel;

        public SchedulePage()
        {
            InitializeComponent();
            _viewModel = new ScheduleViewModel();
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadWeeklySchedule(); 
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

