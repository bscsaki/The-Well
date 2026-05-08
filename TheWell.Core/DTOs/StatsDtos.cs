namespace TheWell.Core.DTOs;

public record StatsResponse(
    int TotalCompleted,
    int CurrentStreak,
    double WellFillPercent);
