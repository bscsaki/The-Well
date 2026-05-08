using TheWell.MAUI.ViewModels;

namespace TheWell.MAUI.Views;

public partial class DashboardPage : ContentPage
{
    private readonly DashboardViewModel _vm;

    public DashboardPage(DashboardViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadCommand.ExecuteAsync(null);
    }

    private async void OnCalendarDayTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is CalendarDay day)
            await _vm.DayTappedAsync(day);
    }
}
