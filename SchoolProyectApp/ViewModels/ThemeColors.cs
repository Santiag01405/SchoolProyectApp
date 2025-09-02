// ThemeColors.cs
using Microsoft.Maui.Graphics;
using SchoolProyectApp.ViewModels;

namespace SchoolProyectApp.ViewModels
{
    public static class ThemeColors
    {
        // Colores extraídos del logo del Colegio "Santa Rosa"

        /// <summary>
        /// Azul oscuro, color principal del logo.
        /// </summary>
        public static Color PrimaryColor { get; } = Color.FromArgb("#222B5F");

        /// <summary>
        /// Color cerceta (Teal), un tono moderno y profesional.
        /// </summary>
        public static Color SecondaryColor { get; } = Color.FromArgb("#00897B");

        /// <summary>
        /// Amarillo dorado de las estrellas.
        /// </summary>
        public static Color AccentColor { get; } = Color.FromArgb("#FBC02D");

        /// <summary>
        /// Blanco para el fondo de las páginas.
        /// </summary>
        public static Color PageBackgroundColor { get; } = Color.FromArgb("#FFFFFF");

        /// <summary>
        /// Blanco para el texto sobre fondos oscuros.
        /// </summary>
        public static Color TextColor { get; } = Color.FromArgb("#FFFFFF");

        /// <summary>
        /// Color estándar para alertas o acciones de eliminación.
        /// </summary>
        public static Color DangerColor { get; } = Color.FromArgb("#FF0000");
    }
}