using TheWell.MAUI.ViewModels;

namespace TheWell.MAUI.Views;

public partial class SettingsPage : ContentPage
{
    private readonly SettingsViewModel _vm;
    public SettingsPage(SettingsViewModel vm) { InitializeComponent(); BindingContext = _vm = vm; }
    protected override async void OnAppearing() { base.OnAppearing(); await _vm.LoadCommand.ExecuteAsync(null); }
}
