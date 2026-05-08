namespace TheWell.Core.Entities;

public class AuthenticationAudit
{
    public Guid AuditID { get; set; } = Guid.NewGuid();
    public Guid UserID { get; set; }
    public string Action { get; set; } = string.Empty;  // 'Login', 'OtpRequest', 'OtpVerify', 'FailedLogin'
    public DateTime AttemptTimestamp { get; set; } = DateTime.UtcNow;
    public string? OtpHash { get; set; }
    public DateTime? OtpExpiresAt { get; set; }

    public User User { get; set; } = null!;
}

public static class AuditActions
{
    public const string Login = "Login";
    public const string FailedLogin = "FailedLogin";
    public const string OtpRequest = "OtpRequest";
    public const string OtpVerify = "OtpVerify";
    public const string PasswordReset = "PasswordReset";
}
