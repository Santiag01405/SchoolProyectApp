// ThemeColors.cs
using Microsoft.Maui.Graphics;
using SchoolProyectApp.ViewModels;

namespace SchoolProyectApp.ViewModels
{
    public static class ThemeColors
    {
        public static Color PrimaryColor { get; } = Color.FromArgb("#0d4483"); //"{x:Static vm:ThemeColors.PrimaryColor}"
        public static Color SecondaryColor { get; } = Color.FromArgb("#0098da");
        public static Color AccentColor { get; } = Color.FromArgb("#e99a27");
        public static Color PageBackgroundColor { get; } = Color.FromArgb("#FFFFFF");

        public static Color TextColor { get; } = Color.FromArgb("#FFFFFF");

        public static Color DangerColor { get; } = Color.FromArgb("#FF0000");
    }
}