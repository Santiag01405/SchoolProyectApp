using SchoolProyectApp.Models;
using SchoolProyectApp.Views;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Diagnostics; // Añadir para Debug.WriteLine

namespace SchoolProyectApp.ViewModels
{
    [QueryProperty(nameof(SelectedChild), "SelectedChild")]
    public class ChildDashboardViewModel : BaseViewModel
    {
        private Child? _selectedChild; // Hacerlo anulable para seguridad
        public Child? SelectedChild
        {
            get => _selectedChild;
            set
            {
                SetProperty(ref _selectedChild, value);
                if (value != null)
                {
                    ChildName = value.StudentName;
                    Debug.WriteLine($"DEBUG: ChildDashboardViewModel - SelectedChild establecido: {value.StudentName} (ID: {value.UserID})");
                }
                else
                {
                    Debug.WriteLine("DEBUG: ChildDashboardViewModel - SelectedChild establecido como NULL.");
                }
            }
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
        }


        private async Task NavigateToChildPage(string route)
        {
            Debug.WriteLine($"DEBUG: NavigateToChildPage llamado para la ruta: {route}");

            if (SelectedChild == null)
            {
                Debug.WriteLine($"DEBUG: SelectedChild es nulo, no se puede navegar.");
                await Application.Current.MainPage!.DisplayAlert("Error", "No se ha seleccionado un hijo.", "OK");
                return;
            }

            try
            {
                var navigationParameter = new Dictionary<string, object>
        {
            { "SelectedChild", SelectedChild }
        };

                // ----- LA CORRECCIÓN FINAL ESTÁ AQUÍ -----
                // Como la ruta ahora solo está registrada en el código, la llamamos de forma relativa (sin barras).
                string navigationRoute = $"{route}?IsParentView=true";

                Debug.WriteLine($"DEBUG: Intentando navegar a la ruta relativa: {navigationRoute}");

                await Shell.Current.GoToAsync(navigationRoute, navigationParameter);

                Debug.WriteLine($"DEBUG: Navegación a {navigationRoute} iniciada.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error al navegar a la página '{route}': {ex.Message}");
                await Application.Current.MainPage!.DisplayAlert("Error", "No se pudo abrir esta sección: " + ex.Message, "OK");
            }
        }
    }
}
