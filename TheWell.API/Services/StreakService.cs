using TheWell.Core.Interfaces;
using TheWell.Data.Repositories;

namespace TheWell.API.Services;

public class StreakService(DailyLogRepository logRepo) : IStreakService
{
    public async Task<int> CalculateAsync(Guid userId)
    {
        var logs = await logRepo.GetByUserAsync(userId);
        if (logs.Count == 0) return 0;

        var completedDates = logs
            .Where(l => l.IsCompleted)
            .Select(l => l.LogDate)
            .ToHashSet();

        var streak = 0;
        var current = DateOnly.FromDateTime(DateTime.UtcNow);

        // Allow today or yesterday as the streak anchor (if today's log not yet submitted)
        if (!completedDates.Contains(current))
            current = current.AddDays(-1);

        while (completedDates.Contains(current))
        {
            streak++;
            current = current.AddDays(-1);
        }
        return streak;
    }
}
