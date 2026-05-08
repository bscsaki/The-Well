namespace TheWell.Core.DTOs;

public record SubmitIntakeRequest(
    string MyHabit,
    string MyGoal,
    string IAmPersonWho,
    string Strategy1,
    string Strategy2,
    string ToImproveMyselfIWill,
    string RewardMyselfWith,
    string PeopleForEncouragement);

public record IntakeResponse(
    string MyHabit,
    string MyGoal,
    string IAmPersonWho,
    string Strategy1,
    string Strategy2,
    string ToImproveMyselfIWill,
    string RewardMyselfWith,
    string PeopleForEncouragement,
    bool IsUnlocked,
    DateTime? CompletedAt);
