using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using SchoolProyectApp.Models;
using SchoolProyectApp.Services;

namespace SchoolProyectApp.ViewModels
{
    public class AssignGradesViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private int _schoolId; // <-- Añade esta línea

        public ObservableCollection<Course> Courses { get; set; } = new();
        public ObservableCollection<Evaluation> Evaluations { get; set; } = new();
        public ObservableCollection<Student> Students { get; set; } = new();

        private Course _selectedCourse;
        public Course SelectedCourse
        {
            get => _selectedCourse;
            set
            {
                if (_selectedCourse != value)
                {
                    _selectedCourse = value;
                    OnPropertyChanged();
                    LoadEvaluationsCommand.Execute(null);
                }
            }
        }

        private Evaluation _selectedEvaluation;
        public Evaluation SelectedEvaluation
        {
            get => _selectedEvaluation;
            set
            {
                if (_selectedEvaluation != value)
                {
                    _selectedEvaluation = value;
                    OnPropertyChanged();
                    LoadStudentsCommand.Execute(null);
                }
            }
        }

        private Student _selectedStudent;
        public Student SelectedStudent
        {
            get => _selectedStudent;
            set
            {
                _selectedStudent = value;
                OnPropertyChanged();
            }
        }

        private decimal? _gradeValue;
        public decimal? GradeValue
        {
            get => _gradeValue;
            set
            {
                _gradeValue = value;
                OnPropertyChanged();
            }
        }

        private string _comments;
        public string Comments
        {
            get => _comments;
            set
            {
                _comments = value;
                OnPropertyChanged();
            }
        }

        private string _gradeText;                     // 👈 NUEVO
        public string GradeText
        {
            get => _gradeText;
            set { _gradeText = value; OnPropertyChanged(); }
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

        private Color _pageBackgroundColor;
        public Color PageBackgroundColor
        {
            get => _pageBackgroundColor;
            set => SetProperty(ref _pageBackgroundColor, value);
        }

        

        public ICommand LoadCoursesCommand { get; }
        public ICommand LoadEvaluationsCommand { get; }
        public ICommand LoadStudentsCommand { get; }
        public ICommand AssignGradeCommand { get; }
        public ICommand HomeCommand { get; }
        public ICommand FirstProfileCommand { get; }
        public ICommand OpenMenuCommand { get; }



        public AssignGradesViewModel()
        {
            _apiService = new ApiService();
            LoadCoursesCommand = new Command(async () => await LoadCoursesAsync());
            LoadEvaluationsCommand = new Command(async () => await LoadEvaluationsAsync());
            LoadStudentsCommand = new Command(async () => await LoadStudentsAsync());
            AssignGradeCommand = new Command(async () => await AssignGradeAsync());
            HomeCommand = new Command(async () => await Shell.Current.GoToAsync("///homepage"));
            OpenMenuCommand = new Command(async () => await Shell.Current.GoToAsync("///menu"));
            FirstProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///firtsprofile"));

            Task.Run(async () => await InitializeAsync());
        }

        private async Task LoadCoursesAsync()
        {
            var userId = await SecureStorage.GetAsync("user_id");
            var schoolId = await SecureStorage.GetAsync("school_id");

            if (!int.TryParse(userId, out int profId) || !int.TryParse(schoolId, out int schId))
                return;

            var url = $"api/courses/user/{profId}/taught-courses?schoolId={schId}";
            var courses = await _apiService.GetAsync<List<Course>>(url);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Courses.Clear();
                if (courses != null)
                {
                    foreach (var course in courses)
                        Courses.Add(course);
                }
            });
        }


        private async Task LoadEvaluationsAsync()
        {
            if (SelectedCourse == null) return;

            var schoolId = await SecureStorage.GetAsync("school_id");
            var url = $"api/grades/course/{SelectedCourse.CourseID}/evaluations/all?schoolId={schoolId}";
            var evals = await _apiService.GetAsync<List<Evaluation>>(url);
            Evaluations.Clear();

            if (evals != null)
                foreach (var e in evals)
                    Evaluations.Add(e);
        }
        private async Task LoadStudentsAsync()
        {
            if (SelectedEvaluation == null) return;

            var url = $"api/grades/evaluation/{SelectedEvaluation.EvaluationID}/students";
            var response = await _apiService.GetAsync<List<Student>>(url);

            Device.BeginInvokeOnMainThread(() =>
            {
                Students.Clear();
                if (response != null)
                {
                    foreach (var student in response)
                    {
                        Students.Add(student);
                    }
                }
            });
        }


        private async Task AssignGradeAsync()
        {
            if (SelectedStudent == null || SelectedCourse == null || SelectedEvaluation == null)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Selecciona curso, evaluación y estudiante.", "OK");
                return;
            }

            var schoolId = await SecureStorage.GetAsync("school_id");
            if (!int.TryParse(schoolId, out int schId)) return;

            // Validación: al menos uno (numérico, texto) o comentario
            bool hasNumeric = GradeValue.HasValue;
            bool hasTextual = !string.IsNullOrWhiteSpace(GradeText);
            bool hasComment = !string.IsNullOrWhiteSpace(Comments);

            if (!hasNumeric && !hasTextual && !hasComment)
            {
                await Application.Current.MainPage.DisplayAlert("Atención",
                    "Debes ingresar una nota (numérica o cualitativa) o un comentario.", "OK");
                return;
            }

            var grade = new Grade
            {
                UserID = SelectedStudent.UserID,
                CourseID = SelectedCourse.CourseID,
                EvaluationID = SelectedEvaluation.EvaluationID,
                SchoolID = schId,
                GradeValue = hasNumeric ? GradeValue : null,
                GradeText = hasTextual ? GradeText : null,
                Comments = hasComment ? Comments : null
            };

            // Tu ApiService ya tiene PostAsync/AssignGradeAsync; usa el que prefieras.
            bool success = await _apiService.PostAsync("api/grades/assign", grade);

            if (success)
            {
                await Application.Current.MainPage.DisplayAlert("✔", "Calificación asignada correctamente", "OK");
                // Opcional: limpiar inputs
                GradeValue = null;
                GradeText = string.Empty;
                Comments = string.Empty;
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("❌", "Error al asignar calificación", "OK");
            }
        }

        private async Task InitializeAsync()
        {
            var schoolIdStr = await SecureStorage.GetAsync("school_id");
            if (int.TryParse(schoolIdStr, out _schoolId))
            {
                // 🎨 aplicar colores dinámicos
                if (_schoolId == 5)
                {
                    PrimaryColor = Color.FromArgb("#0d4483");
                    SecondaryColor = Color.FromArgb("#0098da");
                    PageBackgroundColor = Colors.White;
                }
                else
                {
                    PrimaryColor = Color.FromArgb("#0C4251");
                    SecondaryColor = Color.FromArgb("#6bbdda");
                    PageBackgroundColor = Colors.White;
                }
            }
            await LoadCoursesAsync();
        }
    }
}
