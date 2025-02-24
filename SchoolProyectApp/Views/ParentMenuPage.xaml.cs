using SchoolProyectApp.ViewModels;
using Microsoft.Maui.Controls;

namespace SchoolProyectApp.Views
{
    public partial class ParentMenuPage : ContentPage
    {
        public ParentMenuPage()
        {
            InitializeComponent();
            BindingContext = new ParentMenuViewModel();
        }
    }
}
