using Microsoft.Maui.Controls;
using SchoolProyectApp.ViewModels;
using System.Threading.Tasks;

namespace SchoolProyectApp.Views
{
    public partial class CoursesPage : ContentPage
    {
        private readonly CoursesPageViewModel _viewModel;

        public CoursesPage()
        {
            InitializeComponent();
            _viewModel = new CoursesPageViewModel();
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await Task.Run(() => _viewModel.LoadCoursesCommand.Execute(null));
        }

        private void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            _viewModel.SearchText = e.NewTextValue;
            _viewModel.SearchCommand.Execute(null);
        }
        private async void AnimateButton(object sender, EventArgs e)
        {
            if (sender is ImageButton button)
            {
                await button.ScaleTo(0.8, 100, Easing.CubicIn);
                await button.ScaleTo(1, 100, Easing.CubicOut);
            }
        }
    }
}

