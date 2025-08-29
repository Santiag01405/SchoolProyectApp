// SchoolProyectApp/ViewModels/CreateActivityViewModel.cs
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using SchoolProyectApp.Models;
using SchoolProyectApp.Services;

namespace SchoolProyectApp.ViewModels
{
    public class CreateActivityViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private string _activityName;
        private string _description;
        private string _selectedDay;
        private int _currentUserId;
        private int _currentSchoolId;

        public string ActivityName
        {
            get => _activityName;
            set => SetProperty(ref _activityName, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public string SelectedDay
        {
            get => _selectedDay;
            set => SetProperty(ref _selectedDay, value);
        }

        // Propiedades de colores
        private Color _primaryColor;
        public Color PrimaryColor
        {
            get => _primaryColor;
            set => SetProperty(ref _primaryColor, value);
        }

        private Color _secondaryColor;
        public Color SecondaryColor
        {
            get => _secondaryColor;
            set => SetProperty(ref _secondaryColor, value);
        }

        private Color _textColor;
        public Color TextColor
        {
            get => _textColor;
            set => SetProperty(ref _textColor, value);
        }

        private Color _pageBackgroundColor;
        public Color PageBackgroundColor
        {
            get => _pageBackgroundColor;
            set => SetProperty(ref _pageBackgroundColor, value);
        }

        public List<string> DaysOfWeek => new List<string> { "Lunes", "Martes", "Miércoles", "Jueves", "Viernes" };

        public ICommand CreateActivityCommand { get; }
        public ICommand HomeCommand { get; }
        public ICommand FirstProfileCommand { get; }
        public ICommand OpenMenuCommand { get; }

        public CreateActivityViewModel()
        {
            _apiService = new ApiService();
            CreateActivityCommand = new Command(async () => await CreateActivityAsync());
            _ = LoadUserDataAsync();
            HomeCommand = new Command(async () => await Shell.Current.GoToAsync("///homepage"));
            OpenMenuCommand = new Command(async () => await Shell.Current.GoToAsync("///menu"));
            FirstProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///firtsprofile"));
        }

        private async Task LoadUserDataAsync()
        {
            var userIdString = await SecureStorage.GetAsync("user_id");
            var schoolIdString = await SecureStorage.GetAsync("school_id");

            if (!string.IsNullOrEmpty(userIdString) && int.TryParse(userIdString, out int userId))
            {
                _currentUserId = userId;
            }

            if (!string.IsNullOrEmpty(schoolIdString) && int.TryParse(schoolIdString, out int schoolId))
            {
                _currentSchoolId = schoolId;
            }

            if (_currentSchoolId == 5)
            {
                PrimaryColor = Color.FromArgb("#0d4483");
                SecondaryColor = Color.FromArgb("#0098da");
                TextColor = Colors.White;
                PageBackgroundColor = Color.FromArgb("#f0f2f5");
            }
            else
            {
                PrimaryColor = Color.FromArgb("#0C4251");
                SecondaryColor = Colors.Blue;
                TextColor = Colors.White;
                PageBackgroundColor = Colors.White;
            }
        }

        private async Task CreateActivityAsync()
        {
            if (_currentUserId == 0 || _currentSchoolId == 0)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No se pudo obtener la información del usuario o escuela.", "OK");
                return;
            }

            IsBusy = true;
            try
            {
                var dayAsInt = DaysOfWeek.IndexOf(SelectedDay) + 1;
                var newActivity = new ExtracurricularActivity
                {
                    Name = ActivityName,
                    Description = Description,
                    UserID = _currentUserId,
                    DayOfWeek = dayAsInt,
                    SchoolID = _currentSchoolId
                };

                var success = await _apiService.CreateExtracurricularActivityAsync(newActivity);

                if (success)
                {
                    await Application.Current.MainPage.DisplayAlert("Éxito", "Actividad creada correctamente.", "OK");
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "No se pudo crear la actividad.", "OK");
                }
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}