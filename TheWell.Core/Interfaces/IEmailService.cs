namespace TheWell.Core.Interfaces;

public interface IEmailService
{
    Task SendOtpAsync(string toEmail, string otp);
    Task SendWelcomeEmailAsync(string toEmail, string eNumber, string tempPassword);
}
