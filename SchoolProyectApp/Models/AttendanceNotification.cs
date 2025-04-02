using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProyectApp.Models
{
    public class AttendanceNotification
    {
        public int AttendanceID { get; set; }
        public int UserID { get; set; }
        public string StudentName { get; set; }
        public int RelatedUserID { get; set; }
        public string TeacherName { get; set; }
        public int CourseID { get; set; }
        public string CourseName { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
    }

}
