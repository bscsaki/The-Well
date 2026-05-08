namespace TheWell.Core.Entities;

public class IntakeQuestion
{
    public Guid UserID { get; set; }
    public string MyHabit { get; set; } = string.Empty;
    public string MyGoal { get; set; } = string.Empty;
    public string IAmPersonWho { get; set; } = string.Empty;
    public string Strategy1 { get; set; } = string.Empty;
    public string Strategy2 { get; set; } = string.Empty;
    public string ToImproveMyselfIWill { get; set; } = string.Empty;
    public string RewardMyselfWith { get; set; } = string.Empty;
    public string PeopleForEncouragement { get; set; } = string.Empty;
    public bool IsUnlocked { get; set; } = false;
    public DateTime? CompletedAt { get; set; }

    public User User { get; set; } = null!;
}
