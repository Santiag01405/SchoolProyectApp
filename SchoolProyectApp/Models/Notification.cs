namespace SchoolProyectApp.Models
{
    public class Notification
    {
        public int NotifyID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int UserID { get; set; }
        public bool IsRead { get; set; }
        public int SchoolID { get; set; }
        public School School { get; set; }
    }
}