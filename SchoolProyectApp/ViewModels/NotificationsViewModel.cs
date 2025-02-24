using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using SchoolProyectApp.Models;

namespace SchoolProyectApp.ViewModels
{
    public class NotificationsViewModel
    {
        public ObservableCollection<Notification> Notifications { get; set; }

        public NotificationsViewModel()
        {
            // Aquí podrías cargar las notificaciones desde la API
            Notifications = new ObservableCollection<Notification>
            {
                new Notification { Title = "Examen Próximo", Description = "Tienes un examen la próxima semana." },
                new Notification { Title = "Nueva Calificación", Description = "Se ha publicado una nueva calificación en Matemáticas." }
            };
        }
    }
}
