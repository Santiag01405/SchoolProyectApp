using System.Text.Json.Serialization;

namespace SchoolProyectApp.Models
{
    public class GradeResult
    {
        public int GradeID { get; set; }
        public int UserID { get; set; }

        public string Estudiante { get; set; }
        public string Curso { get; set; }
        public int CourseID { get; set; }
        public string Evaluacion { get; set; }

        public decimal? GradeValue { get; set; }  // numérica

        [JsonPropertyName("gradeText")]
        public string? GradeText { get; set; }
        public string Comments { get; set; }
        public DateTime? Date { get; set; }       // opcional

        public string DisplayGrade { get; set; } = "—";
    }
}
