using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using SchoolProyectApp.Models;
using SchoolProyectApp.Services;
using System.Linq;

namespace SchoolProyectApp.ViewModels
{
    [QueryProperty(nameof(StudentId), "studentId")]
    [QueryProperty(nameof(ChildSchoolId), "schoolId")]
    public class ScheduleViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private string _userName;
        private int _userId;
        private int _roleId;
        private string _pageTitle = "Mi Horario";
        private bool _isBusy;
        private Child _selectedChild;

        // control de carga
        private bool _alreadyLoaded; // evita recargas duplicadas
        private bool _forceReload;   // marca para recargar cuando lleguen los QueryProperty

        public ICommand RefreshCommand { get; }
        public ICommand GoBackCommand { get; }
        public ICommand HomeCommand { get; }
        public ICommand FirstProfileCommand { get; }
        public ICommand OpenMenuCommand { get; }
        public ICommand SelectDayCommand { get; }

        private int _studentId;
        public int StudentId
        {
            get => _studentId;
            set
            {
                if (SetProperty(ref _studentId, value))
                {
                    Debug.WriteLine($"[Schedule] Setter StudentId={_studentId}");
                    _forceReload = true;
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
                    Debug.WriteLine($"[Schedule] Setter ChildSchoolId={_childSchoolId}");
                    _forceReload = true;
                    TryLoadIfReady();
                }
            }
        }

        public ScheduleViewModel()
        {
            _apiService = new ApiService();
            RefreshCommand = new Command(async () => await LoadWeeklySchedule());
            GoBackCommand = new Command(async () => await GoBackAsync());

            HomeCommand = new Command(async () => await Shell.Current.GoToAsync("///homepage"));
            OpenMenuCommand = new Command(async () => await Shell.Current.GoToAsync("///menu"));
            FirstProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///firtsprofile"));

            SelectDayCommand = new Command<object>(param =>
            {
                try
                {
                    if (param == null) return;

                    int day;
                    if (param is int i) day = i;
                    else if (!int.TryParse(param.ToString(), out day)) return;

                    SelectedDay = day;
                    Debug.WriteLine($"[Schedule] SelectedDay -> {SelectedDay}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Schedule] Error en SelectDayCommand: {ex.Message}");
                }
            });

            SelectedDay = (int)DateTime.Now.DayOfWeek;
            if (SelectedDay == 0) SelectedDay = 1;

            // 🚫 ¡Importante! No llames al API aquí.
            // Esperamos a que lleguen los QueryProperty y entonces TryLoadIfReady() disparará la carga correcta.
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
                _forceReload = true;
                TryLoadIfReady();
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
            SelectedChild = null;
            await Shell.Current.GoToAsync("..");
        }

        /// <summary>
        /// Sólo carga cuando:
        /// 1) Es “modo hijo”: StudentId>0 **y** ChildSchoolId>0 (ambos llegaron)
        ///    -> usa ChildSchoolId
        /// 2) Es “modo propio”: StudentId==0 y ChildSchoolId==0
        ///    -> usa school_id del almacenamiento seguro
        /// Evita la primera llamada con el schoolId del padre por adelantado.
        /// </summary>
        private void TryLoadIfReady()
        {
            if (IsBusy) return;

            // Caso “modo hijo”: esperamos a tener AMBOS
            if (StudentId > 0)
            {
                if (ChildSchoolId > 0)
                {
                    Debug.WriteLine("[Schedule] Ready (modo hijo). Cargando con la sede del hijo...");
                    _ = LoadWeeklySchedule();
                }
                else
                {
                    // Aún no ha llegado la sede del hijo -> NO dispares nada
                    Debug.WriteLine("[Schedule] Esperando ChildSchoolId del hijo antes de cargar...");
                }
                return;
            }

            // Caso “modo propio”
            if (!_alreadyLoaded && StudentId == 0 && ChildSchoolId == 0)
            {
                Debug.WriteLine("[Schedule] Ready (modo propio). Cargando con la sede del usuario...");
                _ = LoadWeeklySchedule();
                return;
            }
        }

        public async Task LoadWeeklySchedule()
        {
            if (IsBusy) return;

            IsBusy = true;
            try
            {
                Message = "Cargando horario...";

                // 📌 Determinar schoolId
                int schoolId;
                bool modoHijo = (StudentId != 0);

                if (modoHijo)
                {
                    // En modo hijo siempre usamos la sede del hijo
                    if (ChildSchoolId <= 0)
                    {
                        // defensa extra: si por alguna razón no llegó, no dispares llamada con la del padre
                        Debug.WriteLine("⚠️ ChildSchoolId aún no disponible. Abortando para no usar la sede del padre.");
                        return;
                    }
                    schoolId = ChildSchoolId;
                    PageTitle = "Horario del estudiante";
                }
                else
                {
                    var schoolIdStr = await SecureStorage.GetAsync("school_id");
                    if (!int.TryParse(schoolIdStr, out schoolId))
                    {
                        Message = "Error: No se encontró el ID del colegio.";
                        return;
                    }
                    PageTitle = "Mi Horario";
                }

                // 📌 Determinar userId
                int targetUserId = 0;
                if (modoHijo)
                {
                    targetUserId = StudentId;
                }
                else if (SelectedChild != null)
                {
                    targetUserId = SelectedChild.UserID;
                }
                else
                {
                    var userIdStr = await SecureStorage.GetAsync("user_id");
                    if (!int.TryParse(userIdStr, out targetUserId))
                    {
                        Message = "Error: No se encontró el ID del usuario.";
                        return;
                    }
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

                _alreadyLoaded = true; // marcamos una carga satisfactoria
                FilterCourses();
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