namespace SchoolProyectApp.Models
{
    public class Student
    {
        public int StudentID { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Cedula { get; set; }
        public string? PhoneNumber { get; set; }
        public int? ParentID { get; set; }
        public int UserID { get; set; }
    }
}
