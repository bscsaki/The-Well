using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TheWell.Core.DTOs;
using TheWell.MAUI.Services;

namespace TheWell.MAUI.ViewModels;

public partial class IntakeViewModel(ApiService api) : ObservableObject
{
    [ObservableProperty] private string _myHabit = "";
    [ObservableProperty] private string _myGoal = "";
    [ObservableProperty] private string _iAmPersonWho = "";
    [ObservableProperty] private string _strategy1 = "";
    [ObservableProperty] private string _strategy2 = "";
    [ObservableProperty] private string _toImproveMyselfIWill = "";
    [ObservableProperty] private string _rewardMyselfWith = "";
    [ObservableProperty] private string _peopleForEncouragement = "";
    [ObservableProperty] private string _errorMessage = "";
    [ObservableProperty] private bool _isBusy;

    [RelayCommand]
    private async Task SubmitAsync()
    {
        if (string.IsNullOrWhiteSpace(MyHabit) || string.IsNullOrWhiteSpace(MyGoal))
        {
            ErrorMessage = "Please fill in at least My Habit and My Goal.";
            return;
        }

        IsBusy = true;
        ErrorMessage = "";
        try
        {
            var request = new SubmitIntakeRequest(
                MyHabit, MyGoal, IAmPersonWho,
                Strategy1, Strategy2,
                ToImproveMyselfIWill, RewardMyselfWith,
                PeopleForEncouragement);

            var result = await api.SubmitIntakeAsync(request);
            if (result?.IsUnlocked == true)
                await Shell.Current.GoToAsync("//DashboardPage");
            else
                ErrorMessage = "Submission failed. Please try again.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error: {ex.Message}";
        }
        finally { IsBusy = false; }
    }
}
