using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TheWell.Core.DTOs;
using TheWell.MAUI.Services;
using TheWell.MAUI.Views;


namespace TheWell.MAUI.ViewModels;

public partial class LoginViewModel(ApiService api) : ObservableObject
{
    [ObservableProperty] private string _eNumber = "";
    [ObservableProperty] private string _password = "";
    [ObservableProperty] private string _errorMessage = "";
    [ObservableProperty] private bool _isBusy;

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(ENumber) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Please enter your E-number and password.";
            return;
        }

        IsBusy = true;
        ErrorMessage = "";

        try
        {
            var result = await api.LoginAsync(new LoginRequest(ENumber, Password));
            if (result is null)
            {
                ErrorMessage = "Invalid credentials. Please try again.";
                return;
            }

            if (result.IsPasswordResetRequired)
            {
                await Shell.Current.GoToAsync(nameof(ForceResetPage), new Dictionary<string, object>
                    { ["ENumber"] = ENumber });
                return;
            }

            if (result.AccountStatus == "Graduation")
            {
                await Shell.Current.GoToAsync("//GraduationPage");
                return;
            }

            await Shell.Current.GoToAsync(result.IsIntakeComplete ? "//DashboardPage" : "//IntakePage");
        }
        finally { IsBusy = false; }
    }

}
