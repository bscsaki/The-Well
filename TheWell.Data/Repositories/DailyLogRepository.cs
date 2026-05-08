using Microsoft.EntityFrameworkCore;
using TheWell.Core.Entities;

namespace TheWell.Data.Repositories;

public class DailyLogRepository(WellDbContext db)
{
    public async Task<List<DailyLog>> GetByUserAsync(Guid userId) =>
        await db.DailyLogs
                .Where(l => l.UserID == userId)
                .OrderByDescending(l => l.LogDate)
                .ToListAsync();

    public async Task<DailyLog?> FindAsync(Guid logId) =>
        await db.DailyLogs.FindAsync(logId);

    public async Task<bool> ExistsForDateAsync(Guid userId, DateOnly date) =>
        await db.DailyLogs.AnyAsync(l => l.UserID == userId && l.LogDate == date);

    public async Task AddAsync(DailyLog log)
    {
        db.DailyLogs.Add(log);
        await db.SaveChangesAsync();
    }

    public async Task SaveAsync() => await db.SaveChangesAsync();
}
