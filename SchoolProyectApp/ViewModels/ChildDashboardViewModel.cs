using SchoolProyectApp.Models;
using SchoolProyectApp.Views;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Diagnostics; // Añadir para Debug.WriteLine

namespace SchoolProyectApp.ViewModels
{
    [QueryProperty(nameof(StudentId), "studentId")]
    [QueryProperty(nameof(ChildName), "childName")]
    public class ChildDashboardViewModel : BaseViewModel
    {
        private int _studentId;
        public int StudentId
        {
            get => _studentId;
            set => SetProperty(ref _studentId, value);
        }

        private string _childName = string.Empty; // Inicializar para evitar advertencia de nulidad
        public string ChildName
        {
            get => _childName;
            set => SetProperty(ref _childName, value);
        }

        public ICommand GoToScheduleCommand { get; }
        public ICommand GoToEvaluationsCommand { get; }
        public ICommand GoToGradesCommand { get; }
        public ICommand GoBackCommand { get; }
        public ICommand GoToExtraActivitiesCommand { get; }

        public ChildDashboardViewModel()
        {

            Debug.WriteLine("DEBUG: ChildDashboardViewModel constructor llamado.");

            GoToScheduleCommand = new Command(async () => {
                Debug.WriteLine("DEBUG: GoToScheduleCommand ejecutado.");
                await NavigateToChildPage("schedule");
            });

            GoToEvaluationsCommand = new Command(async () => {
                Debug.WriteLine("DEBUG: GoToEvaluationsCommand ejecutado.");
                await NavigateToChildPage("evaluation");
            });

            // ----- AQUÍ VA LA LÍNEA DE DEBUG -----
            GoBackCommand = new Command(async () =>
            {
                // Esta es la línea que necesitamos ver en la consola.
                Debug.WriteLine("DEBUG: GoBackCommand fue PRESIONADO.");
                await Shell.Current.GoToAsync("..");
            });

            // --> AÑADE ESTE BLOQUE NUEVO <--
            GoToGradesCommand = new Command(async () => {
                Debug.WriteLine("DEBUG: GoToGradesCommand ejecutado.");
                await NavigateToChildPage("seeGrades");
            });

            // -----IR A ACTIVIDADES EXTRACURRICULARES----
            GoToExtraActivitiesCommand = new Command(async () => {
                Debug.WriteLine("DEBUG: GoToExtraActivitiesCommand ejecutado.");
                await NavigateToChildPage("seeExtra");
            });
        }


        private async Task NavigateToChildPage(string route)
        {
            Debug.WriteLine($"DEBUG: NavigateToChildPage llamado para la ruta: {route}");

            // Ahora validamos usando el StudentId que recibimos
            if (StudentId == 0)
            {
                Debug.WriteLine($"DEBUG: StudentId es cero, no se puede navegar.");
                await Application.Current.MainPage!.DisplayAlert("Error", "No se ha seleccionado un hijo.", "OK");
                return;
            }

            try
            {
                // Construimos la ruta y le pasamos el ID del hijo.
                // Usamos la ruta "seeGrades" que ya tienes registrada en AppShell.
                await Shell.Current.GoToAsync($"{route}?studentId={StudentId}");

                Debug.WriteLine($"DEBUG: Navegación a {route}?studentId={StudentId} iniciada.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error al navegar a la página '{route}': {ex.Message}");
                await Application.Current.MainPage!.DisplayAlert("Error", "No se pudo abrir esta sección: " + ex.Message, "OK");
            }


        }
    }
}
