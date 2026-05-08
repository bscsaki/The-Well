using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TheWell.Core.DTOs;
using TheWell.MAUI.Services;

namespace TheWell.MAUI.ViewModels;

public partial class GoalViewModel(ApiService api) : ObservableObject
{
    [ObservableProperty] private IntakeResponse? _intake;
    [ObservableProperty] private bool _isBusy;

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        try { Intake = await api.GetIntakeAsync(); }
        catch { }
        finally { IsBusy = false; }
    }
}
