using Microsoft.Maui.Controls;
using System.Windows.Input;
using SchoolProyectApp.Models;
using System.Threading.Tasks;
using SchoolProyectApp.Services;

namespace SchoolProyectApp.Views
{
    public partial class EditCoursePopup : ContentPage
    {
        public Course EditableCourse { get; set; }
        public ICommand SaveCommand { get; }
        public ICommand CloseCommand { get; }

        public EditCoursePopup(Course course)
        {
            InitializeComponent();
            EditableCourse = new Course
            {
                CourseID = course.CourseID,
                Name = course.Name,
                Description = course.Description,
                TeacherID = course.TeacherID
            };

            SaveCommand = new Command(async () => await SaveChanges());
            CloseCommand = new Command(async () => await ClosePopup());

            BindingContext = this;
        }

        private async Task SaveChanges()
        {
            var apiService = new ApiService();
            bool success = await apiService.UpdateCourseAsync(EditableCourse);

            if (success)
            {
                await Application.Current.MainPage.Navigation.PopModalAsync(); // Cerrar popup
            }
            else
            {
                Console.WriteLine("❌ No se pudo actualizar el curso.");
            }
        }


        private async Task ClosePopup()
        {
            await Shell.Current.GoToAsync(".."); // Cerrar popup
        }
    }
}
