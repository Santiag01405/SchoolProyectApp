namespace SchoolProyectApp.Models
{
    public class Course
    {
        public int CourseID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int TeacherID { get; set; }
    }
}