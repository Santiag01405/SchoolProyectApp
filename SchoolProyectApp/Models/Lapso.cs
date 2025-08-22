using System;
using System.Text.Json.Serialization;

namespace SchoolProyectApp.Models
{
    public class Lapso
    {
        [JsonPropertyName("lapsoID")]
        public int LapsoID { get; set; }

        [JsonPropertyName("nombre")]
        public string Nombre { get; set; }

        [JsonPropertyName("fechaInicio")]
        public DateTime FechaInicio { get; set; }

        [JsonPropertyName("fechaFin")]
        public DateTime FechaFin { get; set; }

        [JsonPropertyName("schoolID")]
        public int SchoolID { get; set; }
    }
}