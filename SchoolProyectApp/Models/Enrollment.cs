namespace SchoolProyectApp.Models
{
    public class Enrollment
    {
        public int EnrollmentID { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; }
        public int CourseID { get; set; }
        public string CourseName { get; set; }
        public int? DayOfWeek { get; set; } // Puede ser null, lo hacemos nullable
        public string? StudentID { get; set; }
    }

}