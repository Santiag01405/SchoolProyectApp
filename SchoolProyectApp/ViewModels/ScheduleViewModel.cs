using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using SchoolProyectApp.Models;
using SchoolProyectApp.Services;
using System.Linq;

namespace SchoolProyectApp.ViewModels
{
    // Habilitar la recepción de un objeto Student desde la navegación
    [QueryProperty(nameof(SelectedChild), "SelectedChild")]
    public class ScheduleViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private string _userName;
        private int _userId;
        private int _roleId;
        private string _pageTitle = "Mi Horario";
        private bool _isBusy;
        private Child _selectedChild;

        public ICommand RefreshCommand { get; }
        public ICommand GoBackCommand { get; }

        public ScheduleViewModel()
        {
            _apiService = new ApiService();
            RefreshCommand = new Command(async () => await LoadWeeklySchedule());
            GoBackCommand = new Command(async () => await GoBackAsync());

            // Establece el día actual al cargar, por defecto a lunes si es domingo
            SelectedDay = (int)DateTime.Now.DayOfWeek;
            if (SelectedDay == 0) SelectedDay = 1;
        }

        #region Properties
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public Child SelectedChild
        {
            get => _selectedChild;
            set
            {
                SetProperty(ref _selectedChild, value);
                // Si se establece un hijo, se recarga el horario para ese hijo.
                // Usamos _ = para no esperar el resultado, manteniendo el flujo asincrono
                _ = LoadWeeklySchedule();
            }
        }

        public string PageTitle
        {
            get => _pageTitle;
            set => SetProperty(ref _pageTitle, value);
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
                }
            }
        }

        public bool IsProfessor => RoleID == 2;
        public bool IsStudent => RoleID == 1;
        public bool IsParent => RoleID == 3;

        public string UserName
        {
            get => _userName;
            set
            {
                if (_userName != value)
                {
                    _userName = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<Course> AllCourses { get; set; } = new();

        private ObservableCollection<Course> _filteredCourses = new();
        public ObservableCollection<Course> FilteredCourses
        {
            get => _filteredCourses;
            set => SetProperty(ref _filteredCourses, value);
        }

        private int _selectedDay;
        public int SelectedDay
        {
            get => _selectedDay;
            set
            {
                if (SetProperty(ref _selectedDay, value))
                {
                    FilterCourses();
                }
            }
        }

        private string _message;
        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }
        #endregion

        private async Task GoBackAsync()
        {
            // Limpiamos la referencia al hijo para evitar conflictos futuros
            SelectedChild = null;
            await Shell.Current.GoToAsync("..");
        }

        public async Task LoadWeeklySchedule()
        {
            if (IsBusy) return;

            IsBusy = true;
            try
            {
                Message = "Cargando horario...";

                var schoolIdStr = await SecureStorage.GetAsync("school_id");
                if (!int.TryParse(schoolIdStr, out int schoolId))
                {
                    Message = "Error: No se encontró el ID del colegio.";
                    return;
                }

                int targetUserId;

                // Determina si se carga el horario del hijo o del usuario logeado
                if (SelectedChild != null)
                {
                    targetUserId = SelectedChild.UserID;
                    PageTitle = $"Horario de {SelectedChild.StudentName}";
                }
                else
                {
                    var userIdStr = await SecureStorage.GetAsync("user_id");
                    if (!int.TryParse(userIdStr, out targetUserId))
                    {
                        Message = "Error: No se encontró el ID del usuario.";
                        return;
                    }
                    PageTitle = "Mi Horario";
                }

                Debug.WriteLine($"🔍 Buscando horario para el usuario ID: {targetUserId}, Escuela ID: {schoolId}");

                var scheduleData = await _apiService.GetUserWeeklySchedule(targetUserId, schoolId);

                AllCourses.Clear();
                if (scheduleData == null || !scheduleData.Any())
                {
                    Message = "No hay clases programadas.";
                }
                else
                {
                    foreach (var c in scheduleData)
                    {
                        AllCourses.Add(new Course
                        {
                            CourseID = c.CourseID,
                            Name = c.CourseName,
                            DayOfWeek = c.DayOfWeek
                        });
                    }
                    Message = "";
                }

                FilterCourses(); // Actualiza la lista visible para el día seleccionado
                Debug.WriteLine($"✅ Horario cargado. Total de cursos: {AllCourses.Count}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error en LoadWeeklySchedule: {ex.Message}");
                Message = "Error al cargar el horario.";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void FilterCourses()
        {
            var filtered = AllCourses.Where(c => c.DayOfWeek == SelectedDay).ToList();
            FilteredCourses = new ObservableCollection<Course>(filtered);
        }
    }
}

/*using System.Collections.ObjectModel;
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
                var schoolIdString = await SecureStorage.GetAsync("school_id");

                if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
                {
                    Message = "Error: No se encontró el usuario.";
                    return;
                }

                if (string.IsNullOrEmpty(schoolIdString) || !int.TryParse(schoolIdString, out int schoolId))
                {
                    Message = "Error: No se encontró el colegio.";
                    return;
                }

                Debug.WriteLine($"🔍 Buscando horario para el usuario ID: {userId}, Escuela ID: {schoolId}");

                var scheduleData = await _apiService.GetUserWeeklySchedule(userId, schoolId);

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

                AllCourses = new ObservableCollection<Course>(scheduleData.Select(c => new Course
                {
                    CourseID = c.CourseID,
                    Name = c.CourseName,
                    DayOfWeek = c.DayOfWeek
                }));

                OnPropertyChanged(nameof(AllCourses));
                FilterCourses();

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

                Message = "";
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
}*/
