using TheWell.MAUI.Controls;
using TheWell.MAUI.ViewModels;

namespace TheWell.MAUI.Views;

public partial class LogEntryPage : ContentPage
{
    private readonly LogEntryViewModel _vm;

    public LogEntryPage(LogEntryViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
        vm.ConfettiRequested += OnConfettiRequested;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.InitAsync();
        TodayLabel.Text = _vm.DateDisplay;
    }

    private void OnConfettiRequested()
    {
        ConfettiCanvas.IsVisible = true;
        ConfettiCanvas.TriggerConfetti();
    }
}
