using TheWell.MAUI.ViewModels;

namespace TheWell.MAUI.Views;

public partial class ForceResetPage : ContentPage
{
    public ForceResetPage(ForceResetViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
