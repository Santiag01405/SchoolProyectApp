using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using SchoolProyectApp.Services;
using SchoolProyectApp.Models;

namespace SchoolProyectApp.ViewModels
{
    public class MenuViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
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
        public bool IsNurse => RoleID == 4;

        public bool IsHiddenForProfessor => !IsProfessor;
        public bool IsHiddenForStudent => !IsStudent;

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

        private Color _menuTextColor;
        public Color MenuTextColor
        {
            get => _menuTextColor;
            set => SetProperty(ref _menuTextColor, value);
        }

        private Color _iconColor;
        public Color IconColor
        {
            get => _iconColor;
            set => SetProperty(ref _iconColor, value);
        }


        public ICommand HomeCommand { get; }
        public ICommand ProfileCommand { get; }
        public ICommand OpenMenuCommand { get; }
        public ICommand CourseCommand { get; }
        public ICommand FirstProfileCommand { get; }
        public ICommand CreateEvaluationCommand { get; }
        public ICommand EvaluationCommand { get; }
        public ICommand SendNotificationCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand ScheduleCommand { get;  }
        public ICommand AttendanceCommand { get; }
        public ICommand NotificationCommand { get; }
        public ICommand AssingGradeCommand { get; }
        public ICommand SeeGradesCommand { get; }
        public ICommand MarkExtraAttendanceCommand { get; }
        public ICommand CreateExtraCommand { get; }
        public ICommand SeeExtraCommand { get; }
        public ICommand EnrollExtraCommand { get; }
        public ICommand NurseVisitCommand { get; }

        public MenuViewModel()
        {
            _apiService = new ApiService();
            Task.Run(async () => await LoadUserData());


            // Definir comandos
            HomeCommand = new Command(async () => await Shell.Current.GoToAsync("///homepage"));
            ProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///profile"));
            OpenMenuCommand = new Command(async () => await Shell.Current.GoToAsync("///menu"));
            CourseCommand = new Command(async () => await Shell.Current.GoToAsync("///courses"));
            FirstProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///firtsprofile"));
            CreateEvaluationCommand = new Command(async () => await Shell.Current.GoToAsync("///createEvaluation"));
            EvaluationCommand = new Command(async () => await Shell.Current.GoToAsync("///evaluation"));
            SendNotificationCommand = new Command(async () => await Shell.Current.GoToAsync("///sendNotification"));
            LogoutCommand = new Command(async () => await Logout());
            ScheduleCommand = new Command(async () => await Shell.Current.GoToAsync("///schedule"));
            AttendanceCommand = new Command(async () => await Shell.Current.GoToAsync("///attendance"));
            NotificationCommand = new Command(async () => await Shell.Current.GoToAsync("///notification"));
            AssingGradeCommand = new Command(async () => await Shell.Current.GoToAsync("///assingGrade"));
            SeeGradesCommand = new Command(async () => await Shell.Current.GoToAsync("///seeGrades"));
            MarkExtraAttendanceCommand = new Command(async () => await Shell.Current.GoToAsync("///extraAttendance"));
            CreateExtraCommand = new Command(async () => await Shell.Current.GoToAsync("///createExtra"));
            SeeExtraCommand = new Command(async () => await Shell.Current.GoToAsync("///seeExtra"));
            EnrollExtraCommand = new Command(async () => await Shell.Current.GoToAsync("///enrollExtra"));
            NurseVisitCommand = new Command(async () => await Shell.Current.GoToAsync("///nurseVisit"));
        }
        private async Task LoadUserData()
        {
            try
            {
                var storedUserId = await SecureStorage.GetAsync("user_id");

                if (!string.IsNullOrEmpty(storedUserId) && int.TryParse(storedUserId, out int userId))
                {
                    var user = await _apiService.GetUserDetailsAsync(userId);

                    if (user != null)
                    {
                        
                        RoleID = user.RoleID;
                        var schoolId = user.SchoolID;

                        // 🎨 aplicar colores dinámicos
                        if (schoolId == 5)
                        {
                            PrimaryColor = Color.FromArgb("#0d4483");
                            SecondaryColor = Color.FromArgb("#0098da");
                            MenuTextColor = Colors.White;
                            IconColor = Color.FromArgb("#f1c864"); // Manteniendo el amarillo para los íconos
                        }
                        else
                        {
                            // Colores por defecto del XAML original
                            PrimaryColor = Color.FromArgb("#0C4251");
                            SecondaryColor = Color.FromArgb("#6bbdda");
                            MenuTextColor = Colors.White;
                            IconColor = Color.FromArgb("#f1c864");
                        }


                        // 🔹 Forzar actualización en UI
                        OnPropertyChanged(nameof(RoleID));
                        OnPropertyChanged(nameof(IsProfessor));
                        OnPropertyChanged(nameof(IsStudent));
                        OnPropertyChanged(nameof(IsParent));
                        OnPropertyChanged(nameof(IsNurse));
                    }
                }
                else
                {
                    RoleID = 0; // Asignar un valor por defecto si no se encuentra el usuario
                }

            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No se pudo cargar el usuario: " + ex.Message, "OK");
            }

        }

        private async Task Logout()
        {
            if (Application.Current.MainPage is AppShell shell)
            {
                await shell.Logout();
            }
        }
    }
}

