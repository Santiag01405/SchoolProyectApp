using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using SchoolProyectApp.Models;

public class Evaluation
{
    // Propiedades obligatorias (no pueden ser null)
    public int EvaluationID { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; } // Ojo, Description también puede ser null en tu JSON
    public DateTime Date { get; set; }
    public int CourseID { get; set; }
    public int UserID { get; set; }
    public int SchoolID { get; set; }

    [JsonPropertyName("lapsoID")]
    public int? LapsoID { get; set; }

    // Propiedades que pueden ser null, usa el operador '?'
    public School? School { get; set; } // <-- Hacemos esta propiedad nullable
    public int? ClassroomID { get; set; } // <-- Hacemos esta propiedad nullable
    public Classroom? Classroom { get; set; } // <-- Hacemos esta propiedad nullable

    // Puedes agregar una propiedad de navegación para el curso
    [JsonPropertyName("course")]
    public Course Course { get; set; }
}
