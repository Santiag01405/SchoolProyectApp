using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace SchoolProyectApp.Models
{
    public class AuthResponse
    {
        [JsonPropertyName("token")] // 🔹 Coincide con el JSON del backend
        public string Token { get; set; } = string.Empty;
    }
}

