using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using SchoolProyectApp.Models;
using SchoolProyectApp.Services;
using CommunityToolkit.Mvvm.Input;
using System; // Agrega esto para DateTime

namespace SchoolProyectApp.ViewModels
{
    public class EvaluationsViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private int _userId;
        private int _schoolId; // Asegúrate de que schoolId se inicialice

        private Course _selectedCourse;
        private string _evaluationTitle;
        private string _description;
        private DateTime _selectedDate = DateTime.Today; // Inicializa con la fecha actual

        // Propiedad para el rol del usuario, asegurando que se notifique el cambio
        private int _roleId;
        public int RoleID
        {
            get => _roleId;
            set
            {
                if (SetProperty(ref _roleId, value))
                {
                    OnPropertyChanged(nameof(IsProfessor));
                    OnPropertyChanged(nameof(IsStudent));
                    OnPropertyChanged(nameof(IsParent));
                    // Al cambiar el rol, recargamos la lógica de visibilidad si es necesario
                    UpdateControlVisibilities();
                }
            }
        }

        public bool IsProfessor => RoleID == 2;
        public bool IsStudent => RoleID == 1;
        public bool IsParent => RoleID == 3;

        public ObservableCollection<Course> Courses { get; set; } = new();

        public Course SelectedCourse
        {
            get => _selectedCourse;
            set
            {
                if (SetProperty(ref _selectedCourse, value))
                {
                    UpdateControlVisibilities();
                    // Al seleccionar un curso, automáticamente cargamos las evaluaciones existentes si el rol lo requiere
                    // Aunque en esta página es para CREAR, si se quiere una vista combinada, este sería el lugar.
                    // Para la página de creación, no es estrictamente necesario cargar evaluaciones aquí.
                }
            }
        }

