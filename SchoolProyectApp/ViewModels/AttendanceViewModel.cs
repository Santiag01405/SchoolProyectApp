using System.Collections.ObjectModel;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using SchoolProyectApp.Models;
using SchoolProyectApp.Services;

namespace SchoolProyectApp.ViewModels
{
    public class AttendanceViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private int _schoolId;

        public ObservableCollection<Course> Courses { get; set; } = new();
        public ObservableCollection<StudentViewModel> Students { get; set; } = new();

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
                    if (value != null)
                        LoadStudentsCommand.Execute(null);
                }
            }
        }

        private StudentViewModel _selectedStudent;
        public StudentViewModel SelectedStudent
        {
            get => _selectedStudent;
            set
            {
                _selectedStudent = value;
                OnPropertyChanged();
            }
        }

       
        public ICommand LoadCoursesCommand { get; }
        public ICommand LoadStudentsCommand { get; }
        public ICommand MarkAttendanceCommand { get; }
        public ICommand MarkAbsentCommand { get; }
        public ICommand MarkPresentCommand { get; }
        public ICommand HomeCommand { get; }
        public ICommand ProfileCommand { get; }
        public ICommand OpenMenuCommand { get; }
        public ICommand CourseCommand { get; }
        public ICommand FirstProfileCommand { get; }
        public ICommand EvaluationCommand { get; }

        public AttendanceViewModel()
        {
            _apiService = new ApiService();

            LoadCoursesCommand = new Command(async () => await LoadCoursesAsync());
            LoadStudentsCommand = new Command(async () => await LoadStudentsAsync());
            MarkPresentCommand = new Command<StudentViewModel>(async (student) => await MarkAttendanceAsync(student, true));
            MarkAbsentCommand = new Command<StudentViewModel>(async (student) => await MarkAttendanceAsync(student, false));

            //Barra de navegacion inferior
            HomeCommand = new Command(async () => await Shell.Current.GoToAsync("///homepage"));
            ProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///profile"));
            OpenMenuCommand = new Command(async () => await Shell.Current.GoToAsync("///menu"));
            CourseCommand = new Command(async () => await Shell.Current.GoToAsync("///courses"));
            FirstProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///firtsprofile"));

        }

        private async Task LoadCoursesAsync()
        {
            var userId = await SecureStorage.GetAsync("user_id");
            var schoolId = await SecureStorage.GetAsync("school_id");

            if (!int.TryParse(userId, out int profId) || !int.TryParse(schoolId, out int schId))
                return;

            var url = $"api/courses/user/{profId}/taught-courses?schoolId={schId}";
            var courses = await _apiService.GetAsync<List<Course>>(url);

            Courses.Clear();
            if (courses != null)
            {
                foreach (var course in courses)
                    Courses.Add(course);
            }
        }


        private async Task LoadStudentsAsync()
        {
            if (SelectedCourse == null) return;
            var students = await _apiService.GetAsync<List<StudentViewModel>>($"api/enrollments/course/{SelectedCourse.CourseID}/students");
            Students.Clear();
            if (students != null)
                foreach (var s in students)
                    Students.Add(s);
        }


        private async Task MarkAttendanceAsync(StudentViewModel student, bool isPresent)
        {
            if (student == null || SelectedCourse == null)
                return;

            var userId = await SecureStorage.GetAsync("user_id");
            var schoolIdStr = await SecureStorage.GetAsync("school_id");

            if (!int.TryParse(userId, out int relatedUserId) || !int.TryParse(schoolIdStr, out int schoolId))
                return;

            var attendances = new List<Attendance>
    {
        new Attendance
        {
            UserID = student.UserID,
            RelatedUserID = relatedUserId,
            CourseID = SelectedCourse.CourseID,
            SchoolID = schoolId, // ✅ Ahora enviamos el schoolID
            Status = isPresent ? "Presente" : "Ausente",
            Date = DateTime.UtcNow
        }
    };

            Console.WriteLine($"📤 Mandando asistencia: {JsonSerializer.Serialize(attendances)}");

            bool success = await _apiService.PostAsync("api/attendance/mark", attendances);

            if (success)
                await Application.Current.MainPage.DisplayAlert("✔", "Asistencia registrada", "OK");
            else
                await Application.Current.MainPage.DisplayAlert("❌", "Error al registrar asistencia", "OK");
        }

    }

}


      
