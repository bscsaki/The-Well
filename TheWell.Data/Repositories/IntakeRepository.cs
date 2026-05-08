using Microsoft.EntityFrameworkCore;
using TheWell.Core.Entities;

namespace TheWell.Data.Repositories;

public class IntakeRepository(WellDbContext db)
{
    public async Task<IntakeQuestion?> GetByUserAsync(Guid userId) =>
        await db.IntakeQuestions.FirstOrDefaultAsync(i => i.UserID == userId);

    public async Task AddAsync(IntakeQuestion intake)
    {
        db.IntakeQuestions.Add(intake);
        await db.SaveChangesAsync();
    }

    public async Task SaveAsync() => await db.SaveChangesAsync();
}
