using SchoolProyectApp.ViewModels;
using Microsoft.Maui.Controls;

namespace SchoolProyectApp.Views
{
    public partial class RegisterPage : ContentPage
    {
        public RegisterViewModel _viewModel; 
        public RegisterPage()
        {
            InitializeComponent();
            _viewModel = new RegisterViewModel();
            BindingContext = _viewModel;
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.ResetFields();
        }
    }
}