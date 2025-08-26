        // SchoolProyectApp/Views/EnrollStudentPage.xaml.cs
        namespace SchoolProyectApp.Views
        {
            public partial class EnrollStudentPage : ContentPage
            {
                public EnrollStudentPage()
                {
                    InitializeComponent();
                }

                private void OnGridTapped(object sender, EventArgs e)
                {
                    // Quita el foco del SearchBar para ocultar el teclado.
                    // Esto se activa al tocar en cualquier parte del Grid.
                    StudentSearchBar.Unfocus();
                }

                private async void AnimateButton(object sender, EventArgs e)
                {
                    if (sender is ImageButton button)
                    {
                        await button.ScaleTo(0.8, 100, Easing.CubicIn);
                        await button.ScaleTo(1, 100, Easing.CubicOut);
                    }
                }

                bool _isMenuVisible = false;

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