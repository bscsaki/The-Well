using Microsoft.EntityFrameworkCore;
using TheWell.Core.Entities;

namespace TheWell.Data.Repositories;

public class AuditRepository(WellDbContext db)
{
    public async Task AddAsync(AuthenticationAudit audit)
    {
        db.AuthenticationAudits.Add(audit);
        await db.SaveChangesAsync();
    }

    public async Task<AuthenticationAudit?> GetLatestOtpAsync(Guid userId) =>
        await db.AuthenticationAudits
                .Where(a => a.UserID == userId && a.Action == AuditActions.OtpRequest && a.OtpHash != null)
                .OrderByDescending(a => a.AttemptTimestamp)
                .FirstOrDefaultAsync();

    public async Task<List<AuthenticationAudit>> GetAllAsync() =>
        await db.AuthenticationAudits
                .OrderByDescending(a => a.AttemptTimestamp)
                .ToListAsync();
}
