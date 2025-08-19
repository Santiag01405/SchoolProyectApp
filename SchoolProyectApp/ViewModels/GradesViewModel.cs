// SchoolProyectApp/ViewModels/GradesViewModel.cs
// ... (usings existentes)
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using SchoolProyectApp.Models;
using SchoolProyectApp.Services;

namespace SchoolProyectApp.ViewModels
{
    [QueryProperty(nameof(StudentId), "studentId")]
    public class GradesViewModel : BaseViewModel

    {
        private int studentId;
        public int StudentId
        {
            get => studentId;
            set
            {
                if (studentId != value)
                {
                    studentId = value;
                    // Cuando el ID del estudiante llega, recargamos sus datos.
                    _ = LoadDataAsync();
                }
            }
        }

        private readonly ApiService _apiService;

        // Nuevas propiedades
        private string _studentOverallAverage;
        public string StudentOverallAverage
        {
            get => _studentOverallAverage;
            set => SetProperty(ref _studentOverallAverage, value);
        }

        private string _courseAverage;
        public string CourseAverage
        {
            get => _courseAverage;
            set => SetProperty(ref _courseAverage, value);
        }

        public ObservableCollection<GradesByCourseGroup> GradesByCourse { get; set; } = new();

        public ICommand LoadGradesCommand { get; }
        public ICommand HomeCommand { get; }
        public ICommand ProfileCommand { get; }
        public ICommand CourseCommand { get; }
        public ICommand OpenMenuCommand { get; }
        public ICommand FirstProfileCommand { get; }

        public GradesViewModel()
        {
            _apiService = new ApiService();
            LoadGradesCommand = new Command(async () => await LoadGradesAsync());
            HomeCommand = new Command(async () => await Shell.Current.GoToAsync("///homepage"));
            OpenMenuCommand = new Command(async () => await Shell.Current.GoToAsync("///menu"));
            CourseCommand = new Command(async () => await Shell.Current.GoToAsync("///courses"));
            FirstProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///firtsprofile"));

            // Carga las calificaciones y los promedios al inicio
            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            await LoadGradesAsync();
            await LoadStudentOverallAverageAsync();
        }

        // Nuevo método para cargar el promedio general del estudiante
        public async Task LoadStudentOverallAverageAsync()
        {
            var userIdToLoad = StudentId > 0 ? StudentId.ToString() : await SecureStorage.GetAsync("user_id");
            var schoolId = await SecureStorage.GetAsync("school_id");

            if (!int.TryParse(userIdToLoad, out int uId) || !int.TryParse(schoolId, out int schId)) return;

            var averageResult = await _apiService.GetStudentOverallAverageAsync(uId, schId);
            if (averageResult != null)
            {
                StudentOverallAverage = averageResult.OverallAverage.ToString("F2", CultureInfo.InvariantCulture);
            }
            else
            {
                StudentOverallAverage = "N/A";
            }
        }

        // Actualiza tu método LoadGradesAsync para que también cargue el promedio del curso
        public async Task LoadGradesAsync()
        {
            var userIdToLoad = StudentId > 0 ? StudentId.ToString() : await SecureStorage.GetAsync("user_id");
            var schoolId = await SecureStorage.GetAsync("school_id");

            if (!int.TryParse(userIdToLoad, out int uId) || !int.TryParse(schoolId, out int schId))
                return;

            var url = $"api/grades/user/{uId}/grades?schoolId={schId}";
            var response = await _apiService.GetAsync<List<GradeResult>>(url);

            Device.BeginInvokeOnMainThread(async () =>
            {
                GradesByCourse.Clear();
                if (response != null)
                {
                    var grouped = response
                        .GroupBy(g => g.Curso)
                        .Select(gr => new GradesByCourseGroup
                        {
                            Curso = gr.Key,
                            Calificaciones = gr.ToList()
                        }).ToList();

                    foreach (var group in grouped)
                    {
                        GradesByCourse.Add(group);
                        // ✅ Carga el promedio del curso y lo asigna a la propiedad
                        var courseAverage = await _apiService.GetCourseAverageAsync(group.Calificaciones.FirstOrDefault()?.CourseID ?? 0, schId);
                        CourseAverage = courseAverage.ToString("F2", CultureInfo.InvariantCulture);
                    }
                }
            });
        }
    }
}