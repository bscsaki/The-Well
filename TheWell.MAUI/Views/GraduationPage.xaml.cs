using TheWell.MAUI.ViewModels;

namespace TheWell.MAUI.Views;

public partial class GraduationPage : ContentPage
{
    private readonly GraduationViewModel _vm;

    public GraduationPage(GraduationViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadCommand.ExecuteAsync(null);
    }
}
