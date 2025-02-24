namespace SchoolProyectApp.Models
{
    public class Role
    {
        public int RoleID { get; set; }  // 1 = Student, 2 = Parent, 3 = Teacher
        public string RoleName { get; set; } = string.Empty;
    }
}
