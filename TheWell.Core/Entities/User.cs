namespace TheWell.Core.Entities;

public class User
{
    public Guid UserID { get; set; } = Guid.NewGuid();
    public string UniversityEID { get; set; } = string.Empty;       // AES-256 encrypted
    public string UniversityEIDHash { get; set; } = string.Empty;   // HMAC-SHA256 for lookup
    public string Email { get; set; } = string.Empty;               // AES-256 encrypted
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsPasswordResetRequired { get; set; } = true;
    public string AccountStatus { get; set; } = AccountStatuses.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid? ProvisionedBy { get; set; }

    public IntakeQuestion? IntakeQuestion { get; set; }
    public Goal? Goal { get; set; }
    public ICollection<DailyLog> DailyLogs { get; set; } = [];
    public ICollection<AuthenticationAudit> AuthAudits { get; set; } = [];
}

public static class AccountStatuses
{
    public const string Pending = "Pending";      // provisioned, temp password not yet changed
    public const string Active = "Active";
    public const string Graduation = "Graduation";
    public const string Suspended = "Suspended";
}
