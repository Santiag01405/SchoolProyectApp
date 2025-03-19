using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using SchoolProyectApp.Models;
using SchoolProyectApp.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SchoolProyectApp.ViewModels
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
}

//OTRO INTENTO
/*namespace SchoolProyectApp.ViewModels
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
    }*/


