// SchoolProyectApp/ViewModels/MarkAttendanceViewModel.cs
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using SchoolProyectApp.Models;
using SchoolProyectApp.Services;

namespace SchoolProyectApp.ViewModels
{
    public class MarkAttendanceViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private ObservableCollection<ExtracurricularActivity> _activities;
        private ExtracurricularActivity _selectedActivity;
        private ObservableCollection<StudentAttendanceModel> _students;
        private int _currentUserId;
        private int _currentSchoolId;

        public ObservableCollection<ExtracurricularActivity> Activities
        {
            get => _activities;
            set => SetProperty(ref _activities, value);
        }

        public ExtracurricularActivity SelectedActivity
        {
            get => _selectedActivity;
            set
            {
                if (SetProperty(ref _selectedActivity, value))
                {
                    if (value != null)
                    {
                        _ = LoadStudentsForActivityAsync();
                    }
                }
            }
        }

        public ObservableCollection<StudentAttendanceModel> Students
        {
            get => _students;
            set => SetProperty(ref _students, value);
        }

        public ICommand LoadActivitiesCommand { get; }
        public ICommand SaveAttendanceCommand { get; }
        public ICommand HomeCommand { get; }
        public ICommand FirstProfileCommand { get; }
        public ICommand OpenMenuCommand { get; }

        public MarkAttendanceViewModel()
        {
            _apiService = new ApiService();
            LoadActivitiesCommand = new Command(async () => await LoadActivitiesAsync());
            SaveAttendanceCommand = new Command(async () => await SaveAttendanceAsync());
            _ = LoadUserDataAndActivitiesAsync();
            HomeCommand = new Command(async () => await Shell.Current.GoToAsync("///homepage"));
            OpenMenuCommand = new Command(async () => await Shell.Current.GoToAsync("///menu"));
            FirstProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///firtsprofile"));
        }

        private async Task LoadUserDataAndActivitiesAsync()
        {
            await LoadUserDataAsync();
            await LoadActivitiesAsync();
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
            if (_currentSchoolId == 0)
            {
                Message = "No se pudo obtener el ID de la escuela.";
                return;
            }

            IsBusy = true;
            try
            {
                var result = await _apiService.GetExtracurricularActivitiesAsync(_currentSchoolId);
                if (result != null)
                {
                    Activities = new ObservableCollection<ExtracurricularActivity>(result);
                }
                else
                {
                    Activities = new ObservableCollection<ExtracurricularActivity>();
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadStudentsForActivityAsync()
        {
            IsBusy = true;
            try
            {
                var studentsInActivity = await _apiService.GetStudentsEnrolledInActivityAsync(SelectedActivity.ActivityID, _currentSchoolId);
                if (studentsInActivity != null)
                {
                    Students = new ObservableCollection<StudentAttendanceModel>(
                        studentsInActivity.Select(s => new StudentAttendanceModel { UserID = s.UserID, StudentName = s.StudentName, IsPresent = false })
                    );
                }
                else
                {
                    Students = new ObservableCollection<StudentAttendanceModel>();
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task SaveAttendanceAsync()
        {
            if (SelectedActivity == null || Students == null || _currentUserId == 0 || _currentSchoolId == 0)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Falta información para guardar la asistencia.", "OK");
                return;
            }

            IsBusy = true;
            try
            {
                var attendanceDto = new ExtracurricularAttendanceMarkDto
                {
                    ActivityId = SelectedActivity.ActivityID,
                    RelatedUserId = _currentUserId,
                    SchoolId = _currentSchoolId,
                    StudentAttendance = Students.Select(s => new StudentAttendanceDto
                    {
                        UserId = s.UserID,
                        Status = s.IsPresent ? "Presente" : "Ausente"
                    }).ToList()
                };

                var success = await _apiService.MarkAttendanceAsync(attendanceDto);

                if (success)
                {
                    await Application.Current.MainPage.DisplayAlert("Éxito", "Asistencia guardada correctamente.", "OK");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "No se pudo guardar la asistencia.", "OK");
                }
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}