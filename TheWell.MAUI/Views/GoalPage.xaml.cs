using TheWell.MAUI.ViewModels;

namespace TheWell.MAUI.Views;

public partial class GoalPage : ContentPage
{
    private readonly GoalViewModel _vm;

    public GoalPage(GoalViewModel vm)
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
