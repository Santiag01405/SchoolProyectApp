using SchoolProyectApp.Models;
using SchoolProyectApp.Views;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Diagnostics;

namespace SchoolProyectApp.ViewModels
{
    [QueryProperty(nameof(StudentId), "studentId")]
    [QueryProperty(nameof(ChildName), "childName")]
    [QueryProperty(nameof(ChildSchoolId), "schoolId")]
    public class ChildDashboardViewModel : BaseViewModel
    {
        private int _studentId;
        public int StudentId
        {
            get => _studentId;
            set => SetProperty(ref _studentId, value);
        }

        private int _childSchoolId;
        public int ChildSchoolId
        {
            get => _childSchoolId;
            set => SetProperty(ref _childSchoolId, value);
        }

        private string _childName = string.Empty;
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

            GoBackCommand = new Command(async () =>
            {
                Debug.WriteLine("DEBUG: GoBackCommand fue PRESIONADO.");
                await Shell.Current.GoToAsync("..");
            });

            GoToGradesCommand = new Command(async () => {
                Debug.WriteLine("DEBUG: GoToGradesCommand ejecutado.");
                await NavigateToChildPage("seeGrades");
            });

            GoToExtraActivitiesCommand = new Command(async () => {
                Debug.WriteLine("DEBUG: GoToExtraActivitiesCommand ejecutado.");
                await NavigateToChildPage("seeExtra");
            });
        }

        private async Task NavigateToChildPage(string route)
        {
            Debug.WriteLine($"DEBUG: NavigateToChildPage llamado para la ruta: {route}");

            if (StudentId == 0)
            {
                Debug.WriteLine($"DEBUG: StudentId es cero, no se puede navegar.");
                await Application.Current.MainPage!.DisplayAlert("Error", "No se ha seleccionado un hijo.", "OK");
                return;
            }

            try
            {
                // Pasamos SIEMPRE studentId, schoolId y childName
                var safeName = Uri.EscapeDataString(ChildName ?? string.Empty);
                await Shell.Current.GoToAsync($"{route}?studentId={StudentId}&schoolId={ChildSchoolId}&childName={safeName}");

                Debug.WriteLine($"DEBUG: Navegación a {route}?studentId={StudentId}&schoolId={ChildSchoolId}&childName={safeName} iniciada.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error al navegar a la página '{route}': {ex.Message}");
                await Application.Current.MainPage!.DisplayAlert("Error", "No se pudo abrir esta sección: " + ex.Message, "OK");
            }
        }
    }
}