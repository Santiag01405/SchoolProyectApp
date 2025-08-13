using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProyectApp.Models
{
    public class ExtracurricularAttendanceMarkDto
    {
        public int ActivityId { get; set; }
        public int RelatedUserId { get; set; }
        public int SchoolId { get; set; }
        public List<StudentAttendanceDto> StudentAttendance { get; set; }
    }
}
