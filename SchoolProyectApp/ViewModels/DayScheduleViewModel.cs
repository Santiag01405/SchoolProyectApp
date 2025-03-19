using System;
using System.Collections.ObjectModel;
using SchoolProyectApp.Models;

namespace SchoolProyectApp.ViewModels
{
    public class DaySchedule : IEquatable<DaySchedule>
    {
        public string DayOfWeek { get; set; }
        public ObservableCollection<Course> Courses { get; set; } = new ObservableCollection<Course>();

        // ✅ Comparación basada en DayOfWeek para evitar duplicados
        public override bool Equals(object obj)
        {
            return obj is DaySchedule schedule && schedule.DayOfWeek == this.DayOfWeek;
        }

        public bool Equals(DaySchedule other)
        {
            return other != null && other.DayOfWeek == this.DayOfWeek;
        }

        public override int GetHashCode()
        {
            return DayOfWeek.GetHashCode();
        }
    }
}
