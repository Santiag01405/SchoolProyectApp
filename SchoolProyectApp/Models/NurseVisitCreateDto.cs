// SchoolProyectApp/Models/NurseVisitCreateDto.cs
using System.ComponentModel.DataAnnotations;

namespace SchoolProyectApp.Models
{
    public class NurseVisitCreateDto
    {
        [Required]
        public int StudentUserID { get; set; }

        [Required]
        public string Reason { get; set; }

        public string Treatment { get; set; }
    }
}