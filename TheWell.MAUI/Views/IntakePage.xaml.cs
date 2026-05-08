using TheWell.MAUI.ViewModels;

namespace TheWell.MAUI.Views;

public partial class IntakePage : ContentPage
{
    public IntakePage(IntakeViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
