using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using SchoolProyectApp.Models;
using SchoolProyectApp.Services;

namespace SchoolProyectApp.ViewModels
{
    public class ScheduleViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private string _userName;
        private int _userId;
        private int _roleId;

        public ICommand SelectDayCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ProfileCommand { get; }
        public ICommand OpenMenuCommand { get; }
        public ICommand CourseCommand { get; }
        public ICommand FirstProfileCommand { get; }

        public ScheduleViewModel()
        {
            _apiService = new ApiService();
            RefreshCommand = new Command(async () => await LoadWeeklySchedule());

            //Task.Run(async () => await LoadWeeklySchedule());
            _ = LoadWeeklySchedule();

            ProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///profile"));
            OpenMenuCommand = new Command(async () => await Shell.Current.GoToAsync("///menu"));
            CourseCommand = new Command(async () => await Shell.Current.GoToAsync("///courses"));
            FirstProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///firtsprofile"));

            SelectedDay = 1; // Lunes por defecto

            SelectDayCommand = new Command<object>(param =>
            {
                if (int.TryParse(param?.ToString(), out int day))
                {
                    OnDaySelected(day);
                }
                else
                {
                    Debug.WriteLine("❌ Parámetro de comando no válido: " + param);
                }
            });
        }

        public int RoleID
        {
            get => _roleId;
            set
            {
                if (_roleId != value)
                {
                    _roleId = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsProfessor));
                    OnPropertyChanged(nameof(IsStudent));
                    OnPropertyChanged(nameof(IsParent));
                    OnPropertyChanged(nameof(IsHiddenForProfessor));
                    OnPropertyChanged(nameof(IsHiddenForStudent));
                }
            }
        }

        public bool IsProfessor => RoleID == 2;
        public bool IsStudent => RoleID == 1;
        public bool IsParent => RoleID == 3;

        public bool IsHiddenForProfessor => !IsProfessor;
        public bool IsHiddenForStudent => !IsStudent;

        public string UserName
        {
            get => _userName;
            set
            {
                if (_userName != value)
                {
                    _userName = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(WelcomeMessage));
                }
            }
        }

        public string WelcomeMessage => $"¡Bienvenido, {UserName}!";

        public ObservableCollection<Notification> Notifications { get; set; } = new();

        public ObservableCollection<DaySchedule> WeeklySchedule { get; set; } = new();

        public bool HasScheduleData => WeeklySchedule.Count > 0;

        public ObservableCollection<Course> AllCourses { get; set; } = new();

        private ObservableCollection<Course> _filteredCourses = new();
        public ObservableCollection<Course> FilteredCourses
        {
            get => _filteredCourses;
            set
            {
                _filteredCourses = value;
                OnPropertyChanged();
            }
        }

        private int _selectedDay;
        public int SelectedDay
        {
            get => _selectedDay;
            set
            {
                if (_selectedDay != value)
                {
                    _selectedDay = value;
                    OnPropertyChanged();
                    FilterCourses();
                }
            }
        }

        private string _message;
        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                OnPropertyChanged();
            }
        }

        private void OnDaySelected(int day)
        {
            Debug.WriteLine($"🟢 Botón día presionado: {day}");
            SelectedDay = day;
            FilterCourses();
        }

        private void FilterCourses()
        {
            if (AllCourses == null)
                return;

            var filtered = AllCourses.Where(c => c.DayOfWeek == SelectedDay).ToList();
            FilteredCourses = new ObservableCollection<Course>(filtered);

            Debug.WriteLine($"📅 Día {SelectedDay}: {FilteredCourses.Count} cursos encontrados.");
        }

        public async Task LoadWeeklySchedule()
        {
            try
            {
                var userIdString = await SecureStorage.GetAsync("user_id");

                if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
                {
                    Message = "Error: No se encontró el usuario.";
                    return;
                }

                Debug.WriteLine($"🔍 Buscando horario para el usuario ID: {userId}");

                var scheduleData = await _apiService.GetUserWeeklySchedule(userId);

                if (scheduleData == null)
                {
                    Message = "Error al obtener datos del horario.";
                    return;
                }

                if (scheduleData.Count == 0)
                {
                    Message = "No tienes clases programadas.";
                    return;
                }

                // Llenar AllCourses
                AllCourses = new ObservableCollection<Course>(scheduleData.Select(c => new Course
                {
                    CourseID = c.CourseID,
                    Name = c.CourseName,
                    DayOfWeek = c.DayOfWeek
                }));

                OnPropertyChanged(nameof(AllCourses));

                // Actualizar filtrados
                FilterCourses();

                // Agrupar para WeeklySchedule
                WeeklySchedule.Clear();
                var groupedSchedule = scheduleData
                    .GroupBy(c => c.DayOfWeek)
                    .OrderBy(g => g.Key)
                    .Select(g => new DaySchedule
                    {
                        DayOfWeek = GetDayName(g.Key),
                        Courses = new ObservableCollection<Course>(g.Select(c => new Course
                        {
                            CourseID = c.CourseID,
                            Name = c.CourseName
                        }))
                    });

                foreach (var item in groupedSchedule)
                {
                    WeeklySchedule.Add(item);
                }

                Message = ""; // Limpiar mensaje
                Debug.WriteLine($"✅ {WeeklySchedule.Count} días con clases cargados.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error en LoadWeeklySchedule: {ex.Message}");
                Message = "Error al cargar el horario.";
            }
        }

        private string GetDayName(int dayOfWeek)
        {
            return dayOfWeek switch
            {
                0 => "Domingo",
                1 => "Lunes",   
                2 => "Martes",
                3 => "Miércoles",
                4 => "Jueves",
                5 => "Viernes",
                6 => "Sábado",
                _ => "Desconocido"
            };
        }
    }
}
