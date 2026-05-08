namespace TheWell.Core.Interfaces;

public interface ILockRuleService
{
    bool IsLogEditable(DateOnly logDate);
    bool IsGoalLocked(DateTime goalCreatedAt);
}
