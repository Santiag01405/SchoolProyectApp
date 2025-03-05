using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace SchoolProyectApp.ViewModels
{
    public class EditSchedulePageViewModel : BaseViewModel
    {
        private string _selectedDay;
        private string _selectedTime;
        private string _selectedSubject;
        private ObservableCollection<string> _days;

        public ObservableCollection<string> Days
        {
            get => _days;
            set
            {
                _days = value;
                OnPropertyChanged(nameof(Days));
            }
        }

        public string SelectedDay
        {
            get => _selectedDay;
            set
            {
                _selectedDay = value;
                OnPropertyChanged(nameof(SelectedDay));
            }
        }

        public string SelectedTime
        {
            get => _selectedTime;
            set
            {
                _selectedTime = value;
                OnPropertyChanged(nameof(SelectedTime));
            }
        }

        public string SelectedSubject
        {
            get => _selectedSubject;
            set
            {
                _selectedSubject = value;
                OnPropertyChanged(nameof(SelectedSubject));
            }
        }

        public ICommand SaveScheduleCommand { get; }

        public EditSchedulePageViewModel()
        {
            Days = new ObservableCollection<string>
            {
                "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado"
            };

            SaveScheduleCommand = new Command(SaveSchedule);
        }

        private async void SaveSchedule()
        {
            if (string.IsNullOrEmpty(SelectedDay) || string.IsNullOrEmpty(SelectedTime) || string.IsNullOrEmpty(SelectedSubject))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Todos los campos son obligatorios.", "OK");
                return;
            }

            // Lógica para guardar el horario (puedes agregarlo a una base de datos o un servicio)
            await Application.Current.MainPage.DisplayAlert("Éxito", "Horario guardado correctamente.", "OK");

            // Regresar a la página anterior
            await Shell.Current.GoToAsync("..");
        }
    }
}
