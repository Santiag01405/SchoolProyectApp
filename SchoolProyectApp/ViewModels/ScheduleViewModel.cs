using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using SchoolProyectApp.Models;
using SchoolProyectApp.Services;
using System.Linq;

namespace SchoolProyectApp.ViewModels
{
    public class ScheduleViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private string _userName;
        private int _userId;

        private int _roleId;

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


        // Propiedades booleanas para Binding en XAML
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

        public ObservableCollection<DaySchedule> WeeklySchedule { get; set; } = new ObservableCollection<DaySchedule>();

        public bool HasScheduleData => WeeklySchedule.Count > 0;

        private string _message;
        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                OnPropertyChanged(nameof(Message));
            }
        }

        public ICommand RefreshCommand { get; }
        public ICommand ProfileCommand { get; }
        public ICommand OpenMenuCommand { get; }
        public ICommand CourseCommand { get; }
        public ICommand FirstProfileCommand { get; }

        public ScheduleViewModel()
        {
            _apiService = new ApiService();
            RefreshCommand = new Command(async () => await LoadWeeklySchedule());

            Task.Run(async () => await LoadWeeklySchedule());

            ProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///profile"));
            OpenMenuCommand = new Command(async () => await Shell.Current.GoToAsync("///menu"));
            CourseCommand = new Command(async () => await Shell.Current.GoToAsync("///courses"));
            FirstProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///firtsprofile"));
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

                // ✅ Limpiar la lista y notificar cambios
                WeeklySchedule.Clear();
                OnPropertyChanged(nameof(WeeklySchedule));

                // ✅ Agrupar los cursos por día de la semana
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
                    }).ToList();

                foreach (var item in groupedSchedule)
                {
                    WeeklySchedule.Add(item);
                }

                Message = ""; // Limpia mensajes de error
                OnPropertyChanged(nameof(Message));

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


/*using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using SchoolProyectApp.Models;
using SchoolProyectApp.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
namespace SchoolProyectApp.ViewModels
{
    public class ScheduleViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        public ObservableCollection<DaySchedule> WeeklySchedule { get; set; } = new ObservableCollection<DaySchedule>();
        public bool HasScheduleData => WeeklySchedule != null && WeeklySchedule.Count > 0;
        private string _message;
        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                OnPropertyChanged(nameof(Message));
            }
        }
        public ICommand RefreshCommand { get; }
        public ICommand HomeCommand { get; }
        public ICommand ProfileCommand { get; }
        public ICommand OpenMenuCommand { get; }
        public ICommand CourseCommand { get; }
        public ICommand FirstProfileCommand { get; }


        public ScheduleViewModel()
        {
            _apiService = new ApiService();
            RefreshCommand = new Command(async () => await LoadWeeklySchedule());
            Task.Run(async () => await LoadWeeklySchedule());

            ProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///profile"));
            OpenMenuCommand = new Command(async () => await Shell.Current.GoToAsync("///menu"));
            CourseCommand = new Command(async () => await Shell.Current.GoToAsync("///courses"));
            FirstProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///firtsprofile"));
        }

        private async Task LoadWeeklySchedule()
        {
            var userId = await SecureStorage.GetAsync("user_id");
            if (string.IsNullOrEmpty(userId))
            {
                Message = "Error: No se encontró el usuario.";
                OnPropertyChanged(nameof(Message));
                return;
            }

            var scheduleData = await _apiService.GetUserWeeklySchedule(int.Parse(userId));

            // ✅ Limpiar antes de agregar
            WeeklySchedule.Clear();

            if (scheduleData == null || scheduleData.Count == 0)
            {
                Message = "No tienes clases programadas para hoy.";
                OnPropertyChanged(nameof(Message));
                return;
            }

            // ✅ Agrupar los cursos por día de la semana sin agregar duplicados
            var groupedSchedule = scheduleData
                .GroupBy(c => c.DayOfWeek)
                .OrderBy(g => g.Key)
                .Select(g => new DaySchedule
                {
                    DayOfWeek = GetDayName(g.Key),
                    Courses = new ObservableCollection<Course>(g.ToList())
                })
                .ToList();

            foreach (var item in groupedSchedule)
            {
                // ✅ Evitar duplicados
                if (!WeeklySchedule.Any(x => x.DayOfWeek == item.DayOfWeek))
                {
                    WeeklySchedule.Add(item);
                }
            }

            Message = ""; // Limpia mensajes de error
            OnPropertyChanged(nameof(Message));

            // ✅ Verificación final
            if (WeeklySchedule.Count == 0)
            {
                Message = "No hay horarios disponibles.";
                OnPropertyChanged(nameof(Message));
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
}*/


/*namespace SchoolProyectApp.ViewModels
{
    public class ScheduleViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private int _selectedDay;
        private List<Enrollment> _allEnrollments = new();

        public ICommand HomeCommand { get; }
        public ICommand ProfileCommand { get; }
        public ICommand OpenMenuCommand { get; }
        public ICommand CourseCommand { get; }
        public ICommand FirstProfileCommand { get; }
        public ObservableCollection<Enrollment> Schedule { get; set; } = new();

        public List<string> Days { get; } = new()
    {
        "Domingo", "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado"
    };

        public int SelectedDay
        {
            get => _selectedDay;
            set
            {
                if (_selectedDay != value)
                {
                    _selectedDay = value;
                    OnPropertyChanged();
                    FilterSchedule();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ScheduleViewModel(int userId)
        {
            _apiService = new ApiService();
            LoadSchedule(userId);
        }


        private async void LoadSchedule(int userId)
        {
            _allEnrollments = await _apiService.GetUserWeeklySchedule(userId);
            FilterSchedule();
        }

        private void FilterSchedule()
        {
            Schedule.Clear();
            foreach (var enrollment in _allEnrollments.Where(e => e.DayOfWeek == SelectedDay))
            {
                Schedule.Add(enrollment);
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}*/

//OTRO INTENTO
