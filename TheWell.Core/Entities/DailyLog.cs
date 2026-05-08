namespace TheWell.Core.Entities;

public class DailyLog
{
    public Guid LogID { get; set; } = Guid.NewGuid();
    public Guid UserID { get; set; }
    public DateOnly LogDate { get; set; }
    public bool IsCompleted { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsLocked { get; set; }  // true when (today - LogDate) > 5 days

    public User User { get; set; } = null!;
}
