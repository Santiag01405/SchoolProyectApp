﻿namespace SchoolProyectApp.Models
{
    public class Notification
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int UserID { get; set; }

    }
}