using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using SchoolProyectApp.Models;
using SchoolProyectApp.Services;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using System.Diagnostics;

namespace SchoolProyectApp.ViewModels
{
    [QueryProperty(nameof(StudentId), "studentId")]
    [QueryProperty(nameof(ChildSchoolId), "schoolId")]
    public class GradesViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        // Estado / control de carga
        private int _activeSchoolId;
        private bool _lapsosLoaded;
        private bool _alreadyLoaded;
        private bool _isBusy;

        // Flags para saber cuándo llegaron ambos parámetros en modo hijo
        private bool _studentIdArrived;
        private bool _childSchoolIdArrived;

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        // -------- Query Properties --------
        private int _studentId;
        public int StudentId
        {
            get => _studentId;
            set
            {
                if (SetProperty(ref _studentId, value))
                {
                    _studentIdArrived = true;
                    Debug.WriteLine($"[Grades] Setter StudentId={_studentId}");
                    TryLoadIfReady();
                }
            }
        }

        private int _childSchoolId;
        public int ChildSchoolId
        {
            get => _childSchoolId;
            set
            {
                if (SetProperty(ref _childSchoolId, value))
                {
                    _childSchoolIdArrived = true;
                    Debug.WriteLine($"[Grades] Setter ChildSchoolId={_childSchoolId}");
                    // Como cambió la sede activa, invalidamos cualquier cache previo
                    _lapsosLoaded = false;
                    _alreadyLoaded = false;
                    TryLoadIfReady();
                }
            }
        }

        // -------- UI data --------
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
                        _ = LoadGradesAsync();
                        _ = LoadOverallByLapsoAsync();
                    }
                }
            }
        }

        public ObservableCollection<Lapso> Lapsos { get; } = new ObservableCollection<Lapso>();
        public ObservableCollection<GradesByCourseGroup> GradesByCourse { get; set; } = new();

        // -------- Commands --------
        public ICommand HomeCommand { get; }
        public ICommand FirstProfileCommand { get; }
        public ICommand LoadGradesCommand { get; }

        public GradesViewModel()
        {
            _apiService = new ApiService();
            HomeCommand = new Command(async () => await Shell.Current.GoToAsync("///homepage"));
            FirstProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///firstprofile"));
            LoadGradesCommand = new Command(async () => await LoadDataAsync());
            // 🚫 Nada de cargas automáticas aquí: esperamos TryLoadIfReady()
        }

        /// <summary>
        /// Dispara la carga inicial sólo cuando:
        /// - Modo hijo: StudentId>0 y ChildSchoolId>0 (ambos llegaron)
        /// - Modo propio: StudentId==0 y ChildSchoolId==0
        /// </summary>
        private void TryLoadIfReady()
        {
            if (IsBusy) return;

            // Modo hijo → espera ambos parámetros
            if (StudentId > 0)
            {
                if (!_studentIdArrived || !_childSchoolIdArrived)
                {
                    Debug.WriteLine("[Grades] Esperando parámetros del hijo (studentId y schoolId)...");
                    return;
                }

                if (_alreadyLoaded) return;
                Debug.WriteLine("[Grades] Ready (modo hijo). Cargando con la sede del hijo...");
                _ = LoadDataAsync();
                return;
            }

            // Modo propio → ambos parámetros vacíos
            if (!_alreadyLoaded && StudentId == 0 && ChildSchoolId == 0)
            {
                Debug.WriteLine("[Grades] Ready (modo propio). Cargando con la sede del usuario...");
                _ = LoadDataAsync();
            }
        }

        /// <summary>
        /// Punto de entrada para cargar todo: lapsos y promedios (global).
        /// </summary>
        public async Task LoadDataAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                // Determinar sede activa
                if (StudentId > 0 && ChildSchoolId > 0)
                    _activeSchoolId = ChildSchoolId;
                else
                    _activeSchoolId = int.TryParse(await SecureStorage.GetAsync("school_id"), out var sid) ? sid : 0;

                Debug.WriteLine($"[Grades] LoadDataAsync con activeSchoolId={_activeSchoolId}");

                await LoadLapsosAsync();                 // usa _activeSchoolId
                await LoadStudentOverallAverageAsync();  // usa _activeSchoolId

                _alreadyLoaded = true;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadLapsosAsync()
        {
            if (_activeSchoolId == 0) return;

            // Evita doble carga
            if (_lapsosLoaded) return;

            Debug.WriteLine($"[Grades] GET lapsos para schoolId={_activeSchoolId}");
            var lapsosList = await _apiService.GetLapsosAsync(_activeSchoolId);
            if (lapsosList == null) return;

            var filtered = lapsosList.Where(l => l.SchoolID == _activeSchoolId).ToList();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Lapsos.Clear();
                foreach (var lapso in filtered)
                    Lapsos.Add(lapso);

                if (Lapsos.Any())
                    SelectedLapso = Lapsos.First(); // asegura que las siguientes llamadas usen el lapso de la sede activa
            });

            _lapsosLoaded = true;
        }

        public async Task LoadStudentOverallAverageAsync()
        {
            // Usuario objetivo
            var userIdToLoad = StudentId > 0 ? StudentId.ToString() : await SecureStorage.GetAsync("user_id");
            if (!int.TryParse(userIdToLoad, out var uId)) return;

            if (_activeSchoolId == 0) return;

            Debug.WriteLine($"[Grades] GET overall average userId={uId}, schoolId={_activeSchoolId}");
            var averageDto = await _apiService.GetStudentOverallAverageAsync(uId, _activeSchoolId);
            StudentOverallAverage = averageDto?.OverallAverage.ToString("F2", CultureInfo.InvariantCulture) ?? "N/A";
        }

        public async Task LoadOverallByLapsoAsync()
        {
            if (SelectedLapso == null || _activeSchoolId == 0) return;

            var userIdToLoad = StudentId > 0 ? StudentId.ToString() : await SecureStorage.GetAsync("user_id");
            if (!int.TryParse(userIdToLoad, out int uId)) return;

            Debug.WriteLine($"[Grades] GET overall-by-lapso userId={uId}, lapsoId={SelectedLapso.LapsoID}, schoolId={_activeSchoolId}");
            var overallByLapso = await _apiService.GetOverallByLapsoAsync(uId, SelectedLapso.LapsoID, _activeSchoolId);
            StudentLapsoAverage = overallByLapso?.AverageGrade.ToString("F2", CultureInfo.InvariantCulture) ?? "N/A";
        }

        public async Task LoadGradesAsync()
        {
            if (SelectedLapso == null || _activeSchoolId == 0) return;

            var userIdToLoad = StudentId > 0 ? StudentId.ToString() : await SecureStorage.GetAsync("user_id");
            if (!int.TryParse(userIdToLoad, out int uId)) return;

            Debug.WriteLine($"[Grades] GET grades-by-lapso userId={uId}, lapsoId={SelectedLapso.LapsoID}, schoolId={_activeSchoolId}");
            var response = await _apiService.GetGradesByLapsoAsync(uId, SelectedLapso.LapsoID, _activeSchoolId);

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
                                var dec = grade.GradeValue;
                                var txt = grade.GradeText;
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