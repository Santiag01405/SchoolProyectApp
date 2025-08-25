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
    public class EnrollStudentViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private ObservableCollection<ExtracurricularActivity> _activities;
        private ExtracurricularActivity _selectedActivity;
        private int _currentSchoolId;
        private ObservableCollection<User> _students;
        private User _selectedStudent;
        private string _searchQuery;

        public ObservableCollection<ExtracurricularActivity> Activities
        {
            get => _activities;
            set => SetProperty(ref _activities, value);
        }

        public ExtracurricularActivity SelectedActivity
        {
            get => _selectedActivity;
            set => SetProperty(ref _selectedActivity, value);
        }

        public ObservableCollection<User> Students
        {
            get => _students;
            set => SetProperty(ref _students, value);
        }

        public User SelectedStudent
        {
            get => _selectedStudent;
            set => SetProperty(ref _selectedStudent, value);
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                SetProperty(ref _searchQuery, value);
                // No necesitas llamar a SearchStudentsAsync aquí, el comando del SearchBar lo hará.
            }
        }

        public ICommand EnrollStudentCommand { get; }
        public ICommand SearchStudentsCommand { get; }
        public ICommand HomeCommand { get; }
        public ICommand FirstProfileCommand { get; }
        public ICommand OpenMenuCommand { get; }

        public EnrollStudentViewModel()
        {
            _apiService = new ApiService();
            EnrollStudentCommand = new Command(async () => await EnrollStudentAsync());
            SearchStudentsCommand = new Command(async () => await SearchStudentsAsync());
            _ = LoadDataAsync();
            HomeCommand = new Command(async () => await Shell.Current.GoToAsync("///homepage"));
            OpenMenuCommand = new Command(async () => await Shell.Current.GoToAsync("///menu"));
            FirstProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///firtsprofile"));
        }

        private async Task LoadDataAsync()
        {
            IsBusy = true;
            try
            {
                await LoadSchoolDataAsync();
                await LoadActivitiesAsync();
                await SearchStudentsAsync(); // Carga inicial para mostrar todos los estudiantes al abrir la página
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadSchoolDataAsync()
        {
            var schoolIdString = await SecureStorage.GetAsync("school_id");
            if (!string.IsNullOrEmpty(schoolIdString) && int.TryParse(schoolIdString, out int schoolId))
            {
                _currentSchoolId = schoolId;
            }
        }

        private async Task LoadActivitiesAsync()
        {
            if (_currentSchoolId == 0)
            {
                return;
            }

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

        private async Task SearchStudentsAsync()
        {
            var schoolIdStr = await SecureStorage.GetAsync("school_id");
            if (!int.TryParse(schoolIdStr, out int schoolId) || schoolId == 0)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No se pudo obtener el SchoolID.", "OK");
                return;
            }

            IsBusy = true;
            try
            {
                if (long.TryParse(SearchQuery, out long cedula))
                {
                    // 🔍 Intenta buscar por cédula si el query es un número
                    var user = await _apiService.GetUserByCedulaAsync(cedula.ToString(), schoolId);

                    if (user != null && user.RoleID == 1)
                    {
                        Students = new ObservableCollection<User>(new List<User> { user });
                    }
                    else
                    {
                        Students = new ObservableCollection<User>();
                    }
                }
                else
                {
                    // 🔎 Si no es un número, o la búsqueda por cédula falló, usa la búsqueda por nombre
                    var users = await _apiService.SearchUsersAsync(SearchQuery, schoolId);

                    if (users != null)
                    {
                        var studentsOnly = users.Where(u => u.RoleID == 1).ToList();
                        Students = new ObservableCollection<User>(studentsOnly);
                    }
                    else
                    {
                        Students = new ObservableCollection<User>();
                    }
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task EnrollStudentAsync()
        {
            if (SelectedActivity == null || SelectedStudent == null)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Debe seleccionar un estudiante y una actividad.", "OK");
                return;
            }

            IsBusy = true;
            try
            {
                var enrollmentDto = new ExtracurricularEnrollmentDto
                {
                    UserID = SelectedStudent.UserID,
                    ActivityID = SelectedActivity.ActivityID,
                    SchoolID = _currentSchoolId
                };

                var success = await _apiService.EnrollStudentInActivityAsync(enrollmentDto);

                if (success)
                {
                    await Application.Current.MainPage.DisplayAlert("Éxito", $"Estudiante {SelectedStudent.UserName} inscrito correctamente.", "OK");
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "No se pudo inscribir al estudiante.", "OK");
                }
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}