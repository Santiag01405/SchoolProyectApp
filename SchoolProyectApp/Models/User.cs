namespace SchoolProyectApp.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string UserName { get; set; } = string.Empty; 
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public int RoleID { get; set; }
        public int SchoolID { get; set; }
        public School School { get; set; }
        public int? ClassroomID { get; set; }

    }
}


