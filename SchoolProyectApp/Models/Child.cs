// SchoolProyectApp/Models/Child.cs
using System.Text.Json.Serialization;

namespace SchoolProyectApp.Models
{
    public class Child
    {
        [JsonPropertyName("studentUserID")] // si en API pusiste StudentUserID
        public int UserID { get; set; }

        [JsonPropertyName("studentName")]
        public string StudentName { get; set; }

        [JsonPropertyName("schoolID")]
        public int SchoolID { get; set; }

        [JsonPropertyName("schoolName")]
        public string? SchoolName { get; set; }
    }
}