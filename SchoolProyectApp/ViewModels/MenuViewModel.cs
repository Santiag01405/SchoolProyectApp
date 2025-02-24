using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace SchoolProyectApp.ViewModels
{
    public class MenuViewModel : BaseViewModel
    {
        public ObservableCollection<MenuItemModel> MenuOptions { get; set; }
        public ICommand NavigateCommand { get; }

        public MenuViewModel()
        {
            MenuOptions = new ObservableCollection<MenuItemModel>();
            NavigateCommand = new Command<MenuItemModel>(NavigateToPage);
            LoadMenuItems();
        }

        private void LoadMenuItems()
        {
            // Obtener el rol del usuario desde SecureStorage o API
            int userRole = GetUserRole();

            if (userRole == 1) // Student
            {
                MenuOptions.Add(new MenuItemModel { Title = "Notificaciones", Icon = "notif_icon.png", Route = "notifications" });
                MenuOptions.Add(new MenuItemModel { Title = "Cursos", Icon = "courses_icon.png", Route = "courses" });
                MenuOptions.Add(new MenuItemModel { Title = "Calificaciones", Icon = "grades_icon.png", Route = "grades" });
            }
            else if (userRole == 2) // Parent
            {
                MenuOptions.Add(new MenuItemModel { Title = "Notificaciones", Icon = "notif_icon.png", Route = "notifications" });
                MenuOptions.Add(new MenuItemModel { Title = "Cursos", Icon = "courses_icon.png", Route = "courses" });
                MenuOptions.Add(new MenuItemModel { Title = "Calificaciones del estudiante", Icon = "grades_icon.png", Route = "studentgrades" });
            }
            else if (userRole == 3) // Teacher
            {
                MenuOptions.Add(new MenuItemModel { Title = "Notificaciones", Icon = "notif_icon.png", Route = "notifications" });
                MenuOptions.Add(new MenuItemModel { Title = "Cursos", Icon = "courses_icon.png", Route = "courses" });
                MenuOptions.Add(new MenuItemModel { Title = "Estudiantes", Icon = "students_icon.png", Route = "students" });
                MenuOptions.Add(new MenuItemModel { Title = "Asignar Calificaciones", Icon = "assigngrades_icon.png", Route = "assigngrades" });
            }
        }

        private int GetUserRole()
        {
            // Obtener el rol del usuario desde SecureStorage
            var roleString = SecureStorage.GetAsync("user_role").Result;
            return int.TryParse(roleString, out int role) ? role : 0;
        }

        private async void NavigateToPage(MenuItemModel menuItem)
        {
            if (menuItem != null)
            {
                await Shell.Current.GoToAsync(menuItem.Route);
            }
        }
    }

    public class MenuItemModel
    {
        public string Title { get; set; }
        public string Icon { get; set; }
        public string Route { get; set; }
    }
}
