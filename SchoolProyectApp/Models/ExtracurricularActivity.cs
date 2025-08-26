using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProyectApp.Models
{
    public class ExtracurricularActivity
    {
        public int ActivityID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int UserID { get; set; }
        public int DayOfWeek { get; set; }
        public int SchoolID { get; set; }

        public string DayOfWeekString
        {
            get
            {
                return DayOfWeek switch
                {
                    1 => "Lunes",
                    2 => "Martes",
                    3 => "Miércoles",
                    4 => "Jueves",
                    5 => "Viernes",
                    6 => "Sábado",
                    7 => "Domingo",
                    _ => "Día no especificado",
                };
            }
        }
    }
}