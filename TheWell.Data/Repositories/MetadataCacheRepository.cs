using Microsoft.EntityFrameworkCore;
using TheWell.Core.Entities;

namespace TheWell.Data.Repositories;

public class MetadataCacheRepository(WellDbContext db)
{
    public async Task<MetadataCache?> GetValidAsync(string contentType) =>
        await db.MetadataCache
                .Where(m => m.ContentType == contentType && m.ExpiryDate > DateTime.UtcNow)
                .OrderByDescending(m => m.ExpiryDate)
                .FirstOrDefaultAsync();

    public async Task UpsertAsync(string contentType, string payload, TimeSpan ttl)
    {
        var existing = await db.MetadataCache
                               .FirstOrDefaultAsync(m => m.ContentType == contentType);
        if (existing is null)
        {
            db.MetadataCache.Add(new MetadataCache
            {
                ContentType = contentType,
                Payload = payload,
                ExpiryDate = DateTime.UtcNow.Add(ttl)
            });
        }
        else
        {
            existing.Payload = payload;
            existing.ExpiryDate = DateTime.UtcNow.Add(ttl);
        }
        await db.SaveChangesAsync();
    }

    public async Task ClearAllAsync()
    {
        db.MetadataCache.RemoveRange(db.MetadataCache);
        await db.SaveChangesAsync();
    }
}
