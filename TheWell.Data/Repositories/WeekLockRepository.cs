using Microsoft.EntityFrameworkCore;
using TheWell.Core.Entities;

namespace TheWell.Data.Repositories;

public class WeekLockRepository(WellDbContext db)
{
    public async Task<List<WeekLock>> GetAllAsync() =>
        await db.WeekLocks.OrderBy(w => w.WeekNumber).ToListAsync();

    public async Task<WeekLock?> GetAsync(int weekNumber) =>
        await db.WeekLocks.FirstOrDefaultAsync(w => w.WeekNumber == weekNumber);

    public async Task SetLockAsync(int weekNumber, bool isLocked)
    {
        var existing = await db.WeekLocks.FirstOrDefaultAsync(w => w.WeekNumber == weekNumber);
        if (existing is null)
        {
            db.WeekLocks.Add(new WeekLock { WeekNumber = weekNumber, IsLocked = isLocked });
        }
        else
        {
            existing.IsLocked = isLocked;
        }
        await db.SaveChangesAsync();
    }
}
