namespace SchoolProyectApp.Models
{
    public class Post
    {
        public string ProfileImage { get; set; } = string.Empty; // URL o nombre del recurso de la imagen de perfil
        public string Name { get; set; } = string.Empty; // Nombre del usuario que publica
        public string Subtitle { get; set; } = string.Empty; // Fecha u otra info adicional
        public string Message { get; set; } = string.Empty; // Contenido del post
        public string Title { get; set; } = "Example Title";
        public string Description { get; set; } = "Example Description";
    }
}