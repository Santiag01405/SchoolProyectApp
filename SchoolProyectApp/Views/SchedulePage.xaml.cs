using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using SchoolProyectApp.Models;
using SchoolProyectApp.Services;

namespace SchoolApp
{
    public partial class SchedulePage : ContentPage
    {
        private readonly ApiService _apiService = new ApiService();
        private List<Enrollment> _allEnrollments = new();
        private int _userId;

        public SchedulePage(int userId)
        {
            InitializeComponent();
            _userId = userId;
            LoadData();
        }

        private async void LoadData()
        {
            try
            {
                var enrollments = await _apiService.GetUserWeeklySchedule(_userId);
                _allEnrollments = enrollments?.ToList() ?? new List<Enrollment>();

                int today = (int)DateTime.Today.DayOfWeek;
                DayPicker.SelectedIndex = today;
                FilterSchedule(today);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "No se pudieron cargar los datos: " + ex.Message, "OK");
            }
        }

        private void FilterSchedule(int day)
        {
            if (ScheduleListView == null || NoDataLabel == null) return;

            var filtered = _allEnrollments.Where(e => e.DayOfWeek == day).ToList();
            ScheduleListView.ItemsSource = filtered.Select(e => $"{e.CourseName} - {GetDayName(e.DayOfWeek)}").ToList();
            NoDataLabel.IsVisible = !filtered.Any();
        }

        private void DayPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (DayPicker == null || DayPicker.SelectedIndex < 0) return;
            FilterSchedule(DayPicker.SelectedIndex);
        }

        private string GetDayName(int day)
        {
            return day switch
            {
                0 => "Domingo",
                1 => "Lunes",
                2 => "Martes",
                3 => "Miércoles",
                4 => "Jueves",
                5 => "Viernes",
                6 => "Sábado",
                _ => "Día desconocido"
            };
        }
    }
}

//ANTERIOR INTENTO
/*using Microsoft.Maui.Controls;
using SchoolProyectApp.ViewModels;

namespace SchoolProyectApp.Views
{
    public partial class SchedulePage : ContentPage
    {
        [QueryProperty(nameof(UserId), "userId")]
        public partial class MainPage : ContentPage
        {
            private int _userId;
            public int UserId
            {
                get => _userId;
                set
                {
                    _userId = value;
                    BindingContext = new ScheduleViewModel(_userId);
                }
            }

            public MainPage()
            {
                InitializeComponent();
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
    }
}*/

