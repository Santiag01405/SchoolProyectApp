using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace SchoolProyectApp.ViewModels
{
    public class StudentAttendanceModel : INotifyPropertyChanged
    {
        public int UserID { get; set; }
        public string StudentName { get; set; }

        private bool _isPresent;
        public bool IsPresent
        {
            get => _isPresent;
            set
            {
                if (_isPresent != value)
                {
                    _isPresent = value;
                    OnPropertyChanged(nameof(IsPresent));
                    OnPropertyChanged(nameof(IsAbsent));
                }
            }
        }
        public ICommand HomeCommand { get; }
        public ICommand FirstProfileCommand { get; }
        public ICommand OpenMenuCommand { get; }

        public StudentAttendanceModel() {

            HomeCommand = new Command(async () => await Shell.Current.GoToAsync("///homepage"));
            OpenMenuCommand = new Command(async () => await Shell.Current.GoToAsync("///menu"));
            FirstProfileCommand = new Command(async () => await Shell.Current.GoToAsync("///firtsprofile"));
        }
        public bool IsAbsent
        {
            get => !_isPresent;
            set => IsPresent = !value;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}