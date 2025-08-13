// SchoolProyectApp/ViewModels/StudentAttendanceModel.cs
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SchoolProyectApp.ViewModels
{
    public class StudentAttendanceModel : INotifyPropertyChanged
    {
        public int UserID { get; set; }
        public string UserName { get; set; }

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