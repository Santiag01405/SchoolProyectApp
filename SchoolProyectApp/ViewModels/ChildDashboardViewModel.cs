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
        public ICommand GoBackCommand { get; }

        public ChildDashboardViewModel()
        {
            Debug.WriteLine("DEBUG: ChildDashboardViewModel constructor llamado."); // Nueva línea de depuración
            GoToScheduleCommand = new Command(async () => {
                Debug.WriteLine("DEBUG: GoToScheduleCommand ejecutado."); // Nueva línea de depuración
                await NavigateToChildPage("schedule");
            });
            GoToEvaluationsCommand = new Command(async () => {
                Debug.WriteLine("DEBUG: GoToEvaluationsCommand ejecutado."); // Nueva línea de depuración
                await NavigateToChildPage("evaluation");
            });
            GoBackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
        }

        private async Task NavigateToChildPage(string route)
        {
            Debug.WriteLine($"DEBUG: NavigateToChildPage llamado para la ruta: {route}");

            if (SelectedChild == null)
            {
                Debug.WriteLine($"DEBUG: SelectedChild es nulo en NavigateToChildPage para la ruta: {route}. No se puede navegar.");
                await Application.Current.MainPage!.DisplayAlert("Error", "No se ha seleccionado un hijo para ver esta sección.", "OK");
                return;
            }

            try
            {
                var navigationParameter = new Dictionary<string, object>
                {
                    { "SelectedChild", SelectedChild }
                };

                Debug.WriteLine($"DEBUG: Intentando navegar a la ruta: //{route} con SelectedChild (ID: {SelectedChild.UserID}, Nombre: {SelectedChild.StudentName})");
                await Shell.Current.GoToAsync($"//{route}", navigationParameter);
                Debug.WriteLine($"DEBUG: Navegación a //{route} iniciada.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error al navegar a la página '{route}': {ex.Message}");
                await Application.Current.MainPage!.DisplayAlert("Error", "No se pudo abrir esta sección: " + ex.Message, "OK");
            }
        }
    }
}
