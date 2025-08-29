// SchoolProyectApp/ViewModels/GradesViewModel.cs
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using SchoolProyectApp.Models;
using SchoolProyectApp.Services;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Maui.Controls; // Importante para Device.BeginInvokeOnMainThread

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
                    _ = LoadDataAsync();
                }
            }
        }

        private readonly ApiService _apiService;

        private string _studentOverallAverage;
        public string StudentOverallAverage
        {
            get => _studentOverallAverage;
            set => SetProperty(ref _studentOverallAverage, value);
        }

        private string _studentLapsoAverage;
        public string StudentLapsoAverage
        {
            get => _studentLapsoAverage;
            set => SetProperty(ref _studentLapsoAverage, value);
        }

        private Lapso _selectedLapso;
        public Lapso SelectedLapso
        {
            get => _selectedLapso;
            set
            {
                if (SetProperty(ref _selectedLapso, value))
                {
                    if (_selectedLapso != null)
                    {
                        // ➡️ Await en las llamadas para garantizar el orden.
                        // Esto elimina la condición de carrera.
                        _ = LoadGradesAsync();
                        _ = LoadOverallByLapsoAsync();
                    }
                }
            }
        }

        public ObservableCollection<Lapso> Lapsos { get; } = new ObservableCollection<Lapso>();
        public ObservableCollection<GradesByCourseGroup> GradesByCourse { get; set; } = new();

        public ICommand HomeCommand { get; }
        public ICommand FirstProfileCommand { get; }
        public ICommand LoadGradesCommand { get; }

        public GradesViewModel()
        {
            _apiService = new ApiService();
            HomeCommand = new Command(async () => await Shell.Current.GoToAsync("///homepage"));
            FirstProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///firstprofile"));
            LoadGradesCommand = new Command(async () => await LoadDataAsync());
        }

        public async Task LoadDataAsync()
        {
            // ➡️ Se asegura que la carga de lapsos y de promedios se ejecute en orden.
            await LoadLapsosAsync();
            await LoadStudentOverallAverageAsync();
        }

        private async Task LoadLapsosAsync()
        {
            var schoolId = await SecureStorage.GetAsync("school_id");
            if (!int.TryParse(schoolId, out int schId)) return;

            var lapsosList = await _apiService.GetLapsosAsync(schId);
            if (lapsosList != null)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Lapsos.Clear();
                    foreach (var lapso in lapsosList)
                    {
                        Lapsos.Add(lapso);
                    }
                    if (Lapsos.Any())
                    {
                        SelectedLapso = Lapsos.FirstOrDefault();
                    }
                });
            }
        }

        public async Task LoadStudentOverallAverageAsync()
        {
            var userIdToLoad = StudentId > 0 ? StudentId.ToString() : await SecureStorage.GetAsync("user_id");
            var schoolId = await SecureStorage.GetAsync("school_id");

            if (!int.TryParse(userIdToLoad, out int uId) || !int.TryParse(schoolId, out int schId))
                return;

            var averageDto = await _apiService.GetStudentOverallAverageAsync(uId, schId);

            // Se corrige la propiedad para que coincida con la del modelo de datos
            StudentOverallAverage = averageDto?.OverallAverage.ToString("F2", CultureInfo.InvariantCulture) ?? "N/A";
        }

        public async Task LoadOverallByLapsoAsync()
        {
            if (SelectedLapso == null) return;

            var userIdToLoad = StudentId > 0 ? StudentId.ToString() : await SecureStorage.GetAsync("user_id");
            var schoolId = await SecureStorage.GetAsync("school_id");

            if (!int.TryParse(userIdToLoad, out int uId) || !int.TryParse(schoolId, out int schId))
                return;

            // ➡️ Corrección: Llama a un nuevo método de la API que espera un objeto simple, no una lista.
            // Para resolver este error, la API debe devolver un objeto directamente.
            // Asumimos que GetOverallByLapsoAsync ahora retorna un objeto simple.
            var overallByLapso = await _apiService.GetOverallByLapsoAsync(uId, SelectedLapso.LapsoID, schId);

            // ➡️ Accede directamente a la propiedad `AverageGrade` del objeto.
            StudentLapsoAverage = overallByLapso?.AverageGrade.ToString("F2", CultureInfo.InvariantCulture) ?? "N/A";
        }
        public async Task LoadGradesAsync()
        {
            if (SelectedLapso == null) return;

            var userIdToLoad = StudentId > 0 ? StudentId.ToString() : await SecureStorage.GetAsync("user_id");
            var schoolId = await SecureStorage.GetAsync("school_id");
            if (!int.TryParse(userIdToLoad, out int uId) || !int.TryParse(schoolId, out int schId)) return;

            var response = await _apiService.GetGradesByLapsoAsync(uId, SelectedLapso.LapsoID, schId);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                GradesByCourse.Clear();
                if (response != null)
                {
                    var grouped = response
        .Where(g => g.Course != null)
        .GroupBy(g => g.Course.Name)
        .Select(gr => new GradesByCourseGroup
        {
            Curso = gr.Key,
            Calificaciones = gr.Select(grade =>
            {
                var dec = grade.GradeValue;         // double? desde la API con includes
                var txt = grade.GradeText;          // texto cualitativo

                // Convierte double? -> decimal?
                decimal? decNullable = dec.HasValue ? (decimal?)dec.Value : null;

                return new GradeResult
                {
                    GradeID = grade.GradeID,
                    UserID = grade.UserID,
                    Curso = grade.Course?.Name,
                    CourseID = grade.CourseID,
                    Evaluacion = grade.Evaluation?.Title,
                    GradeValue = decNullable,
                    GradeText = string.IsNullOrWhiteSpace(txt) ? null : txt,
                    Comments = grade.Comments,

                    // 👇 Única propiedad para la UI
                    DisplayGrade = decNullable.HasValue
                        ? decNullable.Value.ToString("0.00")
                        : (!string.IsNullOrWhiteSpace(txt) ? txt : "—")
                };
            }).ToList()
        })
        .ToList();



                    foreach (var group in grouped)
                        GradesByCourse.Add(group);
                }
            });
        }
    }

    public class GradesByCourseGroup
    {
        public string Curso { get; set; }
        public List<GradeResult> Calificaciones { get; set; }
    }
}