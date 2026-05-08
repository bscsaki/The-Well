using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TheWell.MAUI.Services;

namespace TheWell.MAUI.ViewModels;

public partial class OtpViewModel(ApiService api) : ObservableObject
{
    [ObservableProperty] private string _email = "";
    [ObservableProperty] private string _otp = "";
    [ObservableProperty] private string _message = "";
    [ObservableProperty] private bool _otpSent;
    [ObservableProperty] private bool _isBusy;

    [RelayCommand]
    private async Task RequestOtpAsync()
    {
        if (string.IsNullOrWhiteSpace(Email)) return;
        IsBusy = true;
        try
        {
            await api.RequestOtpAsync(Email);
            OtpSent = true;
            Message = "If your email is registered, a 6-digit code was sent.";
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task VerifyOtpAsync()
    {
        if (string.IsNullOrWhiteSpace(Otp)) return;
        IsBusy = true;
        try
        {
            var result = await api.VerifyOtpAsync(Email, Otp);
            if (result is null)
            {
                Message = "Invalid or expired code.";
                return;
            }
            await Shell.Current.GoToAsync("//LoginPage");
        }
        finally { IsBusy = false; }
    }
}
