// SchoolProyectApp/ViewModels/StudentActivitiesViewModel.cs
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using SchoolProyectApp.Models;
using SchoolProyectApp.Services;
using System.Diagnostics;

namespace SchoolProyectApp.ViewModels
{
    // =========================================================================
    // CAMBIO 1: Añadimos QueryProperty para recibir el ID del hijo desde el dashboard
    // =========================================================================
    [QueryProperty(nameof(StudentId), "studentId")]
    public class StudentActivitiesViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private ObservableCollection<ExtracurricularActivity> _activities;
        private string _pageTitle;
        private int _studentId; // Para recibir el ID del hijo

        // =========================================================================
        // CAMBIO 2: Creamos la propiedad StudentId.
        // Cuando recibe un valor (del padre), dispara la carga de datos.
        // =========================================================================
        public int StudentId
        {
            get => _studentId;
            set
            {
                if (_studentId != value)
                {
                    SetProperty(ref _studentId, value);
                    Debug.WriteLine($"[DEBUG] StudentId recibido: {value}. Cargando datos para este estudiante.");
                    // Carga los datos usando el ID del hijo que acabamos de recibir.
                    _ = LoadDataForStudentAsync(value);
                }
            }
        }

        public string PageTitle
        {
            get => _pageTitle;
            set => SetProperty(ref _pageTitle, value);
        }

        public ObservableCollection<ExtracurricularActivity> Activities
        {
            get => _activities;
            set => SetProperty(ref _activities, value);
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

        private Color _subtleTextColor;
        public Color SubtleTextColor
        {
            get => _subtleTextColor;
            set => SetProperty(ref _subtleTextColor, value);
        }

        private Color _pageBackgroundColor;
        public Color PageBackgroundColor
        {
            get => _pageBackgroundColor;
            set => SetProperty(ref _pageBackgroundColor, value);
        }

        public ICommand LoadActivitiesCommand { get; }
        public ICommand HomeCommand { get; }
        public ICommand FirstProfileCommand { get; }
        public ICommand OpenMenuCommand { get; }

        public StudentActivitiesViewModel()
        {
            _apiService = new ApiService();
            Activities = new ObservableCollection<ExtracurricularActivity>();
            HomeCommand = new Command(async () => await Shell.Current.GoToAsync("///homepage"));
            OpenMenuCommand = new Command(async () => await Shell.Current.GoToAsync("///menu"));
            FirstProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///firtsprofile"));

            _ = LoadThemeAsync();

            // Se dispara automáticamente solo cuando un estudiante abre su propia vista.
            _ = LoadDataForCurrentUserAsync();
        }

        // =========================================================================
        // CAMBIO 3: Nueva función para cargar datos cuando el padre navega.
        // =========================================================================
        private async Task LoadDataForStudentAsync(int studentId)
        {
            if (studentId == 0) return;
            IsBusy = true;
            try
            {
                await SetPageTitleAsync(studentId);
                await LoadActivitiesAsync(studentId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Falló LoadDataForStudentAsync: {ex.Message}");
                Message = "Error al cargar los datos del estudiante.";
            }
            finally
            {
                IsBusy = false;
            }
        }

        // =========================================================================
        // CAMBIO 4: Lógica original, ahora en su propia función.
        // Se ejecuta solo si no se ha recibido un StudentId (vista del estudiante).
        // =========================================================================
        private async Task LoadDataForCurrentUserAsync()
        {
            // Si ya se cargaron datos para un hijo (vista de padre), no hagas nada.
            if (StudentId > 0)
            {
                Debug.WriteLine("[DEBUG] Se omite la carga para el usuario actual porque ya se cargó para un StudentId.");
                return;
            }

            IsBusy = true;
            try
            {
                var userIdString = await SecureStorage.GetAsync("user_id");
                if (!string.IsNullOrEmpty(userIdString) && int.TryParse(userIdString, out int currentUserId))
                {
                    await SetPageTitleAsync(currentUserId);
                    await LoadActivitiesAsync(currentUserId);
                }
                else
                {
                    Message = "No se pudo identificar al usuario.";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Falló LoadDataForCurrentUserAsync: {ex.Message}");
                Message = "Error al cargar tus datos.";
            }
            finally
            {
                IsBusy = false;
            }
        }

        // =========================================================================
        // CAMBIO 5: Ambos métodos de carga ahora usan el mismo método base con un ID.
        // =========================================================================
        private async Task SetPageTitleAsync(int userId)
        {
            var user = await _apiService.GetUserDetailsAsync(userId);
            PageTitle = user != null ? user.UserName : "Actividades";
        }

        private async Task LoadActivitiesAsync(int userId)
        {
            if (userId == 0)
            {
                Message = "ID de usuario no válido.";
                return;
            }

            try
            {
                var enrolledActivities = await _apiService.GetStudentActivitiesAsync(userId);
                if (enrolledActivities != null && enrolledActivities.Any())
                {
                    Activities.Clear();
                    foreach (var activity in enrolledActivities)
                    {
                        Activities.Add(activity);
                    }
                    Message = string.Empty;
                }
                else
                {
                    Activities.Clear();
                    Message = "No hay actividades inscritas.";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Falló LoadActivitiesAsync: {ex.Message}");
                Message = "No se pudieron cargar las actividades.";
            }
        }

        private async Task LoadThemeAsync()
        {
            var schoolIdStr = await SecureStorage.GetAsync("school_id");
            if (int.TryParse(schoolIdStr, out int schoolId))
            {
                // 🎨 aplicar colores dinámicos
                if (schoolId == 5)
                {
                    PrimaryColor = Color.FromArgb("#0d4483");
                    SecondaryColor = Color.FromArgb("#0098da");
                    SubtleTextColor = Colors.DarkGray;
                    PageBackgroundColor = Colors.White;
                }
                else
                {
                    PrimaryColor = Color.FromArgb("#0C4251");
                    SecondaryColor = Colors.Blue;
                    SubtleTextColor = Colors.Gray;
                    PageBackgroundColor = Colors.White;
                }
            }
        }
    }
}