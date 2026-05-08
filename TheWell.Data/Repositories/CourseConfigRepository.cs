using Microsoft.EntityFrameworkCore;
using TheWell.Core.Entities;

namespace TheWell.Data.Repositories;

public class CourseConfigRepository(WellDbContext db)
{
    public async Task<CourseConfig?> GetCurrentAsync() =>
        await db.CourseConfigs.OrderByDescending(c => c.SetAt).FirstOrDefaultAsync();

    public async Task SetAsync(CourseConfig config)
    {
        db.CourseConfigs.Add(config);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAllAsync()
    {
        db.CourseConfigs.RemoveRange(db.CourseConfigs);
        await db.SaveChangesAsync();
    }
}
