using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace SchoolProyectApp.ViewModels
{
    public class SchedulePageViewModel : BaseViewModel
    {
        private ObservableCollection<ScheduleItem> _selectedDaySchedule;
        private int _userRole;

        public bool CanEditSchedule
        {
            get => _userRole == 2; // 🔹 Ahora correctamente asignado a Teachers (rol 2)
        }

        public ObservableCollection<ScheduleItem> SelectedDaySchedule
        {
            get => _selectedDaySchedule;
            set
            {
                _selectedDaySchedule = value;
                OnPropertyChanged(nameof(SelectedDaySchedule));
            }
        }

        public ICommand SelectDayCommand { get; }
        public ICommand EditScheduleCommand { get; }

        public ICommand HomeCommand { get; }
        public ICommand ProfileCommand { get; }
        public ICommand OpenMenuCommand { get; }

        private readonly Dictionary<string, ObservableCollection<ScheduleItem>> _scheduleData;

        public SchedulePageViewModel()
        {
            // 🔹 Obtener el rol del usuario y forzar actualización
            _userRole = GetUserRole();
            OnPropertyChanged(nameof(CanEditSchedule)); // 🔹 Asegurar que se actualiza en la UI

            _scheduleData = new Dictionary<string, ObservableCollection<ScheduleItem>>
            {
                { "Lunes", new ObservableCollection<ScheduleItem> { new ScheduleItem { Time = "07:45AM - 09:15AM", Subject = "Matemáticas" } } },
                { "Martes", new ObservableCollection<ScheduleItem> { new ScheduleItem { Time = "07:45AM - 09:15AM", Subject = "Ciencias" } } },
                { "Miércoles", new ObservableCollection<ScheduleItem> { new ScheduleItem { Time = "07:45AM - 09:15AM", Subject = "Inglés" } } },
                { "Jueves", new ObservableCollection<ScheduleItem> { new ScheduleItem { Time = "07:45AM - 09:15AM", Subject = "API" } } },
                { "Viernes", new ObservableCollection<ScheduleItem> { new ScheduleItem { Time = "07:45AM - 09:15AM", Subject = "Filosofía" } } },
                { "Sábado", new ObservableCollection<ScheduleItem> { new ScheduleItem { Time = "07:45AM - 09:15AM", Subject = "Taller de Robótica" } } }
            };

            SelectedDaySchedule = new ObservableCollection<ScheduleItem>();
            SelectDayCommand = new Command<string>(SelectDay);
            EditScheduleCommand = new Command(EditSchedule, () => CanEditSchedule);

            SelectDay("Lunes"); // Cargar el primer día por defecto

            // Barra inferior
            HomeCommand = new Command(async () => await Shell.Current.GoToAsync("///homepage"));
            ProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///profile"));
            OpenMenuCommand = new Command(async () => await Shell.Current.GoToAsync("///menu"));
        }

        private int GetUserRole()
        {
            var roleString = SecureStorage.GetAsync("user_role").Result;
            return int.TryParse(roleString, out int role) ? role : 0;
        }

        private void SelectDay(string day)
        {
            if (_scheduleData.ContainsKey(day))
            {
                SelectedDaySchedule = new ObservableCollection<ScheduleItem>(_scheduleData[day]);
            }
        }

        private async void EditSchedule()
        {
            if (!CanEditSchedule)
                return;

            await Shell.Current.GoToAsync("///editschedule");
        }
    }

    public class ScheduleItem
    {
        public string? Time { get; set; }
        public string? Subject { get; set; }
    }
}



