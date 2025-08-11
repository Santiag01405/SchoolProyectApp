using System.Text.Json.Serialization;

namespace SchoolProyectApp.Models
{
    public class Student2
    {
        [JsonPropertyName("userID")]
        public int UserID { get; set; }

      
        public string StudentName { get; set; }

        [JsonPropertyName("roleID")]
        public int RoleID { get; set; }
    }
}
