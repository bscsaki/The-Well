using TheWell.MAUI.ViewModels;

namespace TheWell.MAUI.Views;

public partial class OtpPage : ContentPage
{
    public OtpPage(OtpViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
