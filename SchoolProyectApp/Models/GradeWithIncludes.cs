// SchoolProyectApp/Models/GradeWithIncludes.cs

using System.Text.Json.Serialization;

namespace SchoolProyectApp.Models
{
    public class GradeWithIncludes
    {
        [JsonPropertyName("gradeID")]
        public int GradeID { get; set; }

        [JsonPropertyName("userID")]
        public int UserID { get; set; }

        [JsonPropertyName("courseID")]
        public int CourseID { get; set; }

        [JsonPropertyName("evaluationID")]
        public int EvaluationID { get; set; }

        [JsonPropertyName("schoolID")]
        public int SchoolID { get; set; }

        [JsonPropertyName("gradeValue")]
        public double GradeValue { get; set; }

        [JsonPropertyName("comments")]
        public string Comments { get; set; }

        // Propiedades para los objetos anidados
        [JsonPropertyName("course")]
        public Course Course { get; set; }

        [JsonPropertyName("evaluation")]
        public Evaluation Evaluation { get; set; }
    }
}