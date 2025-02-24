namespace SchoolProyectApp.Models
{
    public class Parent
    {
        public int ParentID { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public int UserID { get; set; }
    }
}