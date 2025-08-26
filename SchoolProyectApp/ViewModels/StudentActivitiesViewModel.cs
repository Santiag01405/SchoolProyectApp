// SchoolProyectApp/ViewModels/StudentActivitiesViewModel.cs
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using SchoolProyectApp.Models;
using SchoolProyectApp.Services;

namespace SchoolProyectApp.ViewModels
{
    public class StudentActivitiesViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private ObservableCollection<ExtracurricularActivity> _activities;
        private int _currentUserId;
        private int _currentSchoolId;
        private string _pageTitle;

        public string PageTitle
        {
            get => _pageTitle;
            set => SetProperty(ref _pageTitle, value);
        }

        public ObservableCollection<ExtracurricularActivity> Activities
        {
            get => _activities;
            set => SetProperty(ref _activities, value);
        }

        public ICommand LoadActivitiesCommand { get; }
        public ICommand HomeCommand { get; }
        public ICommand FirstProfileCommand { get; }
        public ICommand OpenMenuCommand { get; }

        public StudentActivitiesViewModel()
        {
            _apiService = new ApiService();
            LoadActivitiesCommand = new Command(async () => await LoadActivitiesAsync());
            _ = LoadUserDataAndActivitiesAsync();
            HomeCommand = new Command(async () => await Shell.Current.GoToAsync("///homepage"));
            OpenMenuCommand = new Command(async () => await Shell.Current.GoToAsync("///menu"));
            FirstProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///firtsprofile"));
        }

        private async Task LoadUserDataAndActivitiesAsync()
        {
            IsBusy = true;
            try
            {
                await LoadUserDataAsync();
                await SetPageTitleAsync();
                await LoadActivitiesAsync();
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task SetPageTitleAsync()
        {
            var user = await _apiService.GetUserDetailsAsync(_currentUserId);
            if (user != null)
            {
                PageTitle = $"{user.UserName}";
            }
            else
            {
                PageTitle = "Actividades Extracurriculares";
            }
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
        }

        private async Task LoadActivitiesAsync()
        {
            if (_currentUserId == 0)
            {
                Message = "No se pudo obtener el ID del usuario.";
                return;
            }

            IsBusy = true;
            try
            {
                var enrolledActivities = await _apiService.GetStudentActivitiesAsync(_currentUserId);
                if (enrolledActivities != null)
                {
                    Activities = new ObservableCollection<ExtracurricularActivity>(enrolledActivities);
                    if (enrolledActivities.Count == 0)
                    {
                        Message = "No estás inscrito en ninguna actividad.";
                    }
                }
                else
                {
                    Activities = new ObservableCollection<ExtracurricularActivity>();
                    Message = "No se pudieron cargar las actividades.";
                }
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}