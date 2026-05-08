using TheWell.Core.Interfaces;

namespace TheWell.API.Services;

public class LockRuleService : ILockRuleService
{
    public bool IsLogEditable(DateOnly logDate) =>
        logDate >= DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-5);

    public bool IsGoalLocked(DateTime goalCreatedAt) =>
        DateTime.UtcNow >= goalCreatedAt.AddDays(5);
}
