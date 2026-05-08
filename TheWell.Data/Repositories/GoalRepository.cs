using Microsoft.EntityFrameworkCore;
using TheWell.Core.Entities;

namespace TheWell.Data.Repositories;

public class GoalRepository(WellDbContext db)
{
    public async Task<Goal?> GetByUserAsync(Guid userId) =>
        await db.Goals.FirstOrDefaultAsync(g => g.UserID == userId);

    public async Task AddAsync(Goal goal)
    {
        db.Goals.Add(goal);
        await db.SaveChangesAsync();
    }

    public async Task SaveAsync() => await db.SaveChangesAsync();
}
