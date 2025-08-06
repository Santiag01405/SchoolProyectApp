using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProyectApp.Views
{
    public partial class AssignGradesPage : ContentPage
    {
        private bool _isMenuVisible = false;
        public AssignGradesPage()
        {
            InitializeComponent();
        }
        private async void AnimateButton(object sender, EventArgs e)
        {
            if (sender is ImageButton button)
            {
                await button.ScaleTo(0.8, 100, Easing.CubicIn);
                await button.ScaleTo(1, 100, Easing.CubicOut);
            }
        }

        private async void MenuButton_Clicked(object sender, EventArgs e)
        {
            if (_isMenuVisible)
            {
                await SideMenu.TranslateTo(-260, 0, 250, Easing.CubicIn);
                SideMenu.IsVisible = false;
                _isMenuVisible = false;
            }
            else
            {
                SideMenu.IsVisible = true;
                await SideMenu.TranslateTo(0, 0, 250, Easing.CubicOut);
                _isMenuVisible = true;
            }
        }

    }
}

