using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public decimal? GradeValue { get; set; }
        public string Comments { get; set; }
    }
}

