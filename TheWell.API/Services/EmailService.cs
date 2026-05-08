using SendGrid;
using SendGrid.Helpers.Mail;
using TheWell.Core.Interfaces;

namespace TheWell.API.Services;

public class EmailService : IEmailService
{
    private readonly ISendGridClient _client;
    private readonly string _fromEmail;
    private readonly string _fromName;
    private readonly string? _welcomeTemplateId;

    public EmailService(ISendGridClient client, IConfiguration configuration)
    {
        _client = client;
        _fromEmail = configuration["SendGrid:FromEmail"] ?? "noreply@thewell.app";
        _fromName  = configuration["SendGrid:FromName"]  ?? "The Well";
        _welcomeTemplateId = configuration["SendGrid:WelcomeTemplateId"];
    }

    public async Task SendOtpAsync(string toEmail, string otp)
    {
        var msg = new SendGridMessage { From = new EmailAddress(_fromEmail, _fromName), Subject = "Your Well Password Reset Code" };
        msg.AddTo(new EmailAddress(toEmail));
        msg.HtmlContent = $"<p>Your 6-digit reset code is: <strong>{otp}</strong></p><p>This code expires in 15 minutes.</p>";
        var response = await _client.SendEmailAsync(msg);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Body.ReadAsStringAsync();
            throw new Exception($"SendGrid error {(int)response.StatusCode}: {body}");
        }
    }

    public async Task SendWelcomeEmailAsync(string toEmail, string eNumber, string tempPassword)
    {
        var msg = new SendGridMessage { From = new EmailAddress(_fromEmail, _fromName) };
        msg.AddTo(new EmailAddress(toEmail));

        if (!string.IsNullOrEmpty(_welcomeTemplateId))
        {
            msg.SetTemplateId(_welcomeTemplateId);
            msg.SetTemplateData(new { e_number = eNumber, temp_password = tempPassword });
        }
        else
        {
            msg.Subject = "Welcome to The Well";
            msg.HtmlContent = $"<p>Welcome to The Well!</p><p>Your E-number: <strong>{eNumber}</strong></p><p>Temporary password: <strong>{tempPassword}</strong></p><p>Please log in and change your password.</p>";
        }

        var response = await _client.SendEmailAsync(msg);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Body.ReadAsStringAsync();
            throw new Exception($"SendGrid error {(int)response.StatusCode}: {body}");
        }
    }
}
