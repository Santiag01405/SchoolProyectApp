﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProyectApp.Models
{
    public class Attendance
    {
        [Key]
        public int AttendanceID { get; set; }

        [Required]
        public int UserID { get; set; } // Estudiante al que se le toma asistencia
        public int RelatedUserID { get; set; } // Profesor o tutor que marca la asistencia

        [Required]
        public int CourseID { get; set; } // Curso al que pertenece la asistencia

        [Required]
        public DateTime Date { get; set; } = DateTime.UtcNow; // Fecha de la asistencia

        [Required]
        [StringLength(10)]
        public string Status { get; set; } // 'Presente' o 'Ausente'
    }
}

