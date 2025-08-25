// SchoolProyectApp/ViewModels/PostNurseVisitViewModel.cs
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
    public class PostNurseVisitViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private string _searchQuery;
        private User _selectedStudent;
        private string _reason;
        private string _treatment;

        public ObservableCollection<User> SearchResults { get; set; } = new ObservableCollection<User>();

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (SetProperty(ref _searchQuery, value))
                {
                    OnPropertyChanged(nameof(HasSearchResults));
                }
            }
        }

        public User SelectedStudent
        {
            get => _selectedStudent;
            set
            {
                if (SetProperty(ref _selectedStudent, value))
                {
                    OnPropertyChanged(nameof(CanPostVisit));
                }
            }
        }

        public string Reason
        {
            get => _reason;
            set
            {
                if (SetProperty(ref _reason, value))
                {
                    OnPropertyChanged(nameof(CanPostVisit));
                }
            }
        }

        public string Treatment
        {
            get => _treatment;
            set => SetProperty(ref _treatment, value);
        }

        public bool HasSearchResults => SearchResults?.Any() == true;
        public bool CanPostVisit => SelectedStudent != null && !string.IsNullOrEmpty(Reason);

        public ICommand SearchCommand { get; }
        public ICommand PostVisitCommand { get; }
        public ICommand ResetCommand { get; }
        public ICommand HomeCommand { get; }
        public ICommand ProfileCommand { get; }
        public ICommand OpenMenuCommand { get; }
        public ICommand CourseCommand { get; }
        public ICommand FirstProfileCommand { get; }

        public PostNurseVisitViewModel()
        {
            _apiService = new ApiService();
            SearchCommand = new Command(async () => await SearchStudents());
            PostVisitCommand = new Command(async () => await PostNurseVisitAsync(), () => CanPostVisit);
            HomeCommand = new Command(async () => await Shell.Current.GoToAsync("///homepage"));
            ProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///profile"));
            OpenMenuCommand = new Command(async () => await Shell.Current.GoToAsync("///menu"));
            CourseCommand = new Command(async () => await Shell.Current.GoToAsync("///courses"));
            FirstProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///firtsprofile"));
            ResetCommand = new Command(ResetPage);
        }

        private async Task SearchStudents()
        {
            if (string.IsNullOrEmpty(SearchQuery)) return;

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

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        SearchResults.Clear();
                        if (user != null && user.RoleID == 1)
                        {
                            SearchResults.Add(user);
                        }
                        OnPropertyChanged(nameof(HasSearchResults));
                    });
                }
                else
                {
                    // 🔎 Si no es un número o la búsqueda por cédula falló, usa la búsqueda por nombre
                    var users = await _apiService.SearchUsersAsync(SearchQuery, schoolId);

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        SearchResults.Clear();
                        if (users != null)
                        {
                            foreach (var user in users.Where(u => u.RoleID == 1))
                            {
                                SearchResults.Add(user);
                            }
                        }
                        OnPropertyChanged(nameof(HasSearchResults));
                    });
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task PostNurseVisitAsync()
        {
            if (!CanPostVisit) return;

            // Obtener schoolId y nurseUserId en el momento de la ejecución del comando.
            var nurseUserIdString = await SecureStorage.GetAsync("user_id");
            var schoolIdString = await SecureStorage.GetAsync("school_id");

            if (!int.TryParse(nurseUserIdString, out int nurseUserId) || !int.TryParse(schoolIdString, out int schoolId))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Datos de usuario no válidos.", "OK");
                return;
            }

            IsBusy = true;
            try
            {
                var visitDto = new NurseVisitCreateDto
                {
                    StudentUserID = SelectedStudent.UserID,
                    Reason = Reason,
                    Treatment = Treatment
                };

                var success = await _apiService.PostNurseVisitAsync(nurseUserId, schoolId, visitDto);

                if (success)
                {
                    await Application.Current.MainPage.DisplayAlert("Éxito", "Visita registrada correctamente.", "OK");
                    ResetPage();
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "No se pudo registrar la visita. Verifique los datos e intente nuevamente.", "OK");
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ResetPage()
        {
            SearchQuery = string.Empty;
            SelectedStudent = null;
            Reason = string.Empty;
            Treatment = string.Empty;
            SearchResults.Clear();
            (PostVisitCommand as Command)?.ChangeCanExecute();
            (SearchCommand as Command)?.ChangeCanExecute();
        }
    }
}