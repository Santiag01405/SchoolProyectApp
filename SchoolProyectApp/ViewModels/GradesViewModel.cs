using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using SchoolProyectApp.Models;
using SchoolProyectApp.Services;

namespace SchoolProyectApp.ViewModels
{
    public class GradesViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        public ObservableCollection<GradesByCourseGroup> GradesByCourse { get; set; } = new();

        public ICommand LoadGradesCommand { get; }

        public GradesViewModel()
        {
            _apiService = new ApiService();
            LoadGradesCommand = new Command(async () => await LoadGradesAsync());
        }

        public async Task LoadGradesAsync()
        {
            var userId = await SecureStorage.GetAsync("user_id");
            var schoolId = await SecureStorage.GetAsync("school_id");

            if (!int.TryParse(userId, out int uId) || !int.TryParse(schoolId, out int schId))
                return;

            var url = $"api/grades/user/{uId}/grades?schoolId={schId}";
            var response = await _apiService.GetAsync<List<GradeResult>>(url);

            Device.BeginInvokeOnMainThread(() =>
            {
                GradesByCourse.Clear();
                if (response != null)
                {
                    // ✅ Agrupar por Curso
                    var grouped = response
                        .GroupBy(g => g.Curso)
                        .Select(gr => new GradesByCourseGroup
                        {
                            Curso = gr.Key,
                            Calificaciones = gr.ToList()
                        }).ToList();

                    foreach (var group in grouped)
                        GradesByCourse.Add(group);
                }
            });
        }
    }
}


/*using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using SchoolProyectApp.Services;
using SchoolProyectApp.Models;

namespace SchoolProyectApp.ViewModels
{
    public class GradesViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        public ObservableCollection<CourseGradesDisplay> GradesByCourse { get; set; } = new();

        public GradesViewModel()
        {
            _apiService = new ApiService();
            Task.Run(async () => await LoadGradesAsync());
        }

        public async Task LoadGradesAsync()
        {
            var userId = await SecureStorage.GetAsync("user_id");
            var schoolId = await SecureStorage.GetAsync("school_id");

            if (!int.TryParse(userId, out int uId) || !int.TryParse(schoolId, out int sId))
                return;

            var url = $"api/grades/user/{uId}/grades?schoolId={sId}";
            var grades = await _apiService.GetAsync<List<GradeResult>>(url);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                GradesByCourse.Clear();

                if (grades != null)
                {
                    var grouped = grades.GroupBy(g => (string)g.Curso);

                    foreach (var group in grouped)
                    {
                        var courseItem = new CourseGradesDisplay
                        {
                            CourseName = group.Key,
                            Evaluations = group.Select(e => new EvaluationGradeDisplay
                            {
                                Title = (string)e.Evaluacion,
                                GradeValue = e.GradeValue != null ? (decimal?)e.GradeValue : null
                            }).ToList()
                        };

                        GradesByCourse.Add(courseItem);
                    }
                }
            });
        }
    }

    public class CourseGradesDisplay
    {
        public string CourseName { get; set; }
        public List<EvaluationGradeDisplay> Evaluations { get; set; }
    }

    public class EvaluationGradeDisplay
    {
        public string Title { get; set; }
        public decimal? GradeValue { get; set; }

        public string DisplayGrade => GradeValue.HasValue ? GradeValue.Value.ToString("0.00") : "Sin corregido";
        public Color GradeColor => GradeValue.HasValue ? Colors.Green : Colors.Red;
    }
}*/
