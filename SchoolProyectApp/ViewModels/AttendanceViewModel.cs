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

        public ObservableCollection<Course> Courses { get; set; } = new();
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
                    if (value != null)
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

        public ICommand LoadCoursesCommand { get; }
        public ICommand LoadStudentsCommand { get; }
        public ICommand MarkAttendanceCommand { get; }
        public ICommand MarkAbsentCommand { get; }
        public ICommand MarkPresentCommand { get; }

        public AttendanceViewModel()
        {
            _apiService = new ApiService();

            LoadCoursesCommand = new Command(async () => await LoadCoursesAsync());
            LoadStudentsCommand = new Command(async () => await LoadStudentsAsync());
            MarkPresentCommand = new Command<Student>(async (student) => await MarkAttendanceAsync(student, true));
            MarkAbsentCommand = new Command<Student>(async (student) => await MarkAttendanceAsync(student, false));


            Task.Run(async () => await LoadCoursesAsync());
        }

        private async Task LoadCoursesAsync()
        {
            var userId = await SecureStorage.GetAsync("user_id");
            if (!int.TryParse(userId, out int profId)) return;
            var courses = await _apiService.GetAsync<List<Course>>($"api/courses/user/{profId}/taught-courses");
            Courses.Clear();
            if (courses != null)
                foreach (var course in courses)
                    Courses.Add(course);
        }

        private async Task LoadStudentsAsync()
        {
            if (SelectedCourse == null) return;
            var students = await _apiService.GetAsync<List<Student>>($"api/enrollments/course/{SelectedCourse.CourseID}/students");
            Students.Clear();
            if (students != null)
                foreach (var s in students)
                    Students.Add(s);
        }

        private async Task MarkAttendanceAsync(Student student, bool isPresent)
        {
            if (student == null || SelectedCourse == null)
                return;

            var userId = await SecureStorage.GetAsync("user_id");
            if (!int.TryParse(userId, out int relatedUserId))
                return;

            var attendances = new List<Attendance>
    {
        new Attendance
        {
            UserID = student.UserID,
            RelatedUserID = relatedUserId,
            CourseID = SelectedCourse.CourseID,
            Status = isPresent ? "Presente" : "Ausente",
            Date = DateTime.UtcNow
        }
    };

            Console.WriteLine($"📤 Mandando asistencia: {JsonSerializer.Serialize(attendances)}");

            bool success = await _apiService.PostAsync("api/attendance/mark", attendances);

            if (success)
                await Application.Current.MainPage.DisplayAlert("✔", "Asistencia registrada", "OK");
            else
                await Application.Current.MainPage.DisplayAlert("❌", "Error al registrar", "OK");
        }
    }
}

        /* private async Task MarkAttendanceAsync(bool isPresent)
         {
             if (SelectedStudent == null || SelectedCourse == null) return;

             var attendanceList = new List<Attendance>
             {
                 new Attendance
                 {
                     UserID = SelectedStudent.UserID,
                     RelatedUserID = SelectedStudent.UserID,
                     CourseID = SelectedCourse.CourseID,
                     Status = isPresent ? "Presente" : "Ausente",
                     Date = DateTime.UtcNow
                 }
             };

             var response = await _apiService.PostAsync("api/attendance/mark", attendanceList);
             await Application.Current.MainPage.DisplayAlert("Asistencia", isPresent ? "Marcado como presente" : "Marcado como ausente", "OK");
         */





/*using System.Collections.ObjectModel;
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

        public ObservableCollection<Course> Courses { get; set; } = new();
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
                    if (value != null)
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

        public ICommand LoadCoursesCommand { get; }
        public ICommand LoadStudentsCommand { get; }
        public ICommand MarkPresentCommand { get; }
        public ICommand MarkAbsentCommand { get; }
        public ICommand HomeCommand { get; }
        public ICommand ProfileCommand { get; }
        public ICommand OpenMenuCommand { get; }
        public ICommand CourseCommand { get; }
        public ICommand FirstProfileCommand { get; }
        

        public AttendanceViewModel()
        {
            _apiService = new ApiService();

            LoadCoursesCommand = new Command(async () => await LoadCoursesAsync());
            LoadStudentsCommand = new Command(async () => await LoadStudentsAsync());
            MarkPresentCommand = new Command(async () => await MarkAttendanceAsync(true));
            MarkAbsentCommand = new Command(async () => await MarkAttendanceAsync(false));

            //Barra de navegacion inferior

            HomeCommand = new Command(async () => await Shell.Current.GoToAsync("///homepage"));
            OpenMenuCommand = new Command(async () => await Shell.Current.GoToAsync("///menu"));
            CourseCommand = new Command(async () => await Shell.Current.GoToAsync("///courses"));
            FirstProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///firtsprofile"));


            Task.Run(async () => await LoadCoursesAsync());
        }

        private async Task LoadCoursesAsync()
        {
            var userId = await SecureStorage.GetAsync("user_id");
            if (!int.TryParse(userId, out int profId)) return;
            var courses = await _apiService.GetAsync<List<Course>>($"api/courses/user/{profId}/taught-courses");
            Courses.Clear();
            if (courses != null)
                foreach (var course in courses)
                    Courses.Add(course);
        }

        private async Task LoadStudentsAsync()
        {
            if (SelectedCourse == null) return;
            var students = await _apiService.GetAsync<List<Student>>($"api/enrollments/course/{SelectedCourse.CourseID}/students");
            Students.Clear();
            if (students != null)
                foreach (var s in students)
                    Students.Add(s);
        }

        private async Task MarkAttendanceAsync(Student student, bool isPresent)
        {
            if (student == null || SelectedCourse == null) return;

            var attendance = new Attendance
            {
                UserID = student.UserID,
                CourseID = SelectedCourse.CourseID,
                Status = isPresent ? "Presente" : "Ausente",
                Date = DateTime.UtcNow
            };

            var response = await _apiService.PostAsync("api/attendance/mark", attendance);
            await Application.Current.MainPage.DisplayAlert("Asistencia", isPresent ? "Marcado como presente" : "Marcado como ausente", "OK");
        }


        /*private async Task MarkAttendanceAsync(bool isPresent)
        {
            if (SelectedStudent == null || SelectedCourse == null) return;

            var attendance = new Attendance
            {
                UserID = SelectedStudent.UserID,
                CourseID = SelectedCourse.CourseID,
                Status = isPresent ? "Presente" : "Ausente",
                Date = DateTime.UtcNow
            };

            var response = await _apiService.PostAsync("api/attendance/mark", attendance);
            await Application.Current.MainPage.DisplayAlert("Asistencia", isPresent ? "Marcado como presente" : "Marcado como ausente", "OK");
        }//
    }
}*/




