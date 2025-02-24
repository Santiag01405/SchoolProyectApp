namespace SchoolProyectApp.Models
{
    public class User
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public int RoleID { get; set; }  // Ahora almacenamos el número del rol
    }
}


