using System.Text.Json.Serialization;

namespace SchoolProyectApp.Models
{
    public class Student
    {
        [JsonPropertyName("userID")]
        public int UserID { get; set; }

        [JsonPropertyName("userName")]
        public string StudentName { get; set; }  

        [JsonPropertyName("roleID")]
        public int RoleID { get; set; }
    }
}
