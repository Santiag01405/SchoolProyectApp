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

        public bool IsHiddenForProfessor => !IsProfessor;
        public bool IsHiddenForStudent => !IsStudent;

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
                        
                        RoleID = user.RoleID; // 🔹 Aquí nos aseguramos de que se asigne correctamente
                       

                        // 🔹 Forzar actualización en UI
                        OnPropertyChanged(nameof(RoleID));
                        OnPropertyChanged(nameof(IsProfessor));
                        OnPropertyChanged(nameof(IsStudent));
                        OnPropertyChanged(nameof(IsParent));
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


/*using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using SchoolProyectApp.Services;
using SchoolProyectApp.Models;

namespace SchoolProyectApp.ViewModels
{
    public class MenuViewModel : BaseViewModel
    {

        private readonly ApiService _apiService;
        private string _userName;
        private string _email;
        private string _role;
        private int _roleId; // Nueva propiedad para RoleID

        /*public string UserName
        {
            get => _userName;
            set => SetProperty(ref _userName, value);
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string Role
        {
            get => _role;
            set => SetProperty(ref _role, value);
        }//

        public int RoleID
        {
            get => _roleId;
            set
            {
                _roleId = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsProfessor));
                OnPropertyChanged(nameof(IsStudent));
                OnPropertyChanged(nameof(IsParent));
            }
        }

        // Propiedades booleanas para Binding en XAML
        public bool IsProfessor => RoleID == 2;
        public bool IsStudent => RoleID == 1;
        public bool IsParent => RoleID == 3;

        public ICommand HomeCommand { get; }
        public ICommand OpenScheduleCommand { get; }
        public ICommand OpenEditScheduleCommand { get; } 
        public ICommand OpenMenuCommand { get; }
        public ICommand CourseCommand { get; }
        public ICommand ProfileCommand { get; }
        public ICommand LoginCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand FirstProfileCommand { get; }
        public ICommand NotificationCommand { get; }
        public ICommand SendNotificationCommand { get; }
        public ICommand CreateEvaluationCommand { get; }
        public ICommand EvaluationCommand { get; }

        public MenuViewModel(ApiService apiService)
        {
            _apiService = apiService;
            _ = LoadUserData(); 
        }


        //Cargar usuario
        private async Task LoadUserData()
        {
            var storedUserId = await SecureStorage.GetAsync("user_id");

            if (!string.IsNullOrEmpty(storedUserId) && int.TryParse(storedUserId, out int userId))
            {
                var user = await _apiService.GetUserDetailsAsync(userId);

                if (user != null)
                {
                    //UserName = user.UserName;
                    //Email = user.Email;
                    RoleID = user.RoleID; // Asignamos el RoleID
                    //Role = RoleID == 1 ? "Estudiante" : RoleID == 2 ? "Profesor" : RoleID == 3 ? "Padre" : "Desconocido";
                }
            }
        }

        public MenuViewModel()
        {
            // Barra de navegación
            HomeCommand = new Command(async () => await Shell.Current.GoToAsync("///homepage"));
            ProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///profile"));
            OpenMenuCommand = new Command(async () => await Shell.Current.GoToAsync("///menu"));
            CourseCommand = new Command(async () => await Shell.Current.GoToAsync("///courses"));
            FirstProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///firtsprofile"));
            CreateEvaluationCommand = new Command(async () => await Shell.Current.GoToAsync("///createEvaluation"));
            EvaluationCommand = new Command(async () => await Shell.Current.GoToAsync("///evaluation"));

            //Mandar notificaciones
            SendNotificationCommand = new Command(async () => await Shell.Current.GoToAsync("///sendNotification"));

            //Cerrar sesion
            LogoutCommand = new Command(async () => await Logout());

            //Notificaciones
            NotificationCommand = new Command(async () => await Shell.Current.GoToAsync("///notification"));

            // Todos pueden ver el horario
            OpenScheduleCommand = new Command(async () => await Shell.Current.GoToAsync("///schedule"));

            // Solo los Teachers (rol 2) pueden editar el horario
            OpenEditScheduleCommand = new Command(async () =>
            {
                if (GetUserRole() == 2) // 🔹 Validar rol en ejecución, no en constructor
                {
                    await Shell.Current.GoToAsync("///editschedule");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Acceso Denegado", "Solo los maestros pueden editar el horario.", "OK");
                }
            });

        }

        private int GetUserRole()
        {
            var roleString = SecureStorage.GetAsync("user_role").Result;
            return int.TryParse(roleString, out int role) ? role : 0;
        }

        //Logout
        private async Task Logout()
        {
            if (Application.Current.MainPage is AppShell shell)
            {
                await shell.Logout();
            }
        }
    }
}*/
