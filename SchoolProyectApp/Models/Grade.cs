namespace SchoolProyectApp.Models
{
    public class Grade
    {
        public int GradeID { get; set; }
        public int UserID { get; set; }
        public int CourseID { get; set; }
        public int? EvaluationID { get; set; }
        public int SchoolID { get; set; }
        public decimal? GradeValue { get; set; }
        public string? Comments { get; set; }
    }

}
