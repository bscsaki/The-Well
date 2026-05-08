namespace TheWell.Core.Entities;

public class Goal
{
    public Guid GoalID { get; set; } = Guid.NewGuid();
    public Guid UserID { get; set; }
    public string GoalDefinition { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LockedAt { get; set; }  // set to CreatedAt + 5 days on create

    public User User { get; set; } = null!;
}
