using System.Text.Json.Serialization;

namespace SchoolProyectApp.Models
{
    public class Child
    {
        [JsonPropertyName("userID")]
        public int UserID { get; set; }

        // Aquí usamos el nombre que viene de la API de hijos
        [JsonPropertyName("studentName")]
        public string StudentName { get; set; }
    }
}