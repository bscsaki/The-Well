using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TheWell.Core.DTOs;
using TheWell.MAUI.Services;

namespace TheWell.MAUI.ViewModels;

public partial class SettingsViewModel(ApiService api) : ObservableObject
{
    [ObservableProperty] private string _eNumber = "";
    [ObservableProperty] private string _universityEmail = "";
    [ObservableProperty] private string _currentPassword = "";
    [ObservableProperty] private string _newPassword = "";
    [ObservableProperty] private string _confirmPassword = "";
    [ObservableProperty] private string _message = "";
    [ObservableProperty] private bool _messageIsError;
    [ObservableProperty] private bool _isBusy;

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            var profile = await api.GetProfileAsync();
            if (profile is not null)
            {
                ENumber = profile.ENumber;
                UniversityEmail = profile.UniversityEmail;
            }
        }
        catch { }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task ChangePasswordAsync()
    {
        if (NewPassword != ConfirmPassword)
        {
            Message = "New passwords do not match.";
            MessageIsError = true;
            return;
        }
        if (NewPassword.Length < 8)
        {
            Message = "Password must be at least 8 characters.";
            MessageIsError = true;
            return;
        }

        IsBusy = true;
        Message = "";
        try
        {
            var (success, msg) = await api.ChangePasswordAsync(
                new ChangePasswordRequest(CurrentPassword, NewPassword));
            Message = msg;
            MessageIsError = !success;
            if (success) { CurrentPassword = ""; NewPassword = ""; ConfirmPassword = ""; }
        }
        catch (Exception ex) { Message = ex.Message; MessageIsError = true; }
        finally { IsBusy = false; }
    }
}
