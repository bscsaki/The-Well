using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TheWell.Core.DTOs;
using TheWell.MAUI.Services;

namespace TheWell.MAUI.ViewModels;

[QueryProperty(nameof(ENumber), "ENumber")]
public partial class ForceResetViewModel(ApiService api) : ObservableObject
{
    [ObservableProperty] private string _eNumber = "";
    [ObservableProperty] private string _newPassword = "";
    [ObservableProperty] private string _confirmPassword = "";
    [ObservableProperty] private string _errorMessage = "";
    [ObservableProperty] private bool _isBusy;

    [RelayCommand]
    private async Task ResetPasswordAsync()
    {
        if (NewPassword != ConfirmPassword) { ErrorMessage = "Passwords do not match."; return; }
        if (NewPassword.Length < 8) { ErrorMessage = "Password must be at least 8 characters."; return; }

        IsBusy = true;
        ErrorMessage = "";
        try
        {
            var ok = await api.ForceResetAsync(new ForceResetRequest(ENumber, NewPassword));
            if (!ok) { ErrorMessage = "Reset failed. Please try again."; return; }
            await Shell.Current.GoToAsync("//LoginPage");
        }
        finally { IsBusy = false; }
    }
}