        public string EvaluationTitle
        {
            get => _evaluationTitle;
            set
            {
                if (SetProperty(ref _evaluationTitle, value))
                {
                    UpdateControlVisibilities();
                }
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                if (SetProperty(ref _description, value))
                {
                    UpdateControlVisibilities();
                }
            }
        }

        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (SetProperty(ref _selectedDate, value))
                {
                    UpdateControlVisibilities();
                }
            }
        }

        // Propiedades de visibilidad
        private bool _isTitleVisible;
        public bool IsTitleVisible
        {
            get => _isTitleVisible;
            set => SetProperty(ref _isTitleVisible, value);
        }

        private bool _isDescriptionVisible;
        public bool IsDescriptionVisible
        {
            get => _isDescriptionVisible;
            set => SetProperty(ref _isDescriptionVisible, value);
        }

        private bool _isDateVisible;
        public bool IsDateVisible
        {
            get => _isDateVisible;
            set => SetProperty(ref _isDateVisible, value);
        }

        private bool _isCreateButtonVisible;
        public bool IsCreateButtonVisible
        {
            get => _isCreateButtonVisible;
            set => SetProperty(ref _isCreateButtonVisible, value);
        }

        // Comandos
        public ICommand CreateEvaluationCommand { get; }
        public ICommand HomeCommand { get; }
        public ICommand ProfileCommand { get; }
        public ICommand OpenMenuCommand { get; }
        public ICommand FirstProfileCommand { get; }
        public ICommand ResetCommand { get; }

        public EvaluationsViewModel()
        {
            _apiService = new ApiService();
            CreateEvaluationCommand = new AsyncRelayCommand(CreateEvaluation);
            HomeCommand = new AsyncRelayCommand(() => Shell.Current.GoToAsync("///homepage"));
            ProfileCommand = new AsyncRelayCommand(() => Shell.Current.GoToAsync("///profile"));
            OpenMenuCommand = new AsyncRelayCommand(() => Shell.Current.GoToAsync("///menu"));
            FirstProfileCommand = new AsyncRelayCommand(() => Shell.Current.GoToAsync("///firtsprofile"));
            ResetCommand = new RelayCommand(ResetPage);

            // ❌ Elimina la inicialización en el constructor. Usa OnAppearing para esto.
            // Task.Run(async () => { ... });
        }

        // Nuevo método InitializeAsync para ser llamado desde OnAppearing
        public async Task InitializeAsync()
        {
            if (IsBusy) return; // Evita múltiples inicializaciones
            IsBusy = true;
            try
            {
                await LoadUserData();
                if (IsProfessor) // Solo carga cursos si es profesor
                {
                    await LoadCourses();
                }
                // Los estudiantes no necesitan cursos ni evaluaciones en esta vista de "Crear Evaluación"
                // La página ListEvaluations se encargará de las evaluaciones para estudiantes/padres
                UpdateControlVisibilities(); // Asegura que las visibilidades se actualicen al cargar
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadUserData()
        {
            try
            {
                var storedUserId = await SecureStorage.GetAsync("user_id");
                var storedSchoolId = await SecureStorage.GetAsync("school_id");

                if (!string.IsNullOrEmpty(storedUserId) && int.TryParse(storedUserId, out int userId))
                {
                    _userId = userId;
                }
                else
                {
                    // Manejar caso donde no hay userId (ej. redirigir a login o mostrar error)
                    await Application.Current.MainPage.DisplayAlert("Error de Sesión", "No se encontró el ID de usuario. Por favor, inicie sesión de nuevo.", "OK");
                    await Shell.Current.GoToAsync("///login"); // O la ruta a tu página de login
                    return;
                }

                if (!string.IsNullOrEmpty(storedSchoolId) && int.TryParse(storedSchoolId, out int schoolId))
                {
                    _schoolId = schoolId;
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error de Sesión", "No se encontró el ID del colegio. Por favor, inicie sesión de nuevo.", "OK");
                    await Shell.Current.GoToAsync("///login");
                    return;
                }

                if (_userId > 0)
                {
                    var user = await _apiService.GetUserDetailsAsync(_userId);
                    if (user != null)
                    {
                        RoleID = user.RoleID; // Esto ya notifica cambios y llama a UpdateControlVisibilities
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar datos de usuario: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error", "No se pudo cargar los datos de usuario: " + ex.Message, "OK");
            }
        }

        // Método para cargar cursos
        private async Task LoadCourses()
        {
            if (_schoolId == 0)
            {
                Console.WriteLine("Advertencia: schoolId es 0 al intentar cargar cursos.");
                return;
            }

            var courses = await _apiService.GetCoursesAsync(_schoolId);
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Courses.Clear();
                if (courses != null)
                {
                    foreach (var course in courses)
                    {
                        Courses.Add(course);
                    }
                }
            });
        }

        // Método de reinicio para la página
        private void ResetPage()
        {
            SelectedCourse = null;
            EvaluationTitle = string.Empty;
            Description = string.Empty;
            SelectedDate = DateTime.Today; // Reinicia la fecha al día actual

            // Las propiedades de visibilidad se actualizarán automáticamente
            // al cambiar SelectedCourse, EvaluationTitle, Description y SelectedDate
        }

        // Lógica unificada para actualizar la visibilidad de los controles
        private void UpdateControlVisibilities()
        {
            // Solo si es profesor
            if (!IsProfessor)
            {
                IsTitleVisible = false;
                IsDescriptionVisible = false;
                IsDateVisible = false;
                IsCreateButtonVisible = false;
                return;
            }

            IsTitleVisible = SelectedCourse != null;
            IsDescriptionVisible = !string.IsNullOrWhiteSpace(EvaluationTitle);
            IsDateVisible = !string.IsNullOrWhiteSpace(Description);
            IsCreateButtonVisible = SelectedDate >= DateTime.Today &&
                                    SelectedCourse != null &&
                                    !string.IsNullOrWhiteSpace(EvaluationTitle) &&
                                    !string.IsNullOrWhiteSpace(Description); // Asegúrate de que todos los campos requeridos estén llenos
        }

        private async Task CreateEvaluation()
        {
            Console.WriteLine("📌 Intentando crear evaluación...");

            // Validaciones antes de enviar
            if (!IsProfessor)
            {
                await Shell.Current.DisplayAlert("Permiso Denegado", "Solo los profesores pueden crear evaluaciones.", "OK");
                return;
            }

            if (SelectedCourse == null)
            {
                await Shell.Current.DisplayAlert("Falta curso", "Debes seleccionar un curso.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(EvaluationTitle))
            {
                await Shell.Current.DisplayAlert("Falta título", "El título de la evaluación es obligatorio.", "OK");
                return;
            }

            // La descripción es opcional, así que no es una validación obligatoria aquí,
            // pero si la quieres obligatoria, añádela.
            // if (string.IsNullOrWhiteSpace(Description)) { ... }

            if (SelectedDate < DateTime.Today)
            {
                await Shell.Current.DisplayAlert("Fecha inválida", "No puedes asignar evaluaciones con fecha pasada.", "OK");
                return;
            }

            if (_userId == 0 || _schoolId == 0)
            {
                await Shell.Current.DisplayAlert("Error de Datos", "ID de usuario o de colegio no disponible. Por favor, intente de nuevo o re-inicie sesión.", "OK");
                return;
            }

            var newEvaluation = new Evaluation
            {
                Title = EvaluationTitle,
                Description = Description,
                Date = SelectedDate, // Usa SelectedDate directamente
                UserID = _userId,
                CourseID = SelectedCourse.CourseID,
                SchoolID = _schoolId
            };

            bool success = await _apiService.CreateEvaluationAsync(newEvaluation);
            if (success)
            {
                await Shell.Current.DisplayAlert("Éxito", "Evaluación creada correctamente.", "OK");
                // No necesitas cargar evaluaciones aquí, ya que es una página de creación.
                // Si quieres actualizar la lista de evaluaciones en otra página, hazlo en esa página.
                ResetPage(); // Resetea la página para crear otra evaluación
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "No se pudo crear la evaluación. Intenta de nuevo.", "OK");
            }
        }
    }
}