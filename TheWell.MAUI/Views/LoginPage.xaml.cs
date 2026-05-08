using TheWell.MAUI.ViewModels;

namespace TheWell.MAUI.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
